using System;
using System.Collections;
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
using UQLT.Models.Chat;
using UQLT.Models.Configuration;
using UQLT.ViewModels;

namespace UQLT.Core.Chat
{
    /// <summary>
    /// Helper class that handles backend XMPP connection and related XMPP events for the buddy list (ChatListViewModel)
    /// </summary>
    public class ChatHandler
    {
        public static Hashtable ActiveChats = new Hashtable();
        public readonly string StrInvite = "UQLT-IncomingGameInvitation-";
        private readonly ConfigurationHandler _cfgHandler = new ConfigurationHandler();
        private readonly SoundPlayer _inviteSound = new SoundPlayer(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\sounds\\invite.wav"));
        private readonly SoundPlayer _msgSound = new SoundPlayer(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\sounds\\notice.wav"));
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHandler" /> class.
        /// </summary>
        /// <param name="clvm">The ChatListViewModel.</param>
        /// <param name="wm">The Window Manager.</param>
        public ChatHandler(ChatListViewModel clvm, IWindowManager wm)
        {
            Clvm = clvm;
            _windowManager = wm;
            XmppCon = new XmppClientConnection();

            // XmppClientConnection event handlers
            XmppCon.OnLogin += XmppCon_OnLogin;
            XmppCon.OnRosterItem += XmppCon_OnRosterItem;
            // TODO: will probably need an OnRosterEnd event when Roster is fully loaded
            XmppCon.OnRosterEnd += XmppCon_OnRosterEnd;
            XmppCon.OnPresence += XmppCon_OnPresence;
            XmppCon.OnMessage += XmppCon_OnMessage;
            XmppCon.ClientSocket.OnValidateCertificate += ClientSocket_OnValidateCertificate;

            ConnectToXmpp();
            ChatGameInfo = new ChatGameInfo(this);
        }

        /// <summary>
        /// Gets or sets the chat game information.
        /// </summary>
        /// <value>The chat game information.</value>
        public ChatGameInfo ChatGameInfo { get; set; }

        /// <summary>
        /// Gets the ChatListViewModel
        /// </summary>
        /// <value>The ChatListViewModel</value>
        public ChatListViewModel Clvm
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the XMPP connection
        /// </summary>
        /// <value>The XMPP connection</value>
        public XmppClientConnection XmppCon { get; set; }

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
        public void PlayMessageSound(SoundTypes soundtype)
        {
            if (!IsChatSoundEnabled()) { return; }

            try
            {
                if (soundtype == SoundTypes.InvitationSound)
                {
                    _inviteSound.Play();
                }
                else
                {
                    _msgSound.Play();
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
            var toUser = recipient.User.ToLowerInvariant();
            if (!IsFriendAlreadyOnline(toUser)) { return false; }
            var msg = new agsXMPP.protocol.client.Message { Type = MessageType.chat };

            // Send to friend's multiple clients (UQLT + QL & others)
            foreach (var resource in Clvm.OnlineGroup.Friends[toUser].XmppResources)
            {
                msg.To = new Jid(recipient.Bare + "/" + resource);
                msg.Body = message;
                XmppCon.Send(msg);
                Debug.WriteLine("Sending message to " + recipient.Bare + "/" + resource);
            }

            return true;
        }

        /// <summary>
        /// Clears a friend's status.
        /// </summary>
        /// <param name="presence">The presence.</param>
        private void ClearFriendStatus(Presence presence)
        {
            FriendViewModel val;
            var fromUser = presence.From.User.ToLowerInvariant();
            // On program start there is a timing issue where the key won't exist, so need to check first
            if (!Clvm.OnlineGroup.Friends.TryGetValue(fromUser, out val)) return;
            Clvm.OnlineGroup.Friends[fromUser].HasXmppStatus = false;
            Clvm.OnlineGroup.Friends[fromUser].IsInGame = false;
            Clvm.OnlineGroup.Friends[fromUser].StatusType = StatusTypes.Nothing;
        }

        /// <summary>
        /// Clears a friend's status.
        /// </summary>
        /// <param name="friend">The friend (jid.From.User)</param>
        public void ClearFriendStatus(string friend)
        {
            FriendViewModel val;
            friend = friend.ToLowerInvariant();
            // On program start there is a timing issue where the key won't exist, so need to check first
            if (!Clvm.OnlineGroup.Friends.TryGetValue(friend, out val)) return;
            Clvm.OnlineGroup.Friends[friend].HasXmppStatus = false;
            Clvm.OnlineGroup.Friends[friend].IsInGame = false;
            Clvm.OnlineGroup.Friends[friend].StatusType = StatusTypes.Nothing;
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
        private void ConnectToXmpp()
        {
            //XmppCon.Username = ***REMOVED***;
            XmppCon.Username = ***REMOVED***;
            //XmppCon.Password = ***REMOVED***;
            XmppCon.Password = ***REMOVED***;
            XmppCon.Server = UQltGlobals.QlXmppDomain;
            XmppCon.Port = 5222;
            XmppCon.Resource = "uqlt";
            //XmppCon.Priority = 12;
            //XmppCon.Priority = 0;
            XmppCon.AutoRoster = true;
            XmppCon.AutoPresence = true;
            XmppCon.Open();
        }

        /// <summary>
        /// Friend has become available.
        /// </summary>
        /// <param name="presence">The user's presence.</param>
        private void FriendBecameAvailable(Presence presence)
        {
            string friend = presence.From.User.ToLowerInvariant();
            var jid = presence.From;

            if (IsMe(jid)) { return; }

            // User was already online.
            if ((IsFriendAlreadyOnline(friend)))
            {
                if (!Clvm.OnlineGroup.Friends[friend].ActiveXmppResource.Equals(jid.Resource))
                {
                    Clvm.OnlineGroup.Friends[friend].HasMultipleXmppClients = true;
                    Clvm.OnlineGroup.Friends[friend].ActiveXmppResource = jid.Resource;
                    Clvm.OnlineGroup.Friends[friend].XmppResources.Add(jid.Resource);

                    Debug.WriteLine("**********" + jid.User +
                                    " (status: " + presence.Status + ") already had a client signed in! Setting multiple client flag and new resource...");
                }
            }
            // User wasn't already online.
            else
            {
                Debug.WriteLine("**********" + jid + " (status: " + presence.Status + ") did NOT already have a client signed in. Updating Online Friends roster group...");

                // Must be done on the UI thread
                Execute.OnUIThread(() =>
                {
                    Clvm.OnlineGroup.Friends[friend] = new FriendViewModel(new Friend(friend, IsFavoriteFriend(presence.From.User)));
                    Clvm.OnlineGroup.Friends[friend].ActiveXmppResource = jid.Resource;
                    Clvm.OnlineGroup.Friends[friend].XmppResources.Add(jid.Resource);
                    Clvm.OnlineGroup.Friends[friend].HasMultipleXmppClients = false;
                    Clvm.OnlineGroup.Friends[friend].IsOnline = true;
                });
            }

            // Update with proper status. We only care about QL client since UQLT will always have a blank status.
            if (jid.Resource.Equals(UQltGlobals.QuakeLiveXmppResource))
            {
                UpdateFriendStatus(friend, presence.Status);
            }

            // If user user was previously offline then remove from offline friends group.
            FriendViewModel val;
            if (!Clvm.OfflineGroup.Friends.TryGetValue(friend, out val)) return;
            // Additions and removals to ObservableDictionary must be done on the UI thread
            Execute.OnUIThread(() => Clvm.OfflineGroup.Friends.Remove(friend));
            Debug.WriteLine("Friends list before adding " + jid.User + "," + " count: " + Clvm.OnlineGroup.Friends.Count() + " After: " + Clvm.OnlineGroup.Friends.Count());
        }

        /// <summary>
        /// Friend has become unavailable.
        /// </summary>
        /// <param name="presence">The presence.</param>
        private void FriendBecameUnavailable(Presence presence)
        {
            var jid = presence.From;
            string friend = jid.User.ToLowerInvariant();

            if (IsMe(jid)) { return; }

            FriendViewModel val;
            if (!Clvm.OnlineGroup.Friends.TryGetValue(friend, out val)) return;

            // Check if friend has multiple clients.
            if (Clvm.OnlineGroup.Friends[friend].HasMultipleXmppClients)
            {
                // If the outgoing friend was a QL client then clear the status displayed in the remaining UQLT client.
                if (jid.Resource.Equals(UQltGlobals.QuakeLiveXmppResource))
                {
                    ClearFriendStatus(presence);
                }

                // Handle the multiple resource situation.
                Debug.WriteLine(jid.User + " had multiple clients signed in! Removing client with resource: " + jid.Resource);
                Clvm.OnlineGroup.Friends[friend].HasMultipleXmppClients = false;
                Clvm.OnlineGroup.Friends[friend].XmppResources.Remove(jid.Resource);

                // Set resource back to first client's resource.
                if (Clvm.OnlineGroup.Friends[friend].XmppResources.Count < 1) return;
                Clvm.OnlineGroup.Friends[friend].ActiveXmppResource = Clvm.OnlineGroup.Friends[friend].XmppResources.ElementAt(0);
                Debug.WriteLine(jid.Bare + "'s new resource is: " +
                                Clvm.OnlineGroup.Friends[friend].XmppResources
                                    .ElementAt(0));
            }
            else
            {
                // Completely remove from Online friends list, since user does not have multiple
                // clients. Additions and removals to ObservableDictionary must be done on the UI thread
                Execute.OnUIThread(() =>
                {
                    Clvm.OnlineGroup.Friends.Remove(friend);
                    Clvm.OfflineGroup.Friends[friend] = new FriendViewModel(new Friend(friend, IsFavoriteFriend(jid.User)));
                    Clvm.OfflineGroup.Friends[friend].IsOnline = false;
                });

                Debug.WriteLine("[FRIEND UNAVAILABLE]: " + " Jid: " + jid + " User: " + jid.User + " Resource: " + jid.Resource);
                Debug.WriteLine("Friends list before removing " + jid.User + "," + " count: " + Clvm.OnlineGroup.Friends.Count());
            }
        }

        /// <summary>
        /// Determines whether the chat sound is enabled?
        /// </summary>
        /// <returns><c>true</c> if the chat sound is enabled, otherwise <c>false</c>.</returns>
        private bool IsChatSoundEnabled()
        {
            _cfgHandler.ReadConfig();
            return (_cfgHandler.ChatOptSound);
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
            friend = friend.ToLowerInvariant();
            return (UQltGlobals.SavedFavoriteFriends.Contains(friend));
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
            friend = friend.ToLowerInvariant();
            return (Clvm.OnlineGroup.Friends.TryGetValue(friend, out val));
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
            friend = friend.ToLowerInvariant();
            if (string.IsNullOrEmpty(status))
            {
                ClearFriendStatus(friend);
                return;
            }
            try
            {
                var statusinfo = JsonConvert.DeserializeObject<StatusInfo>(status);

                // BOT_GAME = 0: friend could either be watching a demo (ADDRESS = "bot") or
                // actually in a real server (ADDRESS = "ip:port")
                if (statusinfo.bot_game == 0)
                {
                    // friend is watching a demo
                    if (statusinfo.address.Equals("bot"))
                    {
                        Clvm.OnlineGroup.Friends[friend].StatusType = StatusTypes.WatchingDemo;
                        Clvm.OnlineGroup.Friends[friend].HasXmppStatus = true;
                    }
                    // friend is actually in game
                    else
                    {
                        Clvm.OnlineGroup.Friends[friend].StatusType = StatusTypes.PlayingRealGame;
                        Clvm.OnlineGroup.Friends[friend].HasXmppStatus = true;
                        Clvm.OnlineGroup.Friends[friend].IsInGame = true;
                        // query API to get type, map, location, player count info for status message
                        // Async: suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
                        var c = ChatGameInfo.CreateServerInfoForStatusAsync(friend, statusinfo.server_id);
                    }
                }

                // BOT_GAME = 1: friend is in a practice game or training match (we don't care about
                // ADDRESS, but it will be = "loopback")
                if (statusinfo.bot_game == 1)
                {
                    Clvm.OnlineGroup.Friends[friend].StatusType = StatusTypes.PlayingPracticeGame;
                    Clvm.OnlineGroup.Friends[friend].HasXmppStatus = true;
                }
            }
            catch (Exception e)
            {
                // Clear status on error.
                ClearFriendStatus(friend);
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
            var fromUser = msg.From.Bare.ToLowerInvariant();
            if (!ActiveChats.ContainsKey(fromUser))
            {
                var cm = new ChatMessageViewModel(msg.From, XmppCon, this, _windowManager);
                dynamic settings = new ExpandoObject();
                settings.WindowStartupLocation = WindowStartupLocation.Manual;

                Execute.OnUIThread(() =>
                {
                    _windowManager.ShowWindow(cm, null, settings);
                    // Async: suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
                    var m = cm.MessageIncoming(msg);
                    if (msg.Body.StartsWith(StrInvite))
                    {
                        const SoundTypes sound = SoundTypes.InvitationSound;
                        PlayMessageSound(sound);
                    }
                    else
                    {
                        const SoundTypes sound = SoundTypes.ChatSound;
                        PlayMessageSound(sound);
                    }
                });
            }
            else
            {
                Debug.WriteLine("A chat window already exists for: " + fromUser + " not opening another.");
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
                    FriendBecameAvailable(presence);
                    break;

                case PresenceType.unavailable:
                    Debug.WriteLine("--> Got unavailable presence from: " + presence.From);
                    FriendBecameUnavailable(presence);
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
            var friend = item.Jid.User.ToLowerInvariant();
            try
            {
                // Additions and removals to ObservableDictionary must be done on the UI thread
                // since ObservableDictionary is databound
                Execute.OnUIThread(() =>
                {
                    Clvm.OfflineGroup.Friends[friend] = new FriendViewModel(new Friend(friend, IsFavoriteFriend(item.Jid.User)));
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}