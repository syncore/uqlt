﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using agsXMPP;
using agsXMPP.Collections;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP.Xml.Dom;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Helpers;
using UQLT.Models.Chat;
using UQLT.Models.QuakeLiveAPI;
using UQLT.ViewModels;

namespace UQLT.Core.Chat
{
    // Helper class that handles XMPP connection and related XMPP events for the ChatListViewModel

    public class QLChatConnection
    {
        private XmppClientConnection XmppCon;
        private QLFormatHelper QLFormatter = QLFormatHelper.Instance;
        private Timer GameServerUpdateTimer;

        public ChatListViewModel CLVM
        {
            get;
            private set;
        }

        public Dictionary<string, string> Roster { get; set; }

        public QLChatConnection(ChatListViewModel clvm)
        {
            CLVM = clvm;
            Roster = new Dictionary<string, string>();
            XmppCon = new XmppClientConnection();
            GameServerUpdateTimer = new Timer();

            // XmppClientConnection event handlers
            XmppCon.OnLogin += new ObjectHandler(XmppCon_OnLogin);
            XmppCon.OnRosterItem += new XmppClientConnection.RosterHandler(XmppCon_OnRosterItem);
            // TODO: will probably need an OnRosterEnd event when Roster is fully loaded
            XmppCon.OnRosterEnd += new ObjectHandler(XmppCon_OnRosterEnd);
            XmppCon.OnPresence += new PresenceHandler(XmppCon_OnPresence);
            XmppCon.ClientSocket.OnValidateCertificate += new RemoteCertificateValidationCallback(ClientSocket_OnValidateCertificate);

            ConnectToXMPP();
        }

        private void ConnectToXMPP()
        {
            XmppCon.Username = ***REMOVED***;
            XmppCon.Password = ***REMOVED***;
            XmppCon.Server = UQLTGlobals.QLXMPPDomain;
            XmppCon.Port = 5222;
            XmppCon.Resource = "quakelive";
            XmppCon.AutoRoster = true;
            XmppCon.AutoPresence = true;
            XmppCon.Open();
        }

