using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using UQLT.Core;
using UQLT.Core.Modules.Chat;
using UQLT.Interfaces;
using UQLT.Models.Chat;
using UQLT.Models.Configuration;

namespace UQLT.ViewModels.Chat
{
    /// <summary>
    /// Viewmodel for the buddy list
    /// </summary>
    [Export(typeof(ChatListViewModel))]
    public class ChatListViewModel : PropertyChangedBase, IUqltConfiguration
    {
        private const string OfflineGroupTitle = "Offline Friends";
        private const string OnlineGroupTitle = "Online Friends";
        private readonly IEventAggregator _events;
        private readonly ChatHandler _handler;
        private readonly IWindowManager _windowManager;
        private BindableCollection<RosterGroupViewModel> _buddyList;
        private ChatHistory _chatHistory;
        private List<string> _favoriteFriends;
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
            _events = events;
            _buddyList = new BindableCollection<RosterGroupViewModel>();
            BuddyList.Add(new RosterGroupViewModel(new RosterGroup(OnlineGroupTitle), true));
            BuddyList.Add(new RosterGroupViewModel(new RosterGroup(OfflineGroupTitle), false));

            LoadConfig();

            // Instantiate a XMPP connection and hook up related events for this viewmodel
            _handler = new ChatHandler(this, _windowManager, _events);
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

        public List<string> FavoriteFriends
        {
            get
            {
                return _favoriteFriends;
            }
            set { _favoriteFriends = value; }
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
        /// <param name="kvp">The <see cref="FriendViewModel"/> key value pair.</param>
        /// <remarks>This is called from the view itself.</remarks>
        public void AddFavoriteFriend(KeyValuePair<string, FriendViewModel> kvp)
        {
            if (CanAddFavoriteFriend(kvp))
            {
                FavoriteFriends.Add(kvp.Key);
                Debug.WriteLine("Added " + kvp.Key + " to favorite friends");
                SaveConfig();
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
        /// <param name="kvp">The <see cref="FriendViewModel"/> key value pair.</param>
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
            return (!FavoriteFriends.Contains(kvp.Key));
        }

        /// <summary>
        /// Determines whether this friend can be removed from the favorite friends list.
        /// </summary>
        /// <param name="kvp">The <see cref="FriendViewModel"/> key value pair.</param>
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
            return (FavoriteFriends.Contains(kvp.Key));
        }

        /// <summary>
        /// Clears the chat history from the chatlist viewmodel by created a new ChatHistory class
        /// for that purpose.
        /// </summary>
        /// <param name="kvp">The <see cref="FriendViewModel"/> key value pair.</param>
        public void ClearChatHistory(KeyValuePair<string, FriendViewModel> kvp)
        {
            _chatHistory = new ChatHistory(_events);
            _chatHistory.DeleteChatHistoryForUser(_handler.MyJidUser(), kvp.Key, true);
        }

        /// <summary>
        /// Checks whether the configuration already exists
        /// </summary>
        /// <returns><c>true</c> if configuration exists, otherwise <c>false</c></returns>
        public bool ConfigExists()
        {
            return File.Exists(UQltFileUtils.GetConfigurationPath());
        }

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        public void LoadConfig()
        {
            if (!ConfigExists())
            {
                LoadDefaultConfig();
            }

            var cfghandler = new ConfigurationHandler();
            cfghandler.ReadConfig();

            FavoriteFriends = cfghandler.ChatFavoriteFriends;
        }

        /// <summary>
        /// Loads the default configuration.
        /// </summary>
        public void LoadDefaultConfig()
        {
            var cfghandler = new ConfigurationHandler();
            cfghandler.RestoreDefaultConfig();
        }

        /// <summary>
        /// Opens the add friend window.
        /// </summary>
        public void OpenAddFriendWindow()
        {
            dynamic settings = new ExpandoObject();
            settings.Topmost = true;
            settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _windowManager.ShowWindow(new AddFriendViewModel(_handler), null, settings);
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
        /// <param name="kvp">The <see cref="FriendViewModel"/> key value pair.</param>
        /// <remarks>
        /// The window is opened when the user double-clicks the friend or right clicks the friend
        /// and selects 'Open chat' This is called from the view itself.
        /// </remarks>
        public void OpenChatWindow(KeyValuePair<string, FriendViewModel> kvp)
        {
            // Manual jid construction. The full jid, including resource is needed here.
            _jid = new agsXMPP.Jid(kvp.Key + "@" + UQltGlobals.QlXmppDomain + "/" + kvp.Value.ActiveXmppResource);
            dynamic settings = new ExpandoObject();
            settings.Topmost = true;
            settings.WindowStartupLocation = WindowStartupLocation.Manual;

            Debug.WriteLine("--> Opening chat with: " + _jid);
            _windowManager.ShowWindow(new ChatMessageViewModel(_jid, _handler.XmppCon, _handler, _windowManager, _events), null, settings);
        }

        /// <summary>
        /// Removes the friend from the favorite friends.
        /// </summary>
        /// <param name="kvp">The <see cref="FriendViewModel"/> key value pair.</param>
        /// <remarks>This is called from the view itself.</remarks>
        public void RemoveFavoriteFriend(KeyValuePair<string, FriendViewModel> kvp)
        {
            if (CanRemoveFavoriteFriend(kvp))
            {
                FavoriteFriends.Remove(kvp.Key);
                Debug.WriteLine("Removed " + kvp.Key + " from favorite friends");
                SaveConfig();
                // Reflect changes now
                kvp.Value.IsFavorite = false;
            }
            else
            {
                Debug.WriteLine("Favorites did not contain " + kvp.Key);
            }
        }

        /// <summary>
        /// Removes the friend from the buddy list and unsubscribes from friend.
        /// </summary>
        /// <param name="kvp">The <see cref="FriendViewModel"/> key value pair.</param>
        public void RemoveFriend(KeyValuePair<string, FriendViewModel> kvp)
        {
            // Manual jid construction. Only the bare jid is needed here.
            _jid = new agsXMPP.Jid(kvp.Key + "@" + UQltGlobals.QlXmppDomain);
            _handler.UnsubscribeAndRemoveFriend(_jid);
            _handler.RemoveFriendFromInternalRosterGroups(_jid);
        }

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        public void SaveConfig()
        {
            var cfghandler = new ConfigurationHandler();
            cfghandler.ReadConfig();

            cfghandler.ChatFavoriteFriends = FavoriteFriends;

            cfghandler.WriteConfig();
        }

        /// <summary>
        /// Updates the friend's game server information. This occurs when the user highlights the
        /// player on the buddylist in the view.
        /// </summary>
        /// <param name="kvp">The <see cref="FriendViewModel"/> key value pair.</param>
        /// <remarks>
        /// The friend's game server information is only updated if the friend is currently in a
        /// server. This is called from the view itself.
        /// </remarks>
        public async Task UpdateFriendGameServerInfo(KeyValuePair<string, FriendViewModel> kvp)
        {
            if (kvp.Value.IsInGame)
            {
                Debug.WriteLine("Requesting server information for friend: " + kvp.Key + " server id: " + kvp.Value.Server.PublicId);
                await _handler.ChatGameInfo.UpdateServerInfoForStatusAsync(kvp.Key);
            }
            else
            {
                Debug.WriteLine("Not refreshing server info for player: " + kvp.Key + " because player isn't currently in a game server.");
            }
        }
    }
}