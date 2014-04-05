using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Caliburn.Micro;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP.Collections;
using System.Windows;
using UQLT.Models.Chat;
using agsXMPP.Xml.Dom;
using System.Collections.ObjectModel;

namespace UQLT.ViewModels
{
    [Export(typeof(ChatListViewModel))]
    public class ChatListViewModel : PropertyChangedBase
    {

        private XmppClientConnection XmppCon;

        private static string OnlineGroup = "Online Friends";
        private static string OfflineGroup = "Offline Friends";

        private Dictionary<string, string> _roster;

        public Dictionary<string, string> Roster
        {
            get
            {
                return _roster;
            }

            set
            {
                _roster = value;
                NotifyOfPropertyChange(() => Roster);
            }
        }

        private BindableCollection<ChatListDetailsViewModel> _buddyList;
        public BindableCollection<ChatListDetailsViewModel> BuddyList
        {
            get
            {
                return _buddyList;
            }

            set
            {
                _buddyList = value;
                NotifyOfPropertyChange(() => BuddyList);
            }
        }

        private RosterGroup _onlineFriends;

        public RosterGroup OnlineFriends
        {
            get
            {
                return _onlineFriends;
            }

            set
            {
                _onlineFriends = value;
                NotifyOfPropertyChange(() => OnlineFriends);
            }
        }

        private RosterGroup _offlineFriends;

        public RosterGroup OfflineFriends
        {
            get
            {
                return _offlineFriends;
            }

            set
            {
                _offlineFriends = value;
                NotifyOfPropertyChange(() => OfflineFriends);
            }
        }


        [ImportingConstructor]
        public ChatListViewModel()
        {
            _roster = new Dictionary<string, string>();
            _onlineFriends = new RosterGroup(OnlineGroup);
            _offlineFriends = new RosterGroup(OfflineGroup);
            _buddyList = new BindableCollection<ChatListDetailsViewModel>();
            BuddyList.Add(new ChatListDetailsViewModel(_onlineFriends, true));
            BuddyList.Add(new ChatListDetailsViewModel(_offlineFriends, false));

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
                OfflineFriends.Friends.Add(new Friend(item.Jid.User.ToLowerInvariant()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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

        private void FriendBecameAvailable(Presence pres)
        {
            if (!pres.From.Bare.Equals(XmppCon.MyJID.Bare.ToLowerInvariant()))
            {
                // prevent "double" online status
                if (!FriendAlreadyOnline(pres.From.User.ToLowerInvariant()))
                {
                    Console.WriteLine("[FRIEND AVAILABLE]: " + " Bare Jid: " + pres.From.Bare + " User: " + pres.From.User);
                    Console.WriteLine("Friends list before adding " + pres.From.User + "," + " count: " + OnlineFriends.Friends.Count());
                    OnlineFriends.Friends.Add(new Friend(pres.From.User.ToLowerInvariant()));
                    Console.WriteLine("Friends list after adding " + pres.From.User + "," + " count: " + OnlineFriends.Friends.Count());
                }

                // user was previously offline
                foreach (var friend in OfflineFriends.Friends)
                {
                    if (friend.FriendName.Equals(pres.From.User.ToLowerInvariant()))
                    {
                        OfflineFriends.Friends.Remove(friend);
                        Console.WriteLine("Friends list before adding " + pres.From.User + "," + " count: " + OnlineFriends.Friends.Count() + " After: " + OnlineFriends.Friends.Count());
                        break;
                    }
                }

            }
        }

        private void FriendBecameUnavailble(Presence pres)
        {
            if (!pres.From.Bare.Equals(XmppCon.MyJID.Bare.ToLowerInvariant()))
            {
                Console.WriteLine("[FRIEND UNAVAILABLE]: " + " Bare Jid: " + pres.From.Bare + " User: " + pres.From.User);

                foreach (var friend in OnlineFriends.Friends)
                {
                    if (friend.FriendName.ToLowerInvariant().Equals(pres.From.User.ToLowerInvariant()))
                    {
                        Console.WriteLine("Friends list before removing " + pres.From.User + "," + " count: " + OnlineFriends.Friends.Count());
                        OnlineFriends.Friends.Remove(friend);
                        OfflineFriends.Friends.Add(new Friend(pres.From.User.ToLowerInvariant()));
                        Console.WriteLine("Friends list after removing " + pres.From.User + "," + " count: " + OnlineFriends.Friends.Count());
                        break;
                    }
                }
            }
        }

        private bool FriendAlreadyOnline(string friend)
        {
            foreach (var frnd in OnlineFriends.Friends)
            {
                if (friend.Equals(frnd.FriendName.ToLowerInvariant()))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddFavoriteFriend(Friend friend)
        {
            Console.WriteLine("Added " + friend.FriendName + " to friends");
        }


        private bool ClientSocket_OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

    }
}
