using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Windows;
using agsXMPP.protocol.extensions.pubsub.@event;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Core.Chat;
using UQLT.Events;
using UQLT.Models.Chat;

namespace UQLT.ViewModels
{
    [Export(typeof(ChatListViewModel))]

    /// <summary>
    /// Viewmodel for the buddy list
    /// </summary>
    public class ChatListViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private ChatHistory _chatHistory;
        private BindableCollection<RosterGroupViewModel> _buddyList;
        private ChatHandler Handler;
        private agsXMPP.Jid Jid;
        private string offlineGroupTitle = "Offline Friends";
        private string onlineGroupTitle = "Online Friends";

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatListViewModel" /> class.
        /// </summary>
        /// <param name="WindowManager">The window manager.</param>
        /// <param name="events">The events that this viewmodel publishes/subscribes to.</param>
        [ImportingConstructor]
        public ChatListViewModel(IWindowManager WindowManager, IEventAggregator events)
        {
            _windowManager = WindowManager;
            _buddyList = new BindableCollection<RosterGroupViewModel>();
            BuddyList.Add(new RosterGroupViewModel(new RosterGroup(onlineGroupTitle), true));
            BuddyList.Add(new RosterGroupViewModel(new RosterGroup(offlineGroupTitle), false));
            LoadFavoriteFriends();

            // Instantiate a XMPP connection and hook up related events for this viewmodel
            Handler = new ChatHandler(this, _windowManager, events);
        }

        /// <summary>
        /// Gets or sets the buddy list.
        /// </summary>
        /// <value>The buddy list.</value>
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

        /// <summary>
        /// Gets the offline group.
        /// </summary>
        /// <value>The offline group.</value>
        public RosterGroupViewModel OfflineGroup
        {
            get
            {
                return BuddyList[1];
            }
        }

        /// <summary>
        /// Gets the online group.
        /// </summary>
        /// <value>The online group.</value>
        public RosterGroupViewModel OnlineGroup
        {
            get
            {
                return BuddyList[0];
            }
        }

        /// <summary>
        /// Adds the friend as a favorite friend.
        /// </summary>
        /// <param name="kvp">The FriendViewModel KeyValuePair.</param>
        /// <remarks>This is called from the view itself.</remarks>
        public void AddFavoriteFriend(KeyValuePair<string, FriendViewModel> kvp)
        {
            if (CanAddFavoriteFriend(kvp))
            {
                UQLTGlobals.SavedFavoriteFriends.Add(kvp.Key);
                Debug.WriteLine("Added " + kvp.Key + " to favorite friends");
                // Dump to disk
                SaveFavoriteFriends();
                // Reflect changes now
                kvp.Value.IsFavorite = true;
            }
            else
            {
                Debug.WriteLine("Favorites already contains " + kvp.Key);
            }
        }

        /// <summary>
        /// Determines whether this friend can be added as a favorite friend.
        /// </summary>
        /// <param name="kvp">The FriendViewModel KeyValuePair.</param>
        /// <returns>
        /// <c>true</c> if the friend can be added, <c>false</c> if the friend is already a favorite
        /// and cannot be added.
        /// </returns>
        /// <remarks>
        /// This is also a Caliburn.Micro action guard that automatically hooks up IsEnabled in the View.
        /// See: https://caliburnmicro.codeplex.com/wikipage?title=All%20About%20Actions
        /// </remarks>
        public bool CanAddFavoriteFriend(KeyValuePair<string, FriendViewModel> kvp)
        {
            return (!UQLTGlobals.SavedFavoriteFriends.Contains(kvp.Key)) ? true : false;
        }

        /// <summary>
        /// Determines whether this friend can be removed from the favorite friends list.
        /// </summary>
        /// <param name="kvp">The FriendViewModel KeyValuePair.</param>
        /// <returns>
        /// <c>true</c> if the friend can be removed, <c>false</c> if the friend is not a favorite
        /// and cannot be removed.
        /// </returns>
        /// <remarks>
        /// This is also a Caliburn.Micro action guard that automatically hooks up IsEnabled in the View.
        /// See: https://caliburnmicro.codeplex.com/wikipage?title=All%20About%20Actions
        /// </remarks>
        public bool CanRemoveFavoriteFriend(KeyValuePair<string, FriendViewModel> kvp)
        {
            return (UQLTGlobals.SavedFavoriteFriends.Contains(kvp.Key)) ? true : false;
        }

