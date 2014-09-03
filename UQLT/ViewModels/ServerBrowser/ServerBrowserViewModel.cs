using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using UQLT.Core;
using UQLT.Core.Modules.ServerBrowser;
using UQLT.Events;
using UQLT.Interfaces;
using UQLT.Models.Configuration;

namespace UQLT.ViewModels.ServerBrowser
{
    /// <summary>
    /// Viewmodel for the Server Browser view.
    /// </summary>
    [Export(typeof(ServerBrowserViewModel))]
    public class ServerBrowserViewModel : PropertyChangedBase, IHandleWithTask<ServerRequestEvent>, IUqltConfiguration
    {
        private readonly Core.Modules.ServerBrowser.ServerBrowser _sb;
        private readonly ServerBrowserSearch _sbsearch;
        private int _autoRefreshIndex;

        private List<ServerBrowserRefreshItem> _autoRefreshItems = new List<ServerBrowserRefreshItem>
        {
                new ServerBrowserRefreshItem {Name = "every 30 seconds", Seconds = 30},
                new ServerBrowserRefreshItem {Name = "every 1 minute", Seconds = 60},
                new ServerBrowserRefreshItem {Name = "every 1.5 minutes", Seconds = 90},
                new ServerBrowserRefreshItem {Name = "every 5 minutes", Seconds = 300},
        };