        private void XmppCon_OnRosterItem(object sender, RosterItem item)
        {
            // TODO: if (item.Subscription != SubscriptionType.remove) stuff
            try
            {
                Roster.Add(item.Jid.Bare.ToLowerInvariant(), item.Jid.User.ToLowerInvariant());
                XmppCon.MessageGrabber.Add(new Jid(item.Jid.Bare.ToLowerInvariant()), new BareJidComparer(), new MessageCB(XmppCon_OnMessage), null);

                // Additions and removals to ObservableDictionary must be done on the UI thread
                Execute.OnUIThread(() =>
                {
                    CLVM.OfflineGroup.Friends[item.Jid.User.ToLowerInvariant()] = new FriendViewModel(new Friend(item.Jid.User.ToLowerInvariant(), IsFavoriteFriend(item.Jid.User)));
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                //MessageBox.Show(ex.ToString());
            }
        }

        // Roster has been fully loaded
        private void XmppCon_OnRosterEnd(object sender)
        {
            // Start timer to refresh in-game friends' server information
            StartServerUpdateTimer();
        }

        // We've received a message.
        private void XmppCon_OnMessage(object sender, agsXMPP.protocol.client.Message msg, object data)
        {
            if (msg.Body != null)
            {
                MessageBox.Show(Roster[msg.From.Bare.ToLowerInvariant()] + ": " + msg.Body);
            }
        }

        // We have successfully authenticated to the server.
        private void XmppCon_OnLogin(object sender)
        {
        }

        private void StartServerUpdateTimer()
        {
            GameServerUpdateTimer.Elapsed += new ElapsedEventHandler(OnTimedServerInfoUpdate);
            GameServerUpdateTimer.Interval = 90000;
            GameServerUpdateTimer.Enabled = true;
            GameServerUpdateTimer.AutoReset = true;
        }

        private void StopServerUpdateTimer()
        {
            // TODO: stop timer if we have launched a game, to prevent lag during game
            GameServerUpdateTimer.Enabled = false;
        }

        private void OnTimedServerInfoUpdate(object source, ElapsedEventArgs e)
        {
            List<string> ingamefriends = new List<string>();
            //TODO: only send one request, not 30 to QL API
            foreach (KeyValuePair<string, FriendViewModel> kvp in CLVM.OnlineGroup.Friends)
            {
                if (kvp.Value.IsInGame)
                {
                    ingamefriends.Add(kvp.Key);
                }
                else
                {
                    Debug.WriteLine("" + kvp.Key + "is not in a game server. Skipping...");
                }
            }
            Debug.WriteLine("Processing batch game server update for " + ingamefriends.Count + " players: " + string.Join(",", ingamefriends));
            // Batch process these in game friends
            GetServerInfoForStatus(ingamefriends);
        }

        // We've received a presence from a contact. Subscriptions are also handled in this event.
        private void XmppCon_OnPresence(object sender, Presence pres)
        {
            switch (pres.Type)
            {
                case PresenceType.available:
                    FriendBecameAvailable(pres);
                    break;

                case PresenceType.unavailable:
                    FriendBecameUnavailble(pres);
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

        // Check a user's status to determine what the user is doing in QL
        // Only fired when availability changes (user: offline -> online OR leave game server <-> join game server)
        private void CheckPlayerStatus(Presence pres)
        {
            if (string.IsNullOrEmpty(pres.Status))
            {
                ClearPlayerStatus(pres);
                //TODO
                // if player is in list of in game players to be updated every X (90? 120?) seconds, then remove him
                Debug.WriteLine("**Status for " + pres.From.User.ToLowerInvariant() + " is empty.");
            }
            else
            {
                CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].HasXMPPStatus = true;
                UpdatePlayerStatus(pres.From.User, pres.Status);
                Debug.WriteLine("**Status for " + pres.From.User.ToLowerInvariant() + " is: " + pres.Status);
            }
        }

        private void UpdatePlayerStatus(string friend, string status)
        {
            try
            {
                StatusInfo si = JsonConvert.DeserializeObject<StatusInfo>(status);

                // BOT_GAME = 0: player could either be watching a demo (ADDRESS = "bot") or actually in a real server (ADDRESS = "ip:port")
                if (si.bot_game == 0)
                {
                    // player is watching a demo
                    if (si.address.Equals("bot"))
                    {
                        CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusType = 1;
                    }
                    // player is actually in game
                    else
                    {
                        CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusType = 3;
                        CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].IsInGame = true;
                        // query API to get type, map, location, player count info for status message
                        GetServerInfoForStatus(friend.ToLowerInvariant(), si.server_id);

                        // TODO
                        // Add to a list of players to be updated every X (90? 120?) seconds
                    }
                }

                // BOT_GAME = 1: player is in a practice game or training match (we don't care about ADDRESS, but it will be = "loopback")
                if (si.bot_game == 1)
                {
                    CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusType = 2;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void ClearPlayerStatus(Presence pres)
        {
            CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].HasXMPPStatus = false;
            CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].IsInGame = false;
            CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].StatusServerId = "";
            CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].StatusGameType = "";
            CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].StatusGameMap = "";
            CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].StatusGameLocation = "";
            CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].StatusGamePlayerCount = "";
            CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].StatusType = 0;
        }
        private void FriendBecameAvailable(Presence pres)
        {
            if (!IsMe(pres))
            {
                // prevent "double" online status
                if (!IsFriendAlreadyOnline(pres.From.User))
                {
                    Debug.WriteLine("[FRIEND AVAILABLE]: " + " Bare Jid: " + pres.From.Bare + " User: " + pres.From.User);
                    Debug.WriteLine("Friends list before adding " + pres.From.User + "," + " count: " + CLVM.OnlineGroup.Friends.Count());

                    // Must be done on the UI thread
                    Execute.OnUIThread(() =>
                {
                    CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()] = new FriendViewModel(new Friend(pres.From.User.ToLowerInvariant(), IsFavoriteFriend(pres.From.User)));
                });
                    Debug.WriteLine("Friends list after adding " + pres.From.User + "," + " count: " + CLVM.OnlineGroup.Friends.Count());
                }

                // user was previously offline
                if (CLVM.OfflineGroup.Friends.ContainsKey(pres.From.User.ToLowerInvariant()))
                {
                    // Additions and removals to ObservableDictionary must be done on the UI thread
                    Execute.OnUIThread(() =>
                {
                    CLVM.OfflineGroup.Friends.Remove(pres.From.User.ToLowerInvariant());
                });
                    Debug.WriteLine("Friends list before adding " + pres.From.User + "," + " count: " + CLVM.OnlineGroup.Friends.Count() + " After: " + CLVM.OnlineGroup.Friends.Count());
                }

            }

            // Check the user's status
            CheckPlayerStatus(pres);
        }

        private void FriendBecameUnavailble(Presence pres)
        {
            if (!IsMe(pres))
            {
                Debug.WriteLine("[FRIEND UNAVAILABLE]: " + " Bare Jid: " + pres.From.Bare + " User: " + pres.From.User);
                Debug.WriteLine("Friends list before removing " + pres.From.User + "," + " count: " + CLVM.OnlineGroup.Friends.Count());
                // Additions and removals to ObservableDictionary must be done on the UI thread
                Execute.OnUIThread(() =>
                {
                    CLVM.OnlineGroup.Friends.Remove(pres.From.User.ToLowerInvariant());
                    CLVM.OfflineGroup.Friends[pres.From.User.ToLowerInvariant()] = new FriendViewModel(new Friend(pres.From.User.ToLowerInvariant(), IsFavoriteFriend(pres.From.User)));
                });
                Debug.WriteLine("Friends list after removing " + pres.From.User + "," + " count: " + CLVM.OnlineGroup.Friends.Count());
            }
        }

        // Single, initial server status update. Call the QL API to get the server information for a friend, then update the friend's details
        public async void GetServerInfoForStatus(string friend, string server_id)
        {
            try
            {
                HttpClientHandler gzipHandler = new HttpClientHandler();
                HttpClient client = new HttpClient(gzipHandler);

                // QL site sends gzip compressed responses
                if (gzipHandler.SupportsAutomaticDecompression)
                    gzipHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                client.DefaultRequestHeaders.Add("User-Agent", UQLTGlobals.QLUserAgent);
                HttpResponseMessage response = await client.GetAsync(UQLTGlobals.QLDomainDetailsIds + server_id);
                response.EnsureSuccessStatusCode();

                // QL site actually doesn't send "application/json", but "text/html" even though it is actually JSON
                // HtmlDecode replaces &gt;, &lt; same as quakelive.js's EscapeHTML function

                string json = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
                // QL API returns an array, even for individual servers as in this case
                List<Server> qlservers = JsonConvert.DeserializeObject<List<Server>>(json);

                // set the player info for status
                foreach (var qlserver in qlservers)
                {
                    CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusServerId = qlserver.public_id.ToString();
                    CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusGameType = QLFormatter.Gametypes[qlserver.game_type].ShortGametypeName;
                    CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusGameMap = "on " + qlserver.map_title;
                    CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusGameFlag = QLFormatter.Locations[qlserver.location_id].Flag;
                    CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusGameLocation = QLFormatter.Locations[qlserver.location_id].City;
                    CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusGamePlayerCount = string.Format("({0}/{1})", qlserver.num_players, qlserver.max_clients);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                // need something here so it re-tries again in X seconds on failure
            }
        }


        // Batch updates from the automatic timer updating function.
        // This will receive a list of all of the players on friends list who are currently on a game server.
        // It will then extract the public_id for each, send a concatenated list of ids to QL API in one pass
        // Then individually update the friends' game server information from whatever is received from QL API
        // This was created to avoid having multiple HTTP GET requests for every single in-game friend on the list.
        public async void GetServerInfoForStatus(List<string> ingamefriends)
        {

            // Get the server ids (public_id)s of all in-game players to send to QL API
            List<string> server_ids = new List<string>();
            for (int i = 0; i < ingamefriends.Count; i++)
            {
                server_ids.Add(CLVM.OnlineGroup.Friends[ingamefriends[i]].StatusServerId);
            }

            try
            {
                HttpClientHandler gzipHandler = new HttpClientHandler();
                HttpClient client = new HttpClient(gzipHandler);

                // QL site sends gzip compressed responses
                if (gzipHandler.SupportsAutomaticDecompression)
                    gzipHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                client.DefaultRequestHeaders.Add("User-Agent", UQLTGlobals.QLUserAgent);
                HttpResponseMessage response = await client.GetAsync(UQLTGlobals.QLDomainDetailsIds + string.Join(",", server_ids));
                response.EnsureSuccessStatusCode();

                // QL site actually doesn't send "application/json", but "text/html" even though it is actually JSON
                // HtmlDecode replaces &gt;, &lt; same as quakelive.js's EscapeHTML function

                string json = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
                List<Server> qlservers = JsonConvert.DeserializeObject<List<Server>>(json);

                // set the player info for status
                for (int i = 0; i < ingamefriends.Count; i++)
                {

                    CLVM.OnlineGroup.Friends[ingamefriends[i].ToLowerInvariant()].StatusServerId = qlservers[i].public_id.ToString();
                    CLVM.OnlineGroup.Friends[ingamefriends[i].ToLowerInvariant()].StatusGameType = QLFormatter.Gametypes[qlservers[i].game_type].ShortGametypeName;
                    CLVM.OnlineGroup.Friends[ingamefriends[i].ToLowerInvariant()].StatusGameMap = "on " + qlservers[i].map_title;
                    CLVM.OnlineGroup.Friends[ingamefriends[i].ToLowerInvariant()].StatusGameFlag = QLFormatter.Locations[qlservers[i].location_id].Flag;
                    CLVM.OnlineGroup.Friends[ingamefriends[i].ToLowerInvariant()].StatusGameLocation = QLFormatter.Locations[qlservers[i].location_id].City;
                    CLVM.OnlineGroup.Friends[ingamefriends[i].ToLowerInvariant()].StatusGamePlayerCount = string.Format("({0}/{1})", qlservers[i].num_players, qlservers[i].max_clients);

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                // need something here so it re-tries again in X seconds on failure
            }
        }

        private bool IsMe(Presence pres)
        {
            return (pres.From.Bare.Equals(XmppCon.MyJID.Bare.ToLowerInvariant())) ? true : false;
        }

        private bool IsFavoriteFriend(string friend)
        {
            return (UQLTGlobals.SavedFavoriteFriends.Contains(friend.ToLowerInvariant())) ? true : false;
        }

        private bool IsFriendAlreadyOnline(string friend)
        {
            return (CLVM.OnlineGroup.Friends.ContainsKey(friend.ToLowerInvariant())) ? true : false;

        }

        private bool ClientSocket_OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

    }
}