        /// <summary>
        /// Opens a new chat message window for this friend.
        /// </summary>
        /// <param name="kvp">The FriendViewModel KeyValuePair.</param>
        /// <remarks>
        /// The window is opened when the user double-clicks the friend or right clicks the friend
        /// and selects 'Open chat' This is called from the view itself.
        /// </remarks>
        public void OpenChatWindow(KeyValuePair<string, FriendViewModel> kvp)
        {
            // manual jid (missing resource, but shouldn't matter)
            //Jid = new agsXMPP.Jid(kvp.Key + "@" + UQLTGlobals.QLXMPPDomain);
            Jid = new agsXMPP.Jid(kvp.Key + "@" + UQLTGlobals.QLXMPPDomain+"/"+kvp.Value.ActiveXMPPResource);
            dynamic settings = new ExpandoObject();
            settings.Topmost = true;
            settings.WindowStartupLocation = WindowStartupLocation.Manual;

            Debug.WriteLine("--> Opening chat with: " + Jid);
            _windowManager.ShowWindow(new ChatMessageViewModel(Jid, Handler.XmppCon, Handler, _windowManager), null, settings);
        }

        /// <summary>
        /// Opens the chat options window.
        /// </summary>
        public void OpenChatOptionsWindow()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _windowManager.ShowWindow(new ChatOptionsViewModel(), null, settings);
        }
        
        /// <summary>
        /// Removes the friend from the favorite friends.
        /// </summary>
        /// <param name="kvp">The FriendViewModel KeyValuePair.</param>
        /// <remarks>This is called from the view itself.</remarks>
        public void RemoveFavoriteFriend(KeyValuePair<string, FriendViewModel> kvp)
        {
            if (CanRemoveFavoriteFriend(kvp))
            {
                UQLTGlobals.SavedFavoriteFriends.Remove(kvp.Key);
                Debug.WriteLine("Removed " + kvp.Key + " from favorite friends");
                // Dump to disk
                SaveFavoriteFriends();
                // Reflect changes now
                kvp.Value.IsFavorite = false;
            }
            else
            {
                Debug.WriteLine("Favorites did not contain " + kvp.Key);
            }
        }

        /// <summary>
        /// Updates the friend's game server information. This occurs when the user highlights the
        /// player on the buddylist in the view.
        /// </summary>
        /// <param name="kvp">The FriendViewModel KeyValuePair.</param>
        /// <remarks>
        /// The friend's game server information is only updated if the friend is currently in a
        /// server. This is called from the view itself.
        /// </remarks>
        public void UpdateFriendGameServerInfo(KeyValuePair<string, FriendViewModel> kvp)
        {
            if (kvp.Value.IsInGame)
            {
                Debug.WriteLine("Requesting server information for friend: " + kvp.Key + " server id: " + kvp.Value.Server.PublicId);
                var h = Handler.ChatGameInfo.UpdateServerInfoForStatusAsync(kvp.Key);
            }
            else
            {
                Debug.WriteLine("Not refreshing server info for player: " + kvp.Key + " because player isn't currently in a game server.");
            }
        }



        /// <summary>
        /// Clears the chat history from the chatlist viewmodel by created a new ChatHistory class for that purpose.
        /// </summary>
        /// <param name="kvp">The FriendViewModel KeyValuePair.</param>
        public void ClearChatHistory(KeyValuePair<string, FriendViewModel> kvp)
        {
            _chatHistory = new ChatHistory();
            _chatHistory.DeleteChatHistoryForUser(Handler.MyJidUser(), kvp.Key);
            

        } 
        
        /// <summary>
        /// Loads the saved favorite friends from JSON file on disk.
        /// </summary>
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

        /// <summary>
        /// Saves the favorite friends to a JSON file on disk.
        /// </summary>
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

    }
}