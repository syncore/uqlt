using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Models.Chat;
using UQLT.Core.Chat;

namespace UQLT.ViewModels
{
    [Export(typeof(ChatListViewModel))]
    public class ChatListViewModel : PropertyChangedBase
    {

        private QLChatConnection QLChat;

        private static string OnlineGroupTitle = "Online Friends";
        private static string OfflineGroupTitle = "Offline Friends";


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
            _buddyList = new BindableCollection<RosterGroupViewModel>();
            BuddyList.Add(new RosterGroupViewModel(new RosterGroup(OnlineGroupTitle), true));
            BuddyList.Add(new RosterGroupViewModel(new RosterGroup(OfflineGroupTitle), false));
            LoadFavoriteFriends();

            // Instantiate a XMPP connection and hook up related events for this viewmodel
            QLChat = new QLChatConnection(this);


        }

        // Load saved friends from JSON file on disk on launch and add to global list
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

        // Dump the global saved friends list to JSON file on disk
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

        // This user can be added as a favorite friend. Also a Caliburn.Micro action guard
        // Automatically hooks up IsEnabled in View, see: https://caliburnmicro.codeplex.com/wikipage?title=All%20About%20Actions
        public bool CanAddFavoriteFriend(FriendViewModel friend)
        {
            return (!UQLTGlobals.SavedFavoriteFriends.Contains(friend.FriendName)) ? true : false;
        }

        // This user can be removed from favorite friends. Also a Caliburn.Micro action guard
        // Automatically hooks up IsEnabled in View, see: https://caliburnmicro.codeplex.com/wikipage?title=All%20About%20Actions
        public bool CanRemoveFavoriteFriend(FriendViewModel friend)
        {
            return (UQLTGlobals.SavedFavoriteFriends.Contains(friend.FriendName)) ? true : false;
        }

        // Add user to favorite friends
        public void AddFavoriteFriend(FriendViewModel friend)
        {
            if (CanAddFavoriteFriend(friend))
            {
                UQLTGlobals.SavedFavoriteFriends.Add(friend.FriendName);
                Debug.WriteLine("Added " + friend.FriendName + " to favorite friends");
                // Dump to disk
                SaveFavoriteFriends();
                // Reflect changes now
                friend.IsFavorite = true;

            }
            else
            {
                Debug.WriteLine("Favorites already contains " + friend.FriendName);
            }

        }

        // Remove user from favorite friends
        public void RemoveFavoriteFriend(FriendViewModel friend)
        {
            if (CanRemoveFavoriteFriend(friend))
            {
                UQLTGlobals.SavedFavoriteFriends.Remove(friend.FriendName);
                Debug.WriteLine("Removed " + friend.FriendName + " from favorite friends");
                // Dump to disk
                SaveFavoriteFriends();
                // Reflect changes now
                friend.IsFavorite = false;
            }
            else
            {
                Debug.WriteLine("Favorites did not contain " + friend.FriendName);
            }
        }

    }
}
