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
using Newtonsoft.Json;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        private BindableCollection<RosterGroupViewModel> _buddyList;
        public BindableCollection<RosterGroupViewModel> BuddyList
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

        private RosterGroupViewModel _oll;
        public RosterGroupViewModel Oll
        {
            get
            {
                foreach (var g in BuddyList)
                {
                    if (g.GroupName.Equals(OnlineGroup))
                    {
                        _oll = g;
                    }
                }
                return _oll;
            }
        }

        private RosterGroupViewModel _off;
        public RosterGroupViewModel Off
        {
            get
            {
                foreach (var g in BuddyList)
                {
                    if (g.GroupName.Equals(OfflineGroup))
                    {
                        _off = g;
                    }
                }
                return _off;
            }
        }

        public ImageSource FavoriteImage
        {
            get
            {
                return new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/favorite.gif", UriKind.RelativeOrAbsolute));
            }
        }

        public ImageSource FriendImage
        {
            get
            {
                return new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/friend.gif", UriKind.RelativeOrAbsolute));

            }
        }

        [ImportingConstructor]
        public ChatListViewModel()
        {
            _roster = new Dictionary<string, string>();
            _onlineFriends = new RosterGroup(OnlineGroup);
            _offlineFriends = new RosterGroup(OfflineGroup);
            _buddyList = new BindableCollection<RosterGroupViewModel>();
            BuddyList.Add(new RosterGroupViewModel(_onlineFriends, true));
            BuddyList.Add(new RosterGroupViewModel(_offlineFriends, false));
            LoadFavoriteFriends();

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
                bool isfavorite = (UQLTGlobals.FavoriteFriends.Contains(item.Jid.User.ToLowerInvariant())) ? true : false;
                //OfflineFriends.Friends.Add(new Friend(item.Jid.User.ToLowerInvariant(), isfavorite));
                Off.TestFriends.Add(new FriendViewModel(new Friend(item.Jid.User.ToLowerInvariant(), isfavorite), true));
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
                    Console.WriteLine("Friends list before adding " + pres.From.User + "," + " count: " + Oll.TestFriends.Count());
                    bool isfavorite = (UQLTGlobals.FavoriteFriends.Contains(pres.From.User.ToLowerInvariant())) ? true : false;
                    Oll.TestFriends.Add(new FriendViewModel(new Friend(pres.From.User.ToLowerInvariant(), isfavorite), true));
                    Console.WriteLine("Friends list after adding " + pres.From.User + "," + " count: " + Oll.TestFriends.Count());
                }

                // user was previously offline
                foreach (var friend in Off.TestFriends)
                //foreach (var friend in OfflineFriends.Friends)
                {
                    if (friend.FName.Equals(pres.From.User.ToLowerInvariant()))
                    {
                        //OfflineFriends.Friends.Remove(friend);
                        Off.TestFriends.Remove(friend);
                        Console.WriteLine("Friends list before adding " + pres.From.User + "," + " count: " + Oll.TestFriends.Count() + " After: " + Oll.TestFriends.Count());
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
                        Console.WriteLine("Friends list before removing " + pres.From.User + "," + " count: " + Oll.TestFriends.Count());
                        OnlineFriends.Friends.Remove(friend);
                        bool isfavorite = (UQLTGlobals.FavoriteFriends.Contains(pres.From.User.ToLowerInvariant())) ? true : false;
                        //OfflineFriends.Friends.Add(new Friend(pres.From.User.ToLowerInvariant(), isfavorite));
                        Off.TestFriends.Add(new FriendViewModel(new Friend(pres.From.User.ToLowerInvariant(), isfavorite), true));
                        Console.WriteLine("Friends list after removing " + pres.From.User + "," + " count: " + Oll.TestFriends.Count());
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

        public void AddFavoriteFriend(FriendViewModel friend)
        {

            if (!UQLTGlobals.FavoriteFriends.Contains(friend.FName))
            {
                UQLTGlobals.FavoriteFriends.Add(friend.FName);
                Console.WriteLine("Added " + friend.FName + " to favorite friends");
                SaveFavoriteFriends();

                // reflect changes now
                friend.IsFavorite = true;

            }
            else
            {
                Console.WriteLine("Favorites already contains " + friend.FName);
            }

        }

        private void SaveFavoriteFriends()
        {
            try
            {
                string friendjson = JsonConvert.SerializeObject(UQLTGlobals.FavoriteFriends);
                using (FileStream fs = File.Create(UQLTGlobals.FavFriendPath))
                using (TextWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine(friendjson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        private void LoadFavoriteFriends()
        {
            try
            {
                using (StreamReader sr = new StreamReader(UQLTGlobals.FavFriendPath))
                {
                    var s = sr.ReadToEnd();
                    var favorites = JsonConvert.DeserializeObject<List<string>>(s);
                    foreach (var favorite in favorites)
                    {
                        UQLTGlobals.FavoriteFriends.Add(favorite);
                        Console.WriteLine("Auto-added " + favorite + " to favorite friends");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private bool ClientSocket_OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

    }
}
