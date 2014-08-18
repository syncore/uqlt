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
using UQLT.Models.Configuration;
using UQLT.ViewModels;

namespace UQLT.Core.Modules.Chat
{
    /// <summary>
    /// Helper class that handles backend XMPP connection and related XMPP events for the buddy list (ChatListViewModel)
    /// </summary>
    public class ChatHandler
    {
        public static Hashtable ActiveChats = new Hashtable();
        public readonly string StrInvite = "UQLT-IncomingGameInvitation-";
        private readonly ConfigurationHandler _cfgHandler = new ConfigurationHandler();

        private readonly IEventAggregator _events;

        private readonly SoundPlayer _friendRequestSound =
            new SoundPlayer(UQltFileUtils.GetUqltChatSoundFilePath(ChatSoundTypes.FriendRequest));

        private readonly SoundPlayer _inviteSound =
            new SoundPlayer(UQltFileUtils.GetUqltChatSoundFilePath(ChatSoundTypes.InvitationSound));

        private readonly SoundPlayer _msgSound =
            new SoundPlayer(UQltFileUtils.GetUqltChatSoundFilePath(ChatSoundTypes.MessageSound));

        private readonly IWindowManager _windowManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHandler" /> class.
        /// </summary>
        /// <param name="clvm">The ChatListViewModel.</param>
        /// <param name="wm">The Window Manager.</param>
        /// <param name="events">The events.</param>
        public ChatHandler(ChatListViewModel clvm, IWindowManager wm, IEventAggregator events)
        {
            Clvm = clvm;
            _windowManager = wm;
            _events = events;
            XmppCon = new XmppClientConnection();

            // XmppClientConnection events
            XmppCon.OnLogin += XmppCon_OnLogin;
            XmppCon.OnRosterItem += XmppCon_OnRosterItem;
            XmppCon.OnRosterEnd += XmppCon_OnRosterEnd;
            XmppCon.OnPresence += XmppCon_OnPresence;
            XmppCon.OnMessage += XmppCon_OnMessage;
            XmppCon.ClientSocket.OnValidateCertificate += ClientSocket_OnValidateCertificate;

            PendingRequests = new Dictionary<string, FriendViewModel>();
            CurrentRosterItems = new Dictionary<string, RosterItem>();
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
        /// Gets the current roster from the server as a result of a manual roster request.
        /// </summary>
        /// <value>The current roster.</value>
        /// <remarks>This only occurrs when the roster is called using UpdateRoster method.</remarks>
        public Dictionary<string, RosterItem> CurrentRosterItems
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the pending (either outgoing/incoming) friend requests.
        /// </summary>
        /// <value>The pending requests.</value>
        public Dictionary<string, FriendViewModel> PendingRequests
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
        /// Accepts the friend request.
        /// </summary>
        /// <param name="jid">The jid.</param>
        public void AcceptFriendRequest(Jid jid)
        {
            // This is to enable auto-acceptance of new request.
            AddToPendingFriends(jid);
            // Accept request.
            XmppCon.PresenceManager.ApproveSubscriptionRequest(jid);
            // Request subscription from the other user.
            XmppCon.PresenceManager.Subscribe(jid);
            Debug.WriteLine("Accepted friend request from " + jid + " and also requested subscription from: " + jid);
        }

        /// <summary>
        /// Adds and subscribes to the friend.
        /// </summary>
        /// <param name="jid">The jid.</param>
        public void AddFriend(Jid jid)
        {
            XmppCon.RosterManager.AddRosterItem(jid);
            XmppCon.PresenceManager.Subscribe(jid);
            // Add friend to outgoing list, for the sake of the UI, even though at this point
            // SubscriptionType will be "none"
            AddToPendingFriends(jid);
            Debug.WriteLine("Successfully sent friend request to: " + jid);
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
            Clvm.OnlineGroup.Friends[friend].StatusType = ChatStatusTypes.Nothing;
        }

        /// <summary>
        /// Manually initiates a requests for the roster, currently for purposes of <see
        /// cref="AddFriendViewModel" /> to identify whether a contact already exists on the XMPP
        /// server roster.
        /// </summary>
        public void ManualRosterUpdateFromServer()
        {
            var iq = new RosterIq(IqType.get);
            XmppCon.IqGrabber.SendIq(iq, XmppCon_OnManualRosterUpdateRequest, null);
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
        public void PlayMessageSound(ChatSoundTypes soundtype)
        {
            if (!IsChatSoundEnabled()) { return; }

            try
            {
                switch (soundtype)
                {
                    case ChatSoundTypes.InvitationSound:
                        _inviteSound.Play();
                        break;

                    case ChatSoundTypes.MessageSound:
                        _msgSound.Play();
                        break;

                    case ChatSoundTypes.FriendRequest:
                        _friendRequestSound.Play();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Rejects the friend request.
        /// </summary>
        /// <param name="jid">The jid.</param>
        public void RejectFriendRequest(Jid jid)
        {
            XmppCon.PresenceManager.RefuseSubscriptionRequest(jid);
        }

        /// <summary>
        /// Removes the friend from the UQLT internal roster. This removes the friend from the
        /// Online, Offline and any other groups irrespective of the actual XMPP server roster.
        /// </summary>
        /// <param name="jid">The jid.</param>
        public void RemoveFriendFromInternalRosterGroups(Jid jid)
        {
            var friend = jid.User.ToLowerInvariant();
            FriendViewModel val;

            for (int i = 0; i < Clvm.BuddyList.Count; i++)
            {
                if (Clvm.BuddyList[i].Friends.TryGetValue(friend, out val))
                {
                    Execute.OnUIThread(() =>
                    {
                        Clvm.BuddyList[i].Friends.Remove(friend);
                    });
                }
            }
            Debug.WriteLine("Removed " + jid.User + " from our internal offline/online/pending groups.");
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
        /// Removes friend and unsubscribes from friend.
        /// </summary>
        /// <param name="jid">The jid.</param>
        /// <remarks>
        /// This is almost always paired with <see cref="RemoveFriendFromInternalRosterGroups" />
        /// </remarks>
        public void UnsubscribeAndRemoveFriend(Jid jid)
        {
            //XmppCon.PresenceManager.Unsubscribe(jid);
            XmppCon.RosterManager.RemoveRosterItem(jid);
            Debug.WriteLine("Removed friend and unsubscribed from: " + jid.User);
        }

        /// <summary>
        /// Adds an internal roster item (not UI roster groups).
        /// </summary>
        /// <param name="item">The roster item.</param>
        private void AddInternalRosterItem(RosterItem item)
        {
            var friend = item.Jid.User.ToLowerInvariant();
            RosterItem val;
            if (!CurrentRosterItems.TryGetValue(friend, out val))
            {
                //Debug.WriteLine("Manually adding: " + item.Jid + " to in-memory roster.");
                CurrentRosterItems.Add(friend, item);
            }
        }

        /// <summary>
        /// Adds the friend to the offline friend group if friend doesn't already exist within that group.
        /// </summary>
        /// <param name="jid">The jid.</param>
        private void AddOfflineFriend(Jid jid)
        {
            FriendViewModel val;
            string friend = jid.User.ToLowerInvariant();
            if (!Clvm.OfflineGroup.Friends.TryGetValue(friend, out val))
            {
                Execute.OnUIThread(() =>
                {
                    Clvm.OfflineGroup.Friends[friend] =
                        new FriendViewModel(new Friend(friend, IsFavoriteFriend(friend), false));
                });
            }
        }

        /// <summary>
        /// Adds the friend to the online friend group if friend doesn't already exist within that group.
        /// </summary>
        /// <param name="jid">The jid.</param>
        private void AddOnlineFriend(Jid jid)
        {
            FriendViewModel val;
            string friend = jid.User.ToLowerInvariant();
            if (!Clvm.OnlineGroup.Friends.TryGetValue(friend, out val))
            {
                Execute.OnUIThread(() =>
                {
                    Clvm.OnlineGroup.Friends[friend] =
                        new FriendViewModel(new Friend(friend, IsFavoriteFriend(friend), false));
                });
            }
        }

        /// <summary>
        /// Adds the friend to the outgoing friend group if friend doesn't already exist within that group.
        /// </summary>
        /// <param name="jid">The jid.</param>
        private void AddToPendingFriends(Jid jid)
        {
            FriendViewModel val;
            string friend = jid.User.ToLowerInvariant();
            if (!PendingRequests.TryGetValue(friend, out val))
            {
                Execute.OnUIThread(() =>
                {
                    PendingRequests[friend] =
                        new FriendViewModel(new Friend(friend, IsFavoriteFriend(friend), true));
                });
                Debug.WriteLine("*** Added " + jid + " to internal pending friend list");
            }
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
            Clvm.OnlineGroup.Friends[fromUser].StatusType = ChatStatusTypes.Nothing;
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
            var jid = presence.From;

            if (IsMe(jid)) { return; }

            string friend = presence.From.User.ToLowerInvariant();

            // Do not add outgoing friends who haven't accepted our request to the online group,
            // even though such friends will send an "available" PresenceType.
            //if (IsOutgoingFriend(jid)) { return; }

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
                    //Clvm.OnlineGroup.Friends[friend] = new FriendViewModel(new Friend(friend, IsFavoriteFriend(presence.From.User), false));
                    AddOnlineFriend(jid);
                    Clvm.OnlineGroup.Friends[friend].ActiveXmppResource = jid.Resource;
                    Clvm.OnlineGroup.Friends[friend].XmppResources.Add(jid.Resource);
                    Clvm.OnlineGroup.Friends[friend].HasMultipleXmppClients = false;
                    Clvm.OnlineGroup.Friends[friend].IsOnline = true;
                });

                // Note: OnRosterItem gets fired before OnPresence during initial login, however,
                // OnPresence gets fired before OnRosterItem when accepting the friend request.
                // Without this check then the accepted friend request will appear in both the
                // offline and online groups in our buddy list, assuming the friend request was
                // accepted while the friend was online:
                RemoveFromOfflineFriends(jid);
            }

            // Update with proper status. We only care about QL client since UQLT will always have a
            // blank status.
            if (jid.Resource.Equals(UQltGlobals.QuakeLiveXmppResource))
            {
                UpdateFriendStatus(friend, presence.Status);
            }

            // If user user was previously offline then remove from offline friends group.
            RemoveFromOfflineFriends(jid);
            Debug.WriteLine("Friends list before adding " + jid.User + "," + " count: " + Clvm.OnlineGroup.Friends.Count() + " After: " + Clvm.OnlineGroup.Friends.Count());
        }

        /// <summary>
        /// Friend has become unavailable.
        /// </summary>
        /// <param name="presence">The presence.</param>
        private void FriendBecameUnavailable(Presence presence)
        {
            var jid = presence.From;

            if (IsMe(jid)) { return; }

            string friend = jid.User.ToLowerInvariant();
            FriendViewModel val;

            // Do not add pending friends to the offline group, even though such friends may still
            // send an "unavailable" PresenceType.
            if (IsPendingFriend(jid)) { return; }

            if (!Clvm.OnlineGroup.Friends.TryGetValue(friend, out val)) return;

            // Check if friend has multiple clients.
            if (Clvm.OnlineGroup.Friends[friend].HasMultipleXmppClients)
            {
                // If the outgoing friend was a QL client then clear the status displayed in the
                // remaining UQLT client.
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
                // Completely remove from Online friends list, since user does not have multiple clients.
                RemoveFromOnlineFriends(jid);
                AddOfflineFriend(jid);

                Debug.WriteLine("[FRIEND UNAVAILABLE]: " + " Jid: " + jid + " User: " + jid.User + " Resource: " + jid.Resource);
                Debug.WriteLine("Friends list before removing " + jid.User + "," + " count: " + Clvm.OnlineGroup.Friends.Count());
            }
        }

        /// <summary>
        /// Handles the incoming friend request.
        /// </summary>
        /// <param name="jid">The jid.</param>
        private void FriendRequestReceived(Jid jid)
        {
            // If the user is pending then auto-accept the friend request so window doesn't show.
            if (IsPendingFriend(jid))
            {
                AcceptFriendRequest(jid);
            }
            else
            {
                var incoming = new FriendRequestViewModel(jid, this, _windowManager);
                dynamic settings = new ExpandoObject();
                settings.Topmost = true;
                settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                Execute.OnUIThread(() =>
                {
                    PlayMessageSound(ChatSoundTypes.FriendRequest);
                    _windowManager.ShowWindow(incoming, null, settings);
                });
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
            return (Clvm.FavoriteFriends.Contains(friend));
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
        /// Determines whether the specified friend's acecptance status is pending (outgoing friend).
        /// </summary>
        /// <param name="jid">The jid.</param>
        /// <returns><c>true</c> if the acceptance status is pending, otherwise <c>false</c>.</returns>
        /// <remarks>
        /// This applies to friend requests that we have sent, but the other user has not confirmed yet
        /// </remarks>
        private bool IsPendingFriend(Jid jid)
        {
            FriendViewModel val;
            string friend = jid.User.ToLowerInvariant();
            return (PendingRequests.TryGetValue(friend, out val));
        }

        /// <summary>
        /// Removes the friend from the offline group if friend exists within that group.
        /// </summary>
        /// <param name="jid">The jid.</param>
        private void RemoveFromOfflineFriends(Jid jid)
        {
            FriendViewModel val;
            var friend = jid.User.ToLowerInvariant();
            if (Clvm.OfflineGroup.Friends.TryGetValue(friend, out val))
            {
                Execute.OnUIThread(() => Clvm.OfflineGroup.Friends.Remove(friend));
            }
        }

        /// <summary>
        /// Removes the friend from the online group if friend exists within that group.
        /// </summary>
        /// <param name="jid">The jid.</param>
        private void RemoveFromOnlineFriends(Jid jid)
        {
            FriendViewModel val;
            var friend = jid.User.ToLowerInvariant();
            if (Clvm.OnlineGroup.Friends.TryGetValue(friend, out val))
            {
                Execute.OnUIThread(() => Clvm.OnlineGroup.Friends.Remove(friend));
            }
        }

        /// <summary>
        /// Removes the friend from the outgoing group if friend exists within that group.
        /// </summary>
        /// <param name="jid">The jid.</param>
        private void RemoveFromPendingFriends(Jid jid)
        {
            FriendViewModel val;
            var friend = jid.User.ToLowerInvariant();
            if (!PendingRequests.TryGetValue(friend, out val)) return;
            Execute.OnUIThread(() => PendingRequests.Remove(friend));
            Debug.WriteLine("*** Removed " + jid + " from internal pending friend list");
        }

        /// <summary>
        /// Removes the friend from internal roster items.
        /// </summary>
        /// <param name="item">The item.</param>
        private void RemoveInternalRosterItem(RosterItem item)
        {
            var friend = item.Jid.User.ToLowerInvariant();
            RosterItem val;
            if (CurrentRosterItems.TryGetValue(friend, out val))
            {
                //Debug.WriteLine("Manually removing: " + item.Jid + " from in-memory roster.");
                CurrentRosterItems.Remove(friend);
            }
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
                        Clvm.OnlineGroup.Friends[friend].StatusType = ChatStatusTypes.WatchingDemo;
                        Clvm.OnlineGroup.Friends[friend].HasXmppStatus = true;
                    }
                    // friend is actually in game
                    else
                    {
                        Clvm.OnlineGroup.Friends[friend].StatusType = ChatStatusTypes.PlayingRealGame;
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
                    Clvm.OnlineGroup.Friends[friend].StatusType = ChatStatusTypes.PlayingPracticeGame;
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
        /// Called when a manual roster request <see cref="ManualRosterUpdateFromServer" /> is initiated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="iq">The iq.</param>
        /// <param name="data">The data.</param>
        private void XmppCon_OnManualRosterUpdateRequest(object sender, IQ iq, object data)
        {
            Debug.WriteLine("Manual roster request received...");

            var r = iq.Query as Roster;
            if (r == null) return;

            foreach (var i in r.GetRoster())
            {
                AddInternalRosterItem(i);
            }
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
                var cm = new ChatMessageViewModel(msg.From, XmppCon, this, _windowManager, _events);
                dynamic settings = new ExpandoObject();
                settings.WindowStartupLocation = WindowStartupLocation.Manual;

                Execute.OnUIThread(() =>
                {
                    _windowManager.ShowWindow(cm, null, settings);
                    // Async: suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
                    var m = cm.MessageIncoming(msg);
                    if (msg.Body.StartsWith(StrInvite))
                    {
                        const ChatSoundTypes sound = ChatSoundTypes.InvitationSound;
                        PlayMessageSound(sound);
                    }
                    else
                    {
                        const ChatSoundTypes sound = ChatSoundTypes.MessageSound;
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
                    RemoveFromPendingFriends(presence.From);
                    break;

                case PresenceType.unavailable:
                    Debug.WriteLine("--> Got unavailable presence from: " + presence.From);
                    FriendBecameUnavailable(presence);
                    break;

                case PresenceType.subscribe:
                    Debug.WriteLine("--> Incoming friend request from: " + presence.From);
                    FriendRequestReceived(presence.From);
                    break;

                case PresenceType.subscribed:
                    // User has accepted our friend request. Now automatically accept his in order
                    // to prevent the incoming message from displaying (mutual subscription on
                    // client side).
                    AcceptFriendRequest(presence.From);
                    Debug.WriteLine("Presence type 'subscribed' received from: " + presence.From);
                    break;

                case PresenceType.unsubscribe:
                    // Friend has removed us from his roster. Remove him from ours as well.
                    XmppCon.RosterManager.RemoveRosterItem(presence.From);
                    RemoveFriendFromInternalRosterGroups(presence.From);
                    Debug.WriteLine("Presence type 'unsubscribe' received from: " + presence.From);
                    break;

                case PresenceType.unsubscribed:
                    // We've denied the incoming friend request. There's really no need to show any
                    // indication of this.
                    Debug.WriteLine("Presence type 'unsubscribed' received from " + presence.From);
                    break;
            }
        }

        /// <summary>
        /// Called when the full XMPP roster has been received from server.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <remarks>
        /// Note: OnRosterItem gets fired before OnPresence during initial login, however,
        /// OnPresence gets fired before OnRosterItem when accepting the friend request. This has
        /// implications for auto-accepting pending friend requests.
        /// </remarks>
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
        /// <remarks>
        /// <c>SubscriptionType.remove</c> - server needs to remove roster item
        /// See: http://xmpp.org/rfcs/rfc3921.html - section 8.6
        /// <c>SubscriptionType.both</c> - both the user and the contact have subscriptions to each
        /// other's presence information
        /// See: http://xmpp.org/rfcs/rfc3921.html - section 7.1
        /// <c>SubscriptionType.none</c> - the user does not have a subscription to the contact's
        /// presence information, and the contact does not have a subscription to the user's
        /// presence information.
        /// See: http://xmpp.org/rfcs/rfc3921.html - section 7.1
        /// <c>SubscriptionType.from</c> - the contact has a subscription to the user's presence
        /// information, but the user does not have a subscription to the contact's presence information.
        /// See: http://xmpp.org/rfcs/rfc3921.html - section 7.1
        /// <c>SubscriptionType.to</c> - the user has a subscription to the contact's presence
        /// information, but the contact does not have a subscription to the user's presence information.
        /// See: http://xmpp.org/rfcs/rfc3921.html - section 7.1
        /// </remarks>
        private void XmppCon_OnRosterItem(object sender, RosterItem item)
        {
            var friend = item.Jid.User.ToLowerInvariant();
            FriendViewModel val;

            // Update internal rosteritems
            AddInternalRosterItem(item);

            if (item.Subscription == SubscriptionType.remove)
            {
                // Remove friend from actual XMPP server roster.
                UnsubscribeAndRemoveFriend(item.Jid);
                // Remove friend from internal rosteritems
                RemoveInternalRosterItem(item);
                // Remove friend from our internal roster (offline/online/pending groups).
                RemoveFriendFromInternalRosterGroups(item.Jid);
            }

            if (item.Subscription == SubscriptionType.both)
            {
                // Not online then add to offline (not a typo, prevent "double" additition to lists)
                if (!Clvm.OnlineGroup.Friends.TryGetValue(friend, out val))
                {
                    Execute.OnUIThread(() =>
                    {
                        Clvm.OfflineGroup.Friends[friend] =
                            new FriendViewModel(new Friend(friend, IsFavoriteFriend(item.Jid.User), false));
                    });
                }
            }

            if (item.Subscription == SubscriptionType.none)
            {
                Debug.WriteLine("Subscription type of \"none\" received for: " + item.Jid);
                // We've sent a request to subscribe to other user's presence but have not received
                // an answer from other user yet.
                if (item.Ask == AskType.subscribe)
                {
                    Debug.WriteLine("Still waiting for friend request acceptance from: " + item.Jid);
                    AddToPendingFriends(item.Jid);
                }
                // We've sent a request to unsubscribe to other user's presence but have not
                // received an answer from other user yet. (i.e. user was offline when we requested
                // the unsubscription). Do nothing with this.
                if (item.Ask == AskType.unsubscribe)
                {
                    Debug.WriteLine("Subscription type is none, but now we are dealing with an unanswered unsubscription request (AskType.unsubscribe)" +
                                    " from: " + item.Jid);
                }
            }

            if (item.Subscription == SubscriptionType.from)
            {
                // Friend requests that the other user has sent us but we have not accepted yet.
                // User SubscriptionType.from typically occurs when user A sends a friend request to
                // user B; B accepts, but A is not online to auto-confirm. QL (and UQLT) will
                // auto-confirm SubscriptionType.from -> SubscriptionType.both, when A comes back
                // online, so at this point, add to Offline contacts.

                Debug.WriteLine("Subscription type of \"from\" received for: " + item.Jid);
                AddToPendingFriends(item.Jid);

                // But first, check online group to prevent user being duplicated in both offline
                // and online group.
                if (!Clvm.OnlineGroup.Friends.TryGetValue(friend, out val))
                {
                    Execute.OnUIThread(() =>
                    {
                        Clvm.OfflineGroup.Friends[friend] =
                            new FriendViewModel(new Friend(friend, IsFavoriteFriend(item.Jid.User), false));
                    });
                }
            }

            // Friend requests we've sent but have not been accepted by the other user yet.
            if (item.Subscription == SubscriptionType.to)
            {
                Debug.WriteLine("Subscription type of \"to\" received for: " + item.Jid);
                AddToPendingFriends(item.Jid);
            }
        }
    }
}