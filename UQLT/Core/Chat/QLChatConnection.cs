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
            //Presence p = new Presence(ShowType.chat, "");
            //p.Type = PresenceType.available;
            //XmppCon.Send(p);

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
        private void CheckStatus(Presence pres)
        {
            if (string.IsNullOrEmpty(pres.Status))
            {
                CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].HasStatus = false;
                CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].IsInGame = false;
                CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].StatusType = 0;
                //TODO
                // if player is in list of in game players to be updated every X (90? 120?) seconds, then remove him
                Debug.WriteLine("**Status for " + pres.From.User.ToLowerInvariant() + " is empty.");
            }
            else
            {
                CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].HasStatus = true;
                UpdateStatus(pres.From.User, pres.Status);
                Debug.WriteLine("**Status for " + pres.From.User.ToLowerInvariant() + " is: " + pres.Status);
            }
        }

        private void UpdateStatus(string friend, string status)
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
                        CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].Status = "Watching a demo";
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
                    CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].Status = "Playing a practice match";
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
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
            CheckStatus(pres);
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

        private async void GetServerInfoForStatus(string friend, int server_id)
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
