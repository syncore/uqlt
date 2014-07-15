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
        public static string DuelMaxSearchString = "duelers with MAXIMUM Elo of";
        public static string DuelMinSearchString = "duelers with minimum Elo of";
        public static string TeamBothTeamsMaxSearchString = "both teams with MAXIMUM average Elo of";
        public static string TeamBothTeamsMinSearchString = "both teams with minimum average Elo of";
        public static string TeamOneTeamMaxSearchString = "at least one team with MAXIMUM average Elo of";
        public static string TeamOneTeamMinSearchString = "at least one team with minimum average Elo of";

        private readonly ObservableCollection<ServerBrowserEloItem> _allEloTypes = new ObservableCollection<ServerBrowserEloItem>()
        {
            new ServerBrowserEloItem {Name = DuelMinSearchString, EloSearchGameType = EloSearchTypes.DuelMinSearch},
            new ServerBrowserEloItem {Name = DuelMaxSearchString, EloSearchGameType = EloSearchTypes.DuelMaxSearch},
            new ServerBrowserEloItem {Name = TeamOneTeamMinSearchString, EloSearchGameType = EloSearchTypes.TeamOneTeamMinSearch},
            new ServerBrowserEloItem {Name = TeamBothTeamsMinSearchString, EloSearchGameType = EloSearchTypes.TeamBothTeamsMinSearch},
            new ServerBrowserEloItem {Name = TeamOneTeamMaxSearchString, EloSearchGameType = EloSearchTypes.TeamOneTeamMaxSearch},
            new ServerBrowserEloItem {Name = TeamBothTeamsMaxSearchString, EloSearchGameType = EloSearchTypes.TeamBothTeamsMaxSearch},
        };

        private readonly ObservableCollection<ServerBrowserEloItem> _duelEloTypes = new ObservableCollection<ServerBrowserEloItem>()
        {
            new ServerBrowserEloItem {Name = DuelMinSearchString, EloSearchGameType = EloSearchTypes.DuelMinSearch},
            new ServerBrowserEloItem {Name = DuelMaxSearchString, EloSearchGameType = EloSearchTypes.DuelMaxSearch}
        };

        private readonly IEventAggregator _events;
        private readonly StringBuilder _playerSearchFoundBuilder = new StringBuilder();
        private readonly ServerBrowser _sb;

        private readonly ObservableCollection<ServerBrowserEloItem> _teamEloTypes = new ObservableCollection<ServerBrowserEloItem>()
        {
            new ServerBrowserEloItem {Name = TeamOneTeamMinSearchString, EloSearchGameType = EloSearchTypes.TeamOneTeamMinSearch},
            new ServerBrowserEloItem {Name = TeamBothTeamsMinSearchString, EloSearchGameType = EloSearchTypes.TeamBothTeamsMinSearch},
            new ServerBrowserEloItem {Name = TeamOneTeamMaxSearchString, EloSearchGameType = EloSearchTypes.TeamOneTeamMaxSearch},
            new ServerBrowserEloItem {Name = TeamBothTeamsMaxSearchString, EloSearchGameType = EloSearchTypes.TeamBothTeamsMaxSearch}
        };

        private int _autoRefreshIndex;

        private List<ServerBrowserRefreshItem> _autoRefreshItems = new List<ServerBrowserRefreshItem>
        {
                new ServerBrowserRefreshItem {Name = "every 30 seconds", Seconds = 30},
                new ServerBrowserRefreshItem {Name = "every 1 minute", Seconds = 60},
                new ServerBrowserRefreshItem {Name = "every 1.5 minutes", Seconds = 90},
                new ServerBrowserRefreshItem {Name = "every 5 minutes", Seconds = 300},
        };

        private int _autoRefreshSeconds;

        private bool _displayEloSearchOptions;
        private int _eloSearchFoundCount;
        private int _eloSearchIndex;
        private ObservableCollection<ServerBrowserEloItem> _eloSearchItems;
        private string _eloSearchValue;
        private string _filterUrl;
        private bool _isAutoRefreshEnabled;
        private bool _isSearchingEnabled;
        private bool _isUpdatingServers;
        private int _playerSearchFoundCount;
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
            _events = events;
            _events.Subscribe(this);

            DoServerBrowserAutoSort("FullLocationName");
            EloSearchItems = _allEloTypes;
            LoadConfig();

            // Instantiate a new server browser for this viewmodel
            _sb = new ServerBrowser(this, events);

            // Hook the collection view source up to the collection of servers to enable ability to search for players on servers.
            ServersView = CollectionViewSource.GetDefaultView(Servers);
            ServersView.Filter = EvaluatePlayerNameFilter;
        }

        /// <summary>
        /// Gets or sets the currently active types of elo search possibilities for filtering purposes.
        /// </summary>
        /// <value>
        /// The currently active types of elo search possibilities.
        /// </value>
        public EloSearchCategoryTypes ActiveEloSearchCategories
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the currently active type of elo search being conducted for filtering purposes.
        /// </summary>
        /// <value>
        /// The currently active type of elo search.
        /// </value>
        /// <remarks>This is directly based on the value that the user has selected in the elo
        /// search combobox in the view.</remarks>
        public EloSearchTypes ActiveEloSearchType
        {
            get;
            set;
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
        public ObservableCollection<ServerBrowserEloItem> EloSearchItems
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
                ResetEloSearchStatus();
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
        /// Clears and resets the elo search value.
        /// </summary>
        public void ClearAndResetEloSearchValue()
        {
            EloSearchValue = string.Empty;
            // Statusbar
            _events.PublishOnUIThread(new EloSearchingEvent(false, string.Empty));
            _events.PublishOnUIThread(new EloFoundCountEvent(0));
            _events.PublishOnUIThread(new EloSearchTypeEvent(EloSearchTypes.None));
        }

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

        public void EnableEloSearchFilter()
        {
            PlayerSearchTerm = string.Empty;
            ServersView.Filter = null;
            ServersView.Filter = EvaluateEloValueFilter;
        }

        public void EnablePlayerSearchFilter()
        {
            EloSearchValue = null;
            ServersView.Filter = null;
            ServersView.Filter = EvaluatePlayerNameFilter;
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
        /// Sets the current type of elo search being conducted based on the value of the index selected in the view.
        /// </summary>
        /// <param name="searchtype">The currently selected game search type (EloSearchGameType) on the <see cref="ServerBrowserEloItem"/></param>
        /// <remarks>This is used to determine what the user has selected in the elo search combo box
        /// in the view for filtering purposes.</remarks>
        public void SetCurrentEloSearchSelection(EloSearchTypes searchtype)
        {
            Debug.WriteLine(">>> Got current selection of: " + searchtype);

            switch (ActiveEloSearchCategories)
            {
                case EloSearchCategoryTypes.BothDuelAndTeamGames:
                    switch (searchtype)
                    {
                        case EloSearchTypes.DuelMinSearch:
                            ActiveEloSearchType = EloSearchTypes.DuelMinSearch;
                            break;

                        case EloSearchTypes.DuelMaxSearch:
                            ActiveEloSearchType = EloSearchTypes.DuelMaxSearch;
                            break;

                        case EloSearchTypes.TeamOneTeamMinSearch:
                            ActiveEloSearchType = EloSearchTypes.TeamOneTeamMinSearch;
                            break;

                        case EloSearchTypes.TeamBothTeamsMinSearch:
                            ActiveEloSearchType = EloSearchTypes.TeamBothTeamsMinSearch;
                            break;

                        case EloSearchTypes.TeamOneTeamMaxSearch:
                            ActiveEloSearchType = EloSearchTypes.TeamOneTeamMaxSearch;
                            break;

                        case EloSearchTypes.TeamBothTeamsMaxSearch:
                            ActiveEloSearchType = EloSearchTypes.TeamBothTeamsMaxSearch;
                            break;
                    }
                    break;

                case EloSearchCategoryTypes.DuelGamesOnly:
                    switch (searchtype)
                    {
                        case EloSearchTypes.DuelMinSearch:
                            ActiveEloSearchType = EloSearchTypes.DuelMinSearch;
                            break;

                        case EloSearchTypes.DuelMaxSearch:
                            ActiveEloSearchType = EloSearchTypes.DuelMaxSearch;
                            break;
                    }
                    break;

                case EloSearchCategoryTypes.TeamGamesOnly:
                    switch (searchtype)
                    {
                        case EloSearchTypes.TeamOneTeamMinSearch:
                            ActiveEloSearchType = EloSearchTypes.TeamOneTeamMinSearch;
                            break;

                        case EloSearchTypes.TeamBothTeamsMinSearch:
                            ActiveEloSearchType = EloSearchTypes.TeamBothTeamsMinSearch;
                            break;

                        case EloSearchTypes.TeamOneTeamMaxSearch:
                            ActiveEloSearchType = EloSearchTypes.TeamOneTeamMaxSearch;
                            break;

                        case EloSearchTypes.TeamBothTeamsMaxSearch:
                            ActiveEloSearchType = EloSearchTypes.TeamBothTeamsMaxSearch;
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Sets the elo search collection based on the type of elo search.
        /// </summary>
        /// <param name="searchtype">The search type.</param>
        public void SetEloSearchCollectionSource(EloSearchCategoryTypes searchtype)
        {
            Debug.WriteLine("Setting appropriate collection type to: " + searchtype);

            switch (searchtype)
            {
                case EloSearchCategoryTypes.BothDuelAndTeamGames:
                    EloSearchItems = _allEloTypes;
                    ActiveEloSearchCategories = EloSearchCategoryTypes.BothDuelAndTeamGames;
                    break;

                case EloSearchCategoryTypes.DuelGamesOnly:
                    EloSearchItems = _duelEloTypes;
                    ActiveEloSearchCategories = EloSearchCategoryTypes.DuelGamesOnly;
                    break;

                case EloSearchCategoryTypes.TeamGamesOnly:
                    EloSearchItems = _teamEloTypes;
                    ActiveEloSearchCategories = EloSearchCategoryTypes.TeamGamesOnly;
                    break;
            }
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
        /// Compares the numeric elo value that the user specifies with players elos (in the case of duel) or team
        /// average elos in the case of team games to see if there are any matches.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns><c>true</c> if a match is found, otherwise <c>false</c>.</returns>
        /// <remarks>Note: this method is executed for every single server in the collection when the player searches.</remarks>
        private bool EvaluateEloValueFilter(object o)
        {
            var server = o as ServerDetailsViewModel;
            if (server == null) { return false; }

            // Fully backspaced so reset...
            if (string.IsNullOrEmpty(EloSearchValue))
            {
                ClearAndResetEloSearchValue();
                return true;
            }
            // Remember, this is on a per server (ServerDetailsViewModel) basis.
            // Minimum elo duel search.
            switch (ActiveEloSearchType)
            {
                case EloSearchTypes.DuelMinSearch:
                    if (server.GameType != 1) return false;
                    foreach (var player in server.FormattedPlayerList.Where(player => player.Team == 0).Where(player => player.PlayerDuelElo >= Convert.ToInt64(EloSearchValue)))
                    {
                        Debug.WriteLine("\n***Success! MINIMUM Duel Elo: " + EloSearchValue + " found for player: " + player.Name + " team: " + player.Team + " elo: " + player.PlayerDuelElo + " on server with id: " + server.PublicId + "***");
                        _eloSearchFoundCount++;
                        HighlightFoundPlayer(player);
                        UpdateEloSearchStatus();
                        return true;
                    }
                    break;

                case EloSearchTypes.DuelMaxSearch:
                    if (server.GameType != 1) return false;
                    foreach (var player in server.FormattedPlayerList.Where(player => player.Team == 0).Where(player => player.PlayerDuelElo <= Convert.ToInt64(EloSearchValue)))
                    {
                        Debug.WriteLine("\n***Success! MAXIMUM Duel Elo: " + EloSearchValue + " found for player: " + player.Name + " team: " + player.Team + " elo: " + player.PlayerDuelElo + " on server with id: " + server.PublicId + "***");
                        _eloSearchFoundCount++;
                        HighlightFoundPlayer(player);
                        UpdateEloSearchStatus();
                        return true;
                    }
                    break;

                case EloSearchTypes.TeamOneTeamMinSearch:
                    if (!server.IsQlRanksSupportedTeamGame) { return false; }
                    server.DoTeamEloRetrieval();
                    if (server.BlueTeamElo >= Convert.ToInt64(EloSearchValue) || server.RedTeamElo >= Convert.ToInt64(EloSearchValue))
                    {
                        Debug.WriteLine("\n***Success! At least one team has MINIMUM Elo of: " + EloSearchValue +
                                        " blue avg Elo: " + server.BlueTeamElo + ", red avg Elo: " + server.RedTeamElo + " on server with id: " + server.PublicId + "***");
                        _eloSearchFoundCount++;
                        UpdateEloSearchStatus();
                        return true;
                    }
                    break;

                case EloSearchTypes.TeamBothTeamsMinSearch:
                    if (!server.IsQlRanksSupportedTeamGame) { return false; }
                    server.DoTeamEloRetrieval();
                    if ((server.BlueTeamElo >= Convert.ToInt64(EloSearchValue)) && (server.RedTeamElo >= Convert.ToInt64(EloSearchValue)))
                    {
                        Debug.WriteLine("\n***Success! Both teams have MINIMUM Elo of: " + EloSearchValue +
                                        " blue avg Elo: " + server.BlueTeamElo + ", red avg Elo: " + server.RedTeamElo + " on server with id: " + server.PublicId + "***");
                        _eloSearchFoundCount++;
                        UpdateEloSearchStatus();
                        return true;
                    }
                    break;

                case EloSearchTypes.TeamOneTeamMaxSearch:
                    if (!server.IsQlRanksSupportedTeamGame) { return false; }
                    server.DoTeamEloRetrieval();
                    if (server.BlueTeamElo <= Convert.ToInt64(EloSearchValue) || server.RedTeamElo <= Convert.ToInt64(EloSearchValue))
                    {
                        Debug.WriteLine("\n***Success! At least one team has MAXIMUM Elo of: " + EloSearchValue +
                                        " blue avg Elo: " + server.BlueTeamElo + ", red avg Elo: " + server.RedTeamElo + " on server with id: " + server.PublicId + "***");
                        _eloSearchFoundCount++;
                        UpdateEloSearchStatus();
                        return true;
                    }
                    break;

                case EloSearchTypes.TeamBothTeamsMaxSearch:
                    if (!server.IsQlRanksSupportedTeamGame) { return false; }
                    server.DoTeamEloRetrieval();
                    if ((server.BlueTeamElo <= Convert.ToInt64(EloSearchValue)) && (server.RedTeamElo <= Convert.ToInt64(EloSearchValue)))
                    {
                        Debug.WriteLine("\n***Success! Both teams have MAXIMUM Elo of: " + EloSearchValue +
                                        " blue avg Elo: " + server.BlueTeamElo + ", red avg Elo: " + server.RedTeamElo + " on server with id: " + server.PublicId + "***");
                        _eloSearchFoundCount++;
                        UpdateEloSearchStatus();
                        return true;
                    }
                    break;
            }

            // Nothing found.
            UpdateEloSearchStatus();
            return false;
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
            // Remember, this is on a per server (ServerDetailsViewModel) basis.
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
        /// Resets the elo search status.
        /// </summary>
        private void ResetEloSearchStatus()
        {
            _eloSearchFoundCount = 0;
            ClearAllHighlightedPlayers();
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
        /// Updates the elo search result status.
        /// </summary>
        /// <remarks>This sends a number of events to the <see cref="MainViewModel"/> to reflect the changes in the statusbar.</remarks>
        private void UpdateEloSearchStatus()
        {
            _events.PublishOnUIThread(new EloSearchingEvent(true, EloSearchValue));
            _events.PublishOnUIThread(new EloFoundCountEvent(_eloSearchFoundCount));
            _events.PublishOnUIThread(new EloSearchTypeEvent(ActiveEloSearchType));
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