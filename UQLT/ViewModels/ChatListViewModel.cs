using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using agsXMPP;
using agsXMPP.Collections;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP.Xml.Dom;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Models.Chat;

namespace UQLT.ViewModels
{
    [Export(typeof(ChatListViewModel))]
    public class ChatListViewModel : PropertyChangedBase
    {

        private XmppClientConnection XmppCon;

        private static string OnlineGroupTitle = "Online Friends";
        private static string OfflineGroupTitle = "Offline Friends";

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

        public RosterGroupViewModel OnlineGroup
        {
            get
            {
                return BuddyList[0];
            }
        }

        public RosterGroupViewModel OfflineGroup
        {
            get
            {
                return BuddyList[1];
            }
        }

        [ImportingConstructor]
        public ChatListViewModel()
        {
            _roster = new Dictionary<string, string>();
            _buddyList = new BindableCollection<RosterGroupViewModel>();
            BuddyList.Add(new RosterGroupViewModel(new RosterGroup(OnlineGroupTitle), true));
            BuddyList.Add(new RosterGroupViewModel(new RosterGroup(OfflineGroupTitle), false));
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
                bool isfavorite = (UQLTGlobals.SavedFavoriteFriends.Contains(item.Jid.User.ToLowerInvariant())) ? true : false;
                OfflineGroup.Friends.Add(new FriendViewModel(new Friend(item.Jid.User.ToLowerInvariant(), isfavorite), true));
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

        private void FriendBecameAvailable(Presence pres)
        {
            if (!pres.From.Bare.Equals(XmppCon.MyJID.Bare.ToLowerInvariant()))
            {
                // prevent "double" online status
                if (!FriendAlreadyOnline(pres.From.User.ToLowerInvariant()))
                {
                    Debug.WriteLine("[FRIEND AVAILABLE]: " + " Bare Jid: " + pres.From.Bare + " User: " + pres.From.User);
                    Debug.WriteLine("Friends list before adding " + pres.From.User + "," + " count: " + OnlineGroup.Friends.Count());
                    bool isfavorite = (UQLTGlobals.SavedFavoriteFriends.Contains(pres.From.User.ToLowerInvariant())) ? true : false;
                    OnlineGroup.Friends.Add(new FriendViewModel(new Friend(pres.From.User.ToLowerInvariant(), isfavorite), true));
                    Debug.WriteLine("Friends list after adding " + pres.From.User + "," + " count: " + OnlineGroup.Friends.Count());
                }

                // user was previously offline
                foreach (var friend in OfflineGroup.Friends)
                {
                    if (friend.FriendName.Equals(pres.From.User.ToLowerInvariant()))
                    {
                        OfflineGroup.Friends.Remove(friend);
                        Debug.WriteLine("Friends list before adding " + pres.From.User + "," + " count: " + OnlineGroup.Friends.Count() + " After: " + OnlineGroup.Friends.Count());
                        break;
                    }
                }

            }
        }

        private void FriendBecameUnavailble(Presence pres)
        {
            if (!pres.From.Bare.Equals(XmppCon.MyJID.Bare.ToLowerInvariant()))
            {
                Debug.WriteLine("[FRIEND UNAVAILABLE]: " + " Bare Jid: " + pres.From.Bare + " User: " + pres.From.User);

                foreach (var friend in OnlineGroup.Friends)
                {
                    if (friend.FriendName.ToLowerInvariant().Equals(pres.From.User.ToLowerInvariant()))
                    {
                        Debug.WriteLine("Friends list before removing " + pres.From.User + "," + " count: " + OnlineGroup.Friends.Count());
                        OnlineGroup.Friends.Remove(friend);
                        bool isfavorite = (UQLTGlobals.SavedFavoriteFriends.Contains(pres.From.User.ToLowerInvariant())) ? true : false;
                        OfflineGroup.Friends.Add(new FriendViewModel(new Friend(pres.From.User.ToLowerInvariant(), isfavorite), true));
                        Debug.WriteLine("Friends list after removing " + pres.From.User + "," + " count: " + OnlineGroup.Friends.Count());
                        break;
                    }
                }
            }
        }

        private bool FriendAlreadyOnline(string friend)
        {
            foreach (var frnd in OnlineGroup.Friends)
            {
                if (friend.Equals(frnd.FriendName.ToLowerInvariant()))
                {
                    return true;
                }
            }
            return false;
        }

        // Caliburn.Micro Action Guard (https://caliburnmicro.codeplex.com/wikipage?title=All%20About%20Actions)
        public bool CanAddFavoriteFriend(FriendViewModel friend)
        {
            return (!UQLTGlobals.SavedFavoriteFriends.Contains(friend.FriendName)) ? true : false;
        }

        // Caliburn.Micro Action Guard (https://caliburnmicro.codeplex.com/wikipage?title=All%20About%20Actions)
        public bool CanRemoveFavoriteFriend(FriendViewModel friend)
        {
            return (UQLTGlobals.SavedFavoriteFriends.Contains(friend.FriendName)) ? true : false;
        }

        public void AddFavoriteFriend(FriendViewModel friend)
        {
            if (CanAddFavoriteFriend(friend))
            {
                UQLTGlobals.SavedFavoriteFriends.Add(friend.FriendName);
                Debug.WriteLine("Added " + friend.FriendName + " to favorite friends");
                SaveFavoriteFriends();

                // reflect changes now
                friend.IsFavorite = true;

            }
            else
            {
                Debug.WriteLine("Favorites already contains " + friend.FriendName);
            }

        }

        public void RemoveFavoriteFriend(FriendViewModel friend)
        {
            if (CanRemoveFavoriteFriend(friend))
            {
                UQLTGlobals.SavedFavoriteFriends.Remove(friend.FriendName);
                Debug.WriteLine("Removed " + friend.FriendName + " from favorite friends");
                SaveFavoriteFriends();

                friend.IsFavorite = false;
            }
            else
            {
                Debug.WriteLine("Favorites did not contain " + friend.FriendName);
            }
        }

        private void SaveFavoriteFriends()
        {
            try
            {
                string friendjson = JsonConvert.SerializeObject(UQLTGlobals.SavedFavoriteFriends);
                using (FileStream fs = File.Create(UQLTGlobals.SavedFavFriendPath))
                using (TextWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine(friendjson);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        private void LoadFavoriteFriends()
        {
            try
            {
                using (StreamReader sr = new StreamReader(UQLTGlobals.SavedFavFriendPath))
                {
                    var s = sr.ReadToEnd();
                    var favorites = JsonConvert.DeserializeObject<List<string>>(s);
                    foreach (var favorite in favorites)
                    {
                        UQLTGlobals.SavedFavoriteFriends.Add(favorite);
                        Debug.WriteLine("Auto-added " + favorite + " to favorite friends");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private bool ClientSocket_OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

    }
}
