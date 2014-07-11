using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Windows;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Core.Chat;
using UQLT.Models.Chat;

namespace UQLT.ViewModels
{
    /// <summary>
    /// Viewmodel for the buddy list
    /// </summary>
    [Export(typeof(ChatListViewModel))]
    public class ChatListViewModel : PropertyChangedBase
    {
        private const string OfflineGroupTitle = "Offline Friends";
        private const string OnlineGroupTitle = "Online Friends";
        private readonly ChatHandler _handler;
        private readonly IWindowManager _windowManager;
        private BindableCollection<RosterGroupViewModel> _buddyList;
        private ChatHistory _chatHistory;
        private agsXMPP.Jid _jid;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatListViewModel" /> class.
        /// </summary>
        /// <param name="windowManager">The window manager.</param>
        /// <param name="events">The events that this viewmodel publishes/subscribes to.</param>
        [ImportingConstructor]
        public ChatListViewModel(IWindowManager windowManager, IEventAggregator events)
        {
            _windowManager = windowManager;
            _buddyList = new BindableCollection<RosterGroupViewModel>();
            BuddyList.Add(new RosterGroupViewModel(new RosterGroup(OnlineGroupTitle), true));
            BuddyList.Add(new RosterGroupViewModel(new RosterGroup(OfflineGroupTitle), false));
            LoadFavoriteFriends();

            // Instantiate a XMPP connection and hook up related events for this viewmodel
            _handler = new ChatHandler(this, _windowManager);
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
                UQltGlobals.SavedFavoriteFriends.Add(kvp.Key);
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
            return (!UQltGlobals.SavedFavoriteFriends.Contains(kvp.Key));
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
            return (UQltGlobals.SavedFavoriteFriends.Contains(kvp.Key));
        }

        /// <summary>
        /// Clears the chat history from the chatlist viewmodel by created a new ChatHistory class
        /// for that purpose.
        /// </summary>
        /// <param name="kvp">The FriendViewModel KeyValuePair.</param>
        public void ClearChatHistory(KeyValuePair<string, FriendViewModel> kvp)
        {
            _chatHistory = new ChatHistory();
            _chatHistory.DeleteChatHistoryForUser(_handler.MyJidUser(), kvp.Key);
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
            //Jid = new agsXMPP.Jid(kvp.Key + "@" + UQLTGlobals.QlXmppDomain);
            _jid = new agsXMPP.Jid(kvp.Key + "@" + UQltGlobals.QlXmppDomain + "/" + kvp.Value.ActiveXmppResource);
            dynamic settings = new ExpandoObject();
            settings.Topmost = true;
            settings.WindowStartupLocation = WindowStartupLocation.Manual;

            Debug.WriteLine("--> Opening chat with: " + _jid);
            _windowManager.ShowWindow(new ChatMessageViewModel(_jid, _handler.XmppCon, _handler, _windowManager), null, settings);
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
                UQltGlobals.SavedFavoriteFriends.Remove(kvp.Key);
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
                // Async: suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
                var h = _handler.ChatGameInfo.UpdateServerInfoForStatusAsync(kvp.Key);
            }
            else
            {
                Debug.WriteLine("Not refreshing server info for player: " + kvp.Key + " because player isn't currently in a game server.");
            }
        }
        
        /// <summary>
        /// Loads the saved favorite friends from JSON file on disk.
        /// </summary>
        private void LoadFavoriteFriends()
        {
            try
            {
                using (var sr = new StreamReader(UQltGlobals.SavedFavFriendPath))
                {
                    var s = sr.ReadToEnd();
                    var favorites = JsonConvert.DeserializeObject<List<string>>(s);
                    foreach (var favorite in favorites)
                    {
                        UQltGlobals.SavedFavoriteFriends.Add(favorite);
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
                string friendjson = JsonConvert.SerializeObject(UQltGlobals.SavedFavoriteFriends);
                using (FileStream fs = File.Create(UQltGlobals.SavedFavFriendPath))
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