        private int _autoRefreshSeconds;
        private bool _canRefreshAllServers;
        private bool _displayEloSearchOptions;
        private int _eloSearchIndex;
        private List<ServerBrowserEloItem> _eloSearchItems;
        private string _eloSearchValue;
        private string _filterUrl;
        private bool _isAutoRefreshEnabled;
        private bool _isSearchingEnabled;
        private bool _isUpdatingServers;
        private int _playerCount;
        private string _playerSearchTerm;
        private ServerDetailsViewModel _selectedServer;
        private ObservableCollection<ServerDetailsViewModel> _servers = new ObservableCollection<ServerDetailsViewModel>();
        private bool _serversContainDuelGames;
        private bool _serversContainQlRanksTeamGames;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerBrowserViewModel" /> class.
        /// </summary>
        /// <param name="events">The events that this viewmodel publishes and/or subscribes to.</param>
        [ImportingConstructor]
        public ServerBrowserViewModel(IEventAggregator events)
        {
            events.Subscribe(this);

            DoServerBrowserAutoSort("FullLocationName");
            LoadConfig();

            // Helpers for this viewmodel
            _sb = new Core.Modules.ServerBrowser.ServerBrowser(this, events);
            _sbsearch = new ServerBrowserSearch(this, events);

            // Source of items
            EloSearchItems = _sbsearch.AllEloTypes;

            // Hook the collection view source up to the collection of servers to enable ability to search for players on servers.
            ServersView = CollectionViewSource.GetDefaultView(Servers);
            ServersView.Filter = _sbsearch.EvaluatePlayerNameFilter;
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
        /// Gets or sets a value indicating whether the entire server list can be refreshed at the present time.
        /// </summary>
        /// <value>
        /// <c>true</c> if the server list is not already being refreshed and thus can be refreshed,
        /// otherwise <c>false</c>.
        /// </value>
        /// <remarks>
        /// This is also a Caliburn.Micro action guard that automatically hooks up IsEnabled in the View.
        /// See: https://caliburnmicro.codeplex.com/wikipage?title=All%20About%20Actions
        /// </remarks>
        public bool CanRefreshAllServers
        {
            get
            {
                return _canRefreshAllServers;
            }
            set
            {
                _canRefreshAllServers = value;
                NotifyOfPropertyChange(() => CanRefreshAllServers);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to display elo search options.
        /// </summary>
        /// <value>
        /// <c>true</c> if elo search options should be displayed, otherwise <c>false</c>.
        /// </value>
        public bool DisplayEloSearchOptions
        {
            get
            {
                return _displayEloSearchOptions;
            }
            set
            {
                _displayEloSearchOptions = value;
                NotifyOfPropertyChange(() => DisplayEloSearchOptions);
            }
        }

        /// <summary>
        /// Gets or sets the index of the elo search.
        /// </summary>
        /// <value>
        /// The index of the currently selected value in the elo search combo box on the view.
        /// </value>
        public int EloSearchIndex
        {
            get
            {
                return _eloSearchIndex;
            }
            set
            {
                _eloSearchIndex = value;
                NotifyOfPropertyChange(() => EloSearchIndex);
            }
        }

        /// <summary>
        /// Gets or sets the elo search items.
        /// </summary>
        /// <value>
        /// The elo search items.
        /// </value>
        /// <remarks>This is used for the combo box in the view.</remarks>
        public List<ServerBrowserEloItem> EloSearchItems
        {
            get
            {
                return _eloSearchItems;
            }
            set
            {
                _eloSearchItems = value;
                NotifyOfPropertyChange(() => EloSearchItems);
            }
        }

        /// <summary>
        /// Gets or sets the elo search value.
        /// </summary>
        /// <value>
        /// The elo search value.
        /// </value>
        public string EloSearchValue
        {
            get
            {
                return _eloSearchValue;
            }
            set
            {
                if (value == _eloSearchValue) return;
                _eloSearchValue = value;
                _sbsearch.ResetEloSearchStatus();
                NotifyOfPropertyChange(() => EloSearchValue);
                ServersView.Refresh();
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
        /// Gets a value indicating whether the search features (player search, elo search) are enabled
        /// </summary>
        /// <value>
        /// <c>true</c> if the server list is not being updated; otherwise, <c>false</c> if the server list is currently being updated.
        /// </value>
        public bool IsSearchingEnabled
        {
            get
            {
                return _isSearchingEnabled;
            }
            set
            {
                _isSearchingEnabled = value;
                NotifyOfPropertyChange(() => IsSearchingEnabled);
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
                // Disable searching while servers are being loaded.
                IsSearchingEnabled = value != true;
                // Disable manual refresh if servers are being loaded.
                CanRefreshAllServers = value != true;
                NotifyOfPropertyChange(() => IsUpdatingServers);
            }
        }

        /// <summary>
        /// Gets or sets the total player count.
        /// </summary>
        /// <value>
        /// The player count.
        /// </value>
        public int PlayerCount
        {
            get
            {
                return _playerCount;
            }
            set
            {
                _playerCount = value;
                NotifyOfPropertyChange(() => PlayerCount);
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
                _sbsearch.ResetPlayerSearchStatus();
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
        /// Gets or sets a value indicating whether the servers contain duel games.
        /// </summary>
        /// <value>
        /// <c>true</c> if servers contain duel game types, otherwise <c>false</c>.
        /// </value>
        /// <remarks>This is used to properly set elo search options in the UI.</remarks>
        public bool ServersContainDuelGames
        {
            get
            {
                return _serversContainDuelGames;
            }
            set
            {
                _serversContainDuelGames = value;
                NotifyOfPropertyChange(() => ServersContainDuelGames);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the servers contain QLRanks supported team game types.
        /// </summary>
        /// <value>
        /// <c>true</c> if servers contain QLRanks supported team game types, otherwise <c>false</c>.
        /// </value>
        /// <remarks>This is used to properly set elo search options in the UI.</remarks>
        public bool ServersContainQlRanksTeamGames
        {
            get
            {
                return _serversContainQlRanksTeamGames;
            }
            set
            {
                _serversContainQlRanksTeamGames = value;
                NotifyOfPropertyChange(() => ServersContainQlRanksTeamGames);
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
        /// Gets the <see cref="ServerBrowserSearch"/> associated with this viewmodel.
        /// </summary>
        /// <value>
        /// The associated <see cref="ServerBrowserSearch"/>.
        /// </value>
        public ServerBrowserSearch SrvBrowserSearch
        {
            get
            {
                return _sbsearch;
            }
        }

        /// <summary>
        /// Clears the and reset elo search value.
        /// </summary>
        /// <remarks>Caliburn.Micro requires methods called from View to be located within associated viewmodel, even if methods are actually called from another class.</remarks>
        public void ClearAndResetEloSearchValue()
        {
            SrvBrowserSearch.ClearAndResetEloSearchValue();
        }

        /// <summary>
        /// Clears and resets the player search term.
        /// </summary>
        /// <remarks>Caliburn.Micro requires methods called from View to be located within associated viewmodel, even if methods are actually called from another class.</remarks>
        public void ClearAndResetPlayerSearchTerm()
        {
            SrvBrowserSearch.ClearAndResetPlayerSearchTerm();
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
        /// Enables the elo search filter.
        /// </summary>
        /// <remarks>Caliburn.Micro requires methods called from View to be located within associated viewmodel, even if methods are actually called from another class.</remarks>
        public void EnableEloSearchFilter()
        {
            SrvBrowserSearch.EnableEloSearchFilter();
        }

        /// <summary>
        /// Enables the player search filter.
        /// </summary>
        /// <remarks>Caliburn.Micro requires methods called from View to be located within associated viewmodel, even if methods are actually called from another class.</remarks>
        public void EnablePlayerSearchFilter()
        {
            SrvBrowserSearch.EnablePlayerSearchFilter();
        }

        /// <summary>
        /// Handles the specified message (event) that is received whenever this viewmodel receive
        /// notice of a new default filter, either through the "make new default" button or "reset
        /// filters" button from the FilterViewModel.
        /// </summary>
        /// <param name="message">The message (event).</param>
        public async Task Handle(ServerRequestEvent message)
        {
            FilterUrl = message.ServerRequestUrl;
            await _sb.InitializeServersAsync(FilterUrl);
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
            DisplayEloSearchOptions = cfghandler.SbOptDisplayEloSearch;
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
        /// Refreshes the servers.
        /// </summary>
        /// <remarks>This is invokved when the user clicks the 'Refresh Servers' button in the view.</remarks>
        public async Task RefreshAllServers()
        {
            await _sb.InitializeServersAsync(FilterUrl);
            // Re-apply any searches after refreshing the list, if they exist.
            if (!string.IsNullOrEmpty(EloSearchValue))
            {
                SetCurrentEloSearchSelection(_sbsearch.ActiveEloSearchType);
                _sbsearch.EnableEloSearchFilter();
            }
            if (!string.IsNullOrEmpty(PlayerSearchTerm))
            {
                _sbsearch.EnablePlayerSearchFilter();
            }
        }

        /// <summary>
        /// Refreshes the selected server.
        /// </summary>
        /// <remarks>Note: Although collections are used, only one individual server can be refreshed at a time.
        /// Collections reflect the servers array data format returned by the QL API.</remarks>
        public async Task RefreshSelectedServer()
        {
            if (SelectedServer == null) { return; }

            var selectedservers = await _sb.GetServersFromDetailsUrlAsync(UQltGlobals.QlDomainDetailsIds + SelectedServer.PublicId);
            ServerDetailsViewModel newserver = null;

            // .ToList() is required in this foreach loop as .NET modifies the contents of the Servers under the hood, causing an error.
            // See: http://stackoverflow.com/questions/604831/collection-was-modified-enumeration-operation-may-not-execute
            foreach (var server in Servers.Where(server => SelectedServer.PublicId == server.PublicId).ToList())
            {
                Servers.Remove(server);
            }
            foreach (var selectedserver in selectedservers)
            {
                newserver = new ServerDetailsViewModel(selectedserver);
                Servers.Add(newserver);
                var pinger = new ServerPinger();
                await pinger.SetPingInformationAsync(newserver);
                await newserver.SetPlayerElosView();
            }

            _sb.GetAndUpdatePlayerCount(Servers);

            // Selected server in the view now reflects the update.
            SelectedServer = newserver;
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
            cfghandler.SbOptDisplayEloSearch = DisplayEloSearchOptions;

            cfghandler.WriteConfig();
        }

        /// <summary>
        /// Sets the current elo search selection.
        /// </summary>
        /// <param name="searchtype">The searchtype.</param>
        /// <remarks>Caliburn.Micro requires methods called from View to be located within associated viewmodel, even if methods are actually called from another class.</remarks>
        public void SetCurrentEloSearchSelection(EloSearchTypes searchtype)
        {
            SrvBrowserSearch.SetCurrentEloSearchSelection(searchtype);
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
        /// Does the server browser automatic sort based on specified criteria.
        /// </summary>
        /// <param name="property">The property criteria.</param>
        private void DoServerBrowserAutoSort(string property)
        {
            var view = CollectionViewSource.GetDefaultView(Servers);
            var sortDescription = new SortDescription(property, ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortDescription);
        }
    }
}