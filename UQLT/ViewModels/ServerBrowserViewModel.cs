using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Caliburn.Micro;
using UQLT.Core.ServerBrowser;
using UQLT.Events;
using UQLT.Interfaces;
using UQLT.Models.Configuration;

namespace UQLT.ViewModels
{
    /// <summary>
    /// Viewmodel for the Server Browser view.
    /// </summary>
    [Export(typeof(ServerBrowserViewModel))]
    public class ServerBrowserViewModel : PropertyChangedBase, IHandle<ServerRequestEvent>, IUqltConfiguration
    {
        private readonly IEventAggregator _events;
        private readonly StringBuilder _playerSearchFoundBuilder = new StringBuilder();
        private readonly ServerBrowser _sb;
        private int _autoRefreshIndex;

        private List<ServerBrowserRefreshItem> _autoRefreshItems = new List<ServerBrowserRefreshItem>
        {
                new ServerBrowserRefreshItem {Name = "every 30 seconds", Seconds = 30},
                new ServerBrowserRefreshItem {Name = "every 1 minute", Seconds = 60},
                new ServerBrowserRefreshItem {Name = "every 1.5 minutes", Seconds = 90},
                new ServerBrowserRefreshItem {Name = "every 5 minutes", Seconds = 300},
        };

        private int _autoRefreshSeconds;
        private string _filterUrl;
        private bool _isAutoRefreshEnabled;
        private bool _isPlayerSearchEnabled;
        private bool _isUpdatingServers;
        private int _playerSearchFoundCount;
        private string _playerSearchTerm;
        private ServerDetailsViewModel _selectedServer;
        private ObservableCollection<ServerDetailsViewModel> _servers = new ObservableCollection<ServerDetailsViewModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerBrowserViewModel" /> class.
        /// </summary>
        /// <param name="events">The events that this viewmodel publishes and/or subscribes to.</param>
        [ImportingConstructor]
        public ServerBrowserViewModel(IEventAggregator events)
        {
            _events = events;
            _events.Subscribe(this);

            DoServerBrowserAutoSort("FullLocationName");
            LoadConfig();

            // Instantiate a new server browser for this viewmodel
            _sb = new ServerBrowser(this, events);

            // Hook the collection view source up to the collection of servers to enable ability to search for players on servers.
            ServersView = CollectionViewSource.GetDefaultView(Servers);
            ServersView.Filter = EvaluatePlayerNameFilter;
        }

        /// <summary>
        /// Gets or sets the index of the automatic refresh value.
        /// </summary>
        /// <value>The index of the automatic refresh value.</value>
        public int AutoRefreshIndex
        {
            get
            {
                return _autoRefreshIndex;
            }
            set
            {
                _autoRefreshIndex = value;
                NotifyOfPropertyChange(() => AutoRefreshIndex);
            }
        }

        /// <summary>
        /// Gets or sets the list of time intervals that the user can select for automatic server refreshing.
        /// </summary>
        /// <value>The list of time intervals.</value>
        public List<ServerBrowserRefreshItem> AutoRefreshItems
        {
            get
            {
                return _autoRefreshItems;
            }
            set
            {
                _autoRefreshItems = value;
            }
        }

        /// <summary>
        /// Gets or sets the time, in seconds, for automatic server refreshing
        /// </summary>
        /// <value>The time in seconds, for automatic server refreshing.</value>
        public int AutoRefreshSeconds
        {
            get
            {
                return _autoRefreshSeconds;
            }
            set
            {
                _autoRefreshSeconds = value;
                NotifyOfPropertyChange(() => AutoRefreshSeconds);
            }
        }

        /// <summary>
        /// Gets or sets the Quake Live filter URL.
        /// </summary>
        /// <value>The Quake Live filter URL.</value>
        public string FilterUrl
        {
            get
            {
                return _filterUrl;
            }

            set
            {
                _filterUrl = value + Math.Truncate((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
                NotifyOfPropertyChange(() => FilterUrl);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this automatic server refreshing is enabled.
        /// </summary>
        /// <value><c>true</c> if this automatic refresh is enabled; otherwise, <c>false</c>.</value>
        public bool IsAutoRefreshEnabled
        {
            get
            {
                return _isAutoRefreshEnabled;
            }
            set
            {
                _isAutoRefreshEnabled = value;
                NotifyOfPropertyChange(() => IsAutoRefreshEnabled);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the player name search is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the server list is not being updated; otherwise, <c>false</c> if the server list is currently being updated.
        /// </value>
        public bool IsPlayerSearchEnabled
        {
            get
            {
                return _isPlayerSearchEnabled;
            }
            set
            {
                _isPlayerSearchEnabled = value;
                NotifyOfPropertyChange(() => IsPlayerSearchEnabled);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this viewmodel is currently updating the server list.
        /// </summary>
        /// <value><c>true</c> if this instance is updating servers; otherwise, <c>false</c>.</value>
        public bool IsUpdatingServers
        {
            get
            {
                return _isUpdatingServers;
            }
            set
            {
                _isUpdatingServers = value;
                IsPlayerSearchEnabled = value != true;
                NotifyOfPropertyChange(() => IsUpdatingServers);
            }
        }

        /// <summary>
        /// Gets or sets the names of the players found in the player search.
        /// </summary>
        /// <value>
        /// The player names.
        /// </value>
        /// <remarks>This is used for the status bar.</remarks>
        public StringBuilder PlayerNameFoundBuilder
        {
            get
            {
                return _playerSearchFoundBuilder;
            }
            set
            {
                _playerSearchFoundBuilder.Append(value);
            }
        }

        /// <summary>
        /// Gets or sets the player search term used to find a player within the current list of servers.
        /// </summary>
        /// <value>
        /// The player name filter.
        /// </value>
        public string PlayerSearchTerm
        {
            get
            {
                return _playerSearchTerm;
            }
            set
            {
                if (value == _playerSearchTerm) return;
                _playerSearchTerm = value;
                ResetPlayerSearchStatus();
                NotifyOfPropertyChange(() => PlayerSearchTerm);
                ServersView.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the selected server.
        /// </summary>
        /// <value>The selected server.</value>
        public ServerDetailsViewModel SelectedServer
        {
            get
            {
                return _selectedServer;
            }

            set
            {
                _selectedServer = value;
                NotifyOfPropertyChange(() => SelectedServer);
            }
        }

        /// <summary>
        /// Gets or sets the servers that this viewmodel will display in the view.
        /// </summary>
        /// <value>The servers that this viewmodel will display in the view.</value>
        public ObservableCollection<ServerDetailsViewModel> Servers
        {
            get
            {
                return _servers;
            }

            set
            {
                _servers = value;
                NotifyOfPropertyChange(() => Servers);
            }
        }

        /// <summary>
        /// Gets or sets the servers view.
        /// </summary>
        /// <value>
        /// The servers view.
        /// </value>
        /// <remarks>This is what the Server ListView in the server browser window binds to.</remarks>
        public ICollectionView ServersView { get; set; }

        /// <summary>
        /// Clears the player name filter.
        /// </summary>
        public void ClearAndResetPlayerSearchTerm()
        {
            PlayerSearchTerm = string.Empty;
            // Statusbar
            _events.PublishOnUIThread(new PlayerSearchingEvent(false, string.Empty));
            _events.PublishOnUIThread(new PlayerFoundCountEvent(0));
            _events.PublishOnUIThread(new PlayerFoundNameEvent(string.Empty));
            PlayerNameFoundBuilder.Clear();
        }

        /// <summary>
        /// Checks whether the configuration already exists
        /// </summary>
        /// <returns><c>true</c> if configuration exists, otherwise <c>false</c></returns>
        public bool ConfigExists()
        {
            return File.Exists(UQltGlobals.ConfigPath);
        }

        /// <summary>
        /// Handles the specified message (event) that is received whenever this viewmodel receive
        /// notice of a new default filter, either through the "make new default" button or "reset
        /// filters" button from the FilterViewModel.
        /// </summary>
        /// <param name="message">The message (event).</param>
        public void Handle(ServerRequestEvent message)
        {
            FilterUrl = message.ServerRequestUrl;
            // Async: suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
            var l = _sb.LoadServerListAsync(FilterUrl);
            Debug.WriteLine("[EVENT RECEIVED] Filter URL Change: " + message.ServerRequestUrl);
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

            IsAutoRefreshEnabled = cfghandler.SbOptAutoRefresh;
            AutoRefreshIndex = cfghandler.SbOptAutoRefreshIndex;
            AutoRefreshSeconds = cfghandler.SbOptAutoRefreshSeconds;
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
        /// Saves the configuration.
        /// </summary>
        public void SaveConfig()
        {
            var cfghandler = new ConfigurationHandler();
            cfghandler.ReadConfig();

            cfghandler.SbOptAutoRefresh = IsAutoRefreshEnabled;
            cfghandler.SbOptAutoRefreshIndex = AutoRefreshIndex;
            cfghandler.SbOptAutoRefreshSeconds = AutoRefreshSeconds;

            cfghandler.WriteConfig();
        }

        /// <summary>
        /// Sets the refresh time.
        /// </summary>
        /// <param name="seconds">The time, in seconds.</param>
        /// <remarks>This is called from the view itself.</remarks>
        public void SetRefreshTime(int seconds)
        {
            Debug.WriteLine("Setting auto-server refresh time to " + seconds + " seconds.");
            AutoRefreshSeconds = seconds;
            SaveConfig();
        }

        /// <summary>
        /// Starts the server refresh timer.
        /// </summary>
        /// <remarks>This is called from the view itself.</remarks>
        public void StartServerRefreshTimer()
        {
            _sb.StartServerRefreshTimer();
        }

        /// <summary>
        /// Stops the server refresh timer.
        /// </summary>
        /// <remarks>This is called from the view itself.</remarks>
        public void StopServerRefreshTimer()
        {
            _sb.StopServerRefreshTimer();
        }

        /// <summary>
        /// Clears all highlighted players found in a player search.
        /// </summary>
        private void ClearAllHighlightedPlayers()
        {
            foreach (var server in Servers)
            {
                foreach (var player in server.FormattedPlayerList.Where(player => player.IsPlayerFoundInSearch))
                {
                    player.IsPlayerFoundInSearch = false;
                }
            }
        }

        /// <summary>
        /// Does the server browser automatic sort based on specified criteria.
        /// </summary>
        /// <param name="property">The property criteria.</param>
        private void DoServerBrowserAutoSort(string property)
        {
            var view = CollectionViewSource.GetDefaultView(Servers);
            var sortDescription = new SortDescription(property, ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortDescription);
        }

        /// <summary>
        /// Compares the player name that the user specifies with the player list of each visible server to see if there are any matches.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns><c>true</c> if a match is found, otherwise <c>false</c>.</returns>
        /// <remarks>Note: This method is executed for every single server in the collection when the player searches.</remarks>
        private bool EvaluatePlayerNameFilter(object o)
        {
            var server = o as ServerDetailsViewModel;
            if (server == null) { return false; }

            // Fully backspaced so reset...
            if (string.IsNullOrEmpty(PlayerSearchTerm))
            {
                ClearAndResetPlayerSearchTerm();
                return true;
            }
            // QL minimum name length is two, so show all results if user only enters 1 character.
            if (PlayerSearchTerm.Length < 2) { return true; }
            // This is on a per server (ServerDetailsViewModel) basis.
            foreach (var player in server.FormattedPlayerList.Where(player => player.Name.IndexOf(PlayerSearchTerm, StringComparison.InvariantCultureIgnoreCase) >= 0))
            {
                Debug.WriteLine("\n***Success! Match found: " + player.Name + " on server with id: " + server.PublicId + "***");
                PlayerNameFoundBuilder.Append(player.Name + " ");
                _playerSearchFoundCount++;
                HighlightFoundPlayer(player);
                UpdatePlayerSearchStatus();

                return true;
            }

            // Nothing found.
            UpdatePlayerSearchStatus();
            return false;
        }

        /// <summary>
        /// Sets a highlight flag on the player found in a player search in the player's <see cref="PlayerDetailsViewModel"/>
        /// </summary>
        /// <param name="player">The player.</param>
        private void HighlightFoundPlayer(PlayerDetailsViewModel player)
        {
            player.IsPlayerFoundInSearch = true;
        }

        /// <summary>
        /// Resets the player search status.
        /// </summary>
        private void ResetPlayerSearchStatus()
        {
            _playerSearchFoundCount = 0;
            ClearAllHighlightedPlayers();
            PlayerNameFoundBuilder.Clear();
        }

        /// <summary>
        /// Updates the player search result status.
        /// </summary>
        /// <remarks>This sends a number of events to the <see cref="MainViewModel"/> to reflect the changes in the statusbar.</remarks>
        private void UpdatePlayerSearchStatus()
        {
            _events.PublishOnUIThread(new PlayerSearchingEvent(true, PlayerSearchTerm));
            _events.PublishOnUIThread(new PlayerFoundCountEvent(_playerSearchFoundCount));
            _events.PublishOnUIThread(new PlayerFoundNameEvent(PlayerNameFoundBuilder.ToString()));
        }
    }
}