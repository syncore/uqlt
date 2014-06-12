using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Media;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Helpers;
using UQLT.Models.Chat;
using UQLT.ViewModels;

namespace UQLT.Core.Chat
{
    /// <summary>
    /// Helper class that handles backend XMPP connection and related XMPP events for the buddy list (ChatListViewModel)
    /// </summary>
    public class ChatHandler
    {
        public static Hashtable ActiveChats = new Hashtable();
        private readonly IWindowManager _windowManager;
        private ChatGameInfo _qlChatGameInfo;
        private XmppClientConnection _xmppCon;
        private SoundPlayer inviteSound = new SoundPlayer(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\sounds\\invite.wav"));
        private SoundPlayer msgSound = new SoundPlayer(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\sounds\\notice.wav"));
        private QLFormatHelper QLFormatter = QLFormatHelper.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHandler" /> class.
        /// </summary>
        /// <param name="clvm">The ChatListViewModel.</param>
        /// <param name="wm">The Window Manager.</param>
        public ChatHandler(ChatListViewModel clvm, IWindowManager wm)
        {
            CLVM = clvm;
            _windowManager = wm;
            _xmppCon = new XmppClientConnection();

            // XmppClientConnection event handlers
            XmppCon.OnLogin += new ObjectHandler(XmppCon_OnLogin);
            XmppCon.OnRosterItem += new XmppClientConnection.RosterHandler(XmppCon_OnRosterItem);
            // TODO: will probably need an OnRosterEnd event when Roster is fully loaded
            XmppCon.OnRosterEnd += new ObjectHandler(XmppCon_OnRosterEnd);
            XmppCon.OnPresence += new PresenceHandler(XmppCon_OnPresence);
            XmppCon.OnMessage += new MessageHandler(XmppCon_OnMessage);
            XmppCon.ClientSocket.OnValidateCertificate += new RemoteCertificateValidationCallback(ClientSocket_OnValidateCertificate);

            ConnectToXMPP();
            _qlChatGameInfo = new ChatGameInfo(this);
        }

        /// <summary>
        /// Gets or sets the chat game information.
        /// </summary>
        /// <value>The chat game information.</value>
        public ChatGameInfo ChatGameInfo
        {
            get
            {
                return _qlChatGameInfo;
            }
            set
            {
                _qlChatGameInfo = value;
            }
        }

        /// <summary>
        /// Gets the ChatListViewModel
        /// </summary>
        /// <value>The ChatListViewModel</value>
        public ChatListViewModel CLVM
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the XMPP connection
        /// </summary>
        /// <value>The XMPP connection</value>
        public XmppClientConnection XmppCon
        {
            get
            {
                return _xmppCon;
            }
            set
            {
                _xmppCon = value;
            }
        }

        /// <summary>
        /// Returns the currently logged in user's user name.
        /// </summary>
        /// <returns>
        /// Current logged in user's user name (just the "user" user@server part of the Jid)
        /// </returns>
        public string MyJidUser()
        {
            return XmppCon.MyJID.User.ToLowerInvariant();
        }

        /// <summary>
        /// Plays the chat notification sound.
        /// </summary>
        /// <remarks>
        /// if <c>type == 1</c> play normal message sound if <c>type == 2</c> play game invitation sound
        /// </remarks>
        public void PlayMessageSound(int type)
        {
            try
            {
                if (type == 2)
                {
                    inviteSound.Play();
                }
                else
                {
                    msgSound.Play();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Sends an XMPP message to another user.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public bool SendMessage(Jid recipient, string message)
        {

            if (IsFriendAlreadyOnline(recipient.User.ToLowerInvariant()))
            {
                var msg = new agsXMPP.protocol.client.Message();
                msg.Type = MessageType.chat;

                // Send to friend's multiple clients (UQLT + QL & others)
                foreach (var resource in CLVM.OnlineGroup.Friends[recipient.User.ToLowerInvariant()].XMPPResources)
                {
                msg.To = new Jid(recipient.Bare + "/" + resource);
                msg.Body = message;
                XmppCon.Send(msg);
                Debug.WriteLine("Sending message to " + recipient.Bare + "/" + resource);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks a friend's status to determine what the friend is doing in Quake Live.
        /// </summary>
        /// <param name="presence">The presence.</param>
        /// <remarks>
        /// This is only fired when availability changes (user: offline -&gt; online OR user leaves
        /// game server AND/OR joins game server).
        /// </remarks>
        private void CheckFriendStatus(Presence presence)
        {
            if (IsMe(presence.From)) return;
            
            if (string.IsNullOrEmpty(presence.Status))
            {
                ClearFriendStatus(presence);
                Debug.WriteLine("**Status for " + presence.From.User.ToLowerInvariant() + " is empty.");
            }
            else
            {
                CLVM.OnlineGroup.Friends[presence.From.User.ToLowerInvariant()].HasXMPPStatus = true;
                UpdateFriendStatus(presence.From.User, presence.Status);
                Debug.WriteLine("**Status for " + presence.From.User.ToLowerInvariant() + " is: " + presence.Status);
            }
        }

        /// <summary>
        /// Clears a friend's status.
        /// </summary>
        /// <param name="presence">The presence.</param>
        private void ClearFriendStatus(Presence presence)
        {
            FriendViewModel val;
            // On program start there is a timing issue where the key won't exist, so need to check first
            if (!CLVM.OnlineGroup.Friends.TryGetValue(presence.From.User.ToLowerInvariant(), out val)) return;
            CLVM.OnlineGroup.Friends[presence.From.User.ToLowerInvariant()].HasXMPPStatus = false;
            CLVM.OnlineGroup.Friends[presence.From.User.ToLowerInvariant()].IsInGame = false;
            CLVM.OnlineGroup.Friends[presence.From.User.ToLowerInvariant()].StatusType = TypeOfStatus.Nothing;
        }

        /// <summary>
        /// Called when a valid certificate is received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="chain">The chain.</param>
        /// <param name="sslPolicyErrors">The SSL policy errors.</param>
        /// <returns>true</returns>
        private bool ClientSocket_OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Connects to XMPP
        /// </summary>
        private void ConnectToXMPP()
        {
            XmppCon.Username = ***REMOVED***;
            //XmppCon.Username = ***REMOVED***;
            XmppCon.Password = ***REMOVED***;
            //XmppCon.Password = ***REMOVED***;
            XmppCon.Server = UQLTGlobals.QLXMPPDomain;
            XmppCon.Port = 5222;
            //XmppCon.Resource = "uqlt";
            //XmppCon.Priority = 12;
            //XmppCon.Priority = 0;
            XmppCon.AutoRoster = true;
            XmppCon.AutoPresence = true;
            XmppCon.Open();
        }

        /// <summary>
        /// Friend has become available.
        /// </summary>
        /// <param name="jid">The user's jid.</param>
        private void FriendBecameAvailable(Jid jid)
        {
            if (IsMe(jid)) return;

            // User was already online.
            if (IsFriendAlreadyOnline(jid.User.ToLowerInvariant()))
            {
                CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].HasMultipleXMPPClients = true;
                CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].ActiveXMPPResource = jid.Resource;
                CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].XMPPResources.Add(jid.Resource);
                Debug.WriteLine("**********" + jid.User + " already had a client signed in! Setting multiple client flag and new resource...");
            }
            // User wasn't already online.
            else
            {
                Debug.WriteLine("**********" + jid + " did NOT already have a client signed in. Updating Online Friends roster group...");
                // Must be done on the UI thread
                Execute.OnUIThread(() =>
                {
                    CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()] = new FriendViewModel(new Friend(jid.User.ToLowerInvariant(), IsFavoriteFriend(jid.User)));
                    CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].ActiveXMPPResource = jid.Resource;
                    CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].XMPPResources.Add(jid.Resource);
                    CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].HasMultipleXMPPClients = false;
                    CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].IsOnline = true;
                });

            }

            // If user user was previously offline then remove from offline friends group.        
            FriendViewModel val;
            if (CLVM.OfflineGroup.Friends.TryGetValue(jid.User.ToLowerInvariant(), out val))
            {
                // Additions and removals to ObservableDictionary must be done on the UI thread
                Execute.OnUIThread(() => CLVM.OfflineGroup.Friends.Remove(jid.User.ToLowerInvariant()));
                Debug.WriteLine("Friends list before adding " + jid.User + "," + " count: " + CLVM.OnlineGroup.Friends.Count() + " After: " + CLVM.OnlineGroup.Friends.Count());
            }
        }

        /// <summary>
        /// Friend has become unavailable.
        /// </summary>
        /// <param name="jid">The user's jid.</param>
        private void FriendBecameUnavailable(Jid jid)
        {
            if (IsMe(jid)) return;

            FriendViewModel val;
            if (!CLVM.OnlineGroup.Friends.TryGetValue(jid.User.ToLowerInvariant(), out val)) return;

            // Check if friend has multiple clients.
            if (CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].HasMultipleXMPPClients)
            {
                Debug.WriteLine(jid.User+ " had multiple clients signed in! Removing client with resource: " + jid.Resource);

                CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].HasMultipleXMPPClients = false;
                CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].XMPPResources.Remove(jid.Resource);

                // Set resource back to first client's resource.
                if (CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].XMPPResources.Count < 1) return;
                CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].ActiveXMPPResource = CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].XMPPResources.ElementAt(0);
                Debug.WriteLine(jid.Bare + "'s new resource is: " +
                                CLVM.OnlineGroup.Friends[jid.User.ToLowerInvariant()].XMPPResources
                                    .ElementAt(0));
            }
            else
            {
                // Completely remove from Online friends list, since user does not have multiple clients.
                // Additions and removals to ObservableDictionary must be done on the UI thread
                Execute.OnUIThread(() =>
                {
                    CLVM.OnlineGroup.Friends.Remove(jid.User.ToLowerInvariant());
                    CLVM.OfflineGroup.Friends[jid.User.ToLowerInvariant()] = new FriendViewModel(new Friend(jid.User.ToLowerInvariant(), IsFavoriteFriend(jid.User)));
                    CLVM.OfflineGroup.Friends[jid.User.ToLowerInvariant()].IsOnline = false;
                });

                Debug.WriteLine("[FRIEND UNAVAILABLE]: " + " Jid: " + jid + " User: " + jid.User + " Resource: " + jid.Resource);
                Debug.WriteLine("Friends list before removing " + jid.User + "," + " count: " + CLVM.OnlineGroup.Friends.Count());
            }
        }

        /// <summary>
        /// Determines whether the specified friend is a favorite friend].
        /// </summary>
        /// <param name="friend">The friend.</param>
        /// <returns>
        /// true if the friend is a favorite friend. false if the friend is not a favorite friend.
        /// </returns>
        private bool IsFavoriteFriend(string friend)
        {
            return (UQLTGlobals.SavedFavoriteFriends.Contains(friend.ToLowerInvariant()));
        }

        /// <summary>
        /// Determines whether the specified friend is already online.
        /// </summary>
        /// <param name="friend">The friend.</param>
        /// <returns>
        /// true if the friend is already online. false if the friend is not already online.
        /// </returns>
        private bool IsFriendAlreadyOnline(string friend)
        {
            FriendViewModel val;
            return (CLVM.OnlineGroup.Friends.TryGetValue(friend.ToLowerInvariant(), out val));
        }

        /// <summary>
        /// Determines whether the specified presence is from me, the currently logged in user.
        /// </summary>
        /// <param name="jid">The user's jid.</param>
        /// <returns>
        /// true if the presence is from the currently logged in user. false if the presence is not
        /// from the currently logged in user.
        /// </returns>
        private bool IsMe(Jid jid)
        {
            return (jid.Bare.Equals(XmppCon.MyJID.Bare.ToLowerInvariant()));
        }


        /// <summary>
        /// Updates a friend's status based on the activity the friend is performing. Meaning that
        /// the friend has an XMPP status, which further means one of three things: The user is
        /// watching a demo, playing a practice match, or is actually in a game server.
        /// </summary>
        /// <param name="friend">The friend.</param>
        /// <param name="status">The status.</param>
        /// <remarks>
        /// This method will determine which of the three possible activities the friend is doing in
        /// Quake Live and update his status accordingly.
        /// </remarks>
        private void UpdateFriendStatus(string friend, string status)
        {
            try
            {
                StatusInfo si = JsonConvert.DeserializeObject<StatusInfo>(status);

                // BOT_GAME = 0: friend could either be watching a demo (ADDRESS = "bot") or
                // actually in a real server (ADDRESS = "ip:port")
                if (si.bot_game == 0)
                {
                    // friend is watching a demo
                    if (si.address.Equals("bot"))
                    {
                        CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusType = TypeOfStatus.WatchingDemo;
                    }
                    // friend is actually in game
                    else
                    {
                        CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusType = TypeOfStatus.PlayingRealGame;
                        CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].IsInGame = true;
                        // query API to get type, map, location, player count info for status message
                        var c = ChatGameInfo.CreateServerInfoForStatusAsync(friend.ToLowerInvariant(), si.server_id);
                    }
                }

                // BOT_GAME = 1: friend is in a practice game or training match (we don't care about
                // ADDRESS, but it will be = "loopback")
                if (si.bot_game == 1)
                {
                    CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusType = TypeOfStatus.PlayingPracticeGame;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// Called upon successful authentication to XMPP server.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void XmppCon_OnLogin(object sender)
        {
        }

        /// <summary>
        /// Called when the user receives an XMPP message.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The message.</param>
        private void XmppCon_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            if (msg.Body == null) return;
            if (!ActiveChats.ContainsKey(msg.From.Bare.ToLowerInvariant()))
            {
                var cm = new ChatMessageViewModel(msg.From, XmppCon, this, _windowManager);
                dynamic settings = new ExpandoObject();
                settings.WindowStartupLocation = WindowStartupLocation.Manual;

                Execute.OnUIThread(() =>
                {
                    _windowManager.ShowWindow(cm, null, settings);
                    cm.MessageIncoming(msg);
                    if (msg.Body.StartsWith("Join me in this QUAKE LIVE match:"))
                    {
                        PlayMessageSound(2);
                    }
                    else
                    {
                        PlayMessageSound(1);
                    }
                });
            }
            else
            {
                Debug.WriteLine("A chat window already exists for: " + msg.From.Bare.ToLowerInvariant() + " not opening another.");
            }
        }

        /// <summary>
        /// Called when a presence has been received from a contact. This also handles contact subscriptions.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="presence">The presence.</param>
        private void XmppCon_OnPresence(object sender, Presence presence)
        {
            switch (presence.Type)
            {
                case PresenceType.available:
                    Debug.WriteLine("--> Got available presence from: " + presence.From);
                    FriendBecameAvailable(presence.From);
                    CheckFriendStatus(presence);
                    break;

                case PresenceType.unavailable:
                    Debug.WriteLine("--> Got unavailable presence from: " + presence.From);
                    FriendBecameUnavailable(presence.From);
                    break;

                case PresenceType.subscribe:
                    // Show a message indicating that friend request was sent to another user
                    break;

                case PresenceType.subscribed:
                    // Show a dialog that allows the user to accept or reject the incoming friend request
                    break;

                case PresenceType.unsubscribe:
                    // Show a message indicating that user successfully un-friended another user
                    break;

                case PresenceType.unsubscribed:
                    // Show a message indicating that the user chose to reject the incoming friend request
                    break;
            }
        }

        /// <summary>
        /// Called when the full XMPP roster has been received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void XmppCon_OnRosterEnd(object sender)
        {
            // Start timer to refresh in-game friends' server information
            ChatGameInfo.StartServerUpdateTimer();
        }

        /// <summary>
        /// Called whenever a roster item is received via XMPP
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="item">The roster item.</param>
        private void XmppCon_OnRosterItem(object sender, RosterItem item)
        {
            // TODO: if (item.Subscription != SubscriptionType.remove) stuff
            try
            {
                // Additions and removals to ObservableDictionary must be done on the UI thread
                // since ObservableDictionary is databound
                Execute.OnUIThread(() =>
                {
                    CLVM.OfflineGroup.Friends[item.Jid.User.ToLowerInvariant()] = new FriendViewModel(new Friend(item.Jid.User.ToLowerInvariant(), IsFavoriteFriend(item.Jid.User)));
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}