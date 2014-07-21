using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using UQLT.Events;
using UQLT.Models.Configuration;
using UQLT.ViewModels;

namespace UQLT.Core.ServerBrowser
{
    /// <summary>
    /// Helper class responsible for searching servers based on user specified criteria, which can be
    /// player name or certain elo criteria for a <see cref="ServerBrowserViewModel"/>
    /// </summary>
    
    public class ServerBrowserSearch
    {
        public static string DuelMaxSearchString = "duelers with MAXIMUM Elo of";

        public static string DuelMinSearchString = "duelers with minimum Elo of";

        public static string TeamBothTeamsMaxSearchString = "both teams with MAXIMUM average Elo of";

        public static string TeamBothTeamsMinSearchString = "both teams with minimum average Elo of";

        public static string TeamOneTeamMaxSearchString = "at least one team with MAXIMUM average Elo of";

        public static string TeamOneTeamMinSearchString = "at least one team with minimum average Elo of";

        private readonly List<ServerBrowserEloItem> _allEloTypes = new List<ServerBrowserEloItem>
        {
            new ServerBrowserEloItem {Name = DuelMinSearchString, EloSearchGameType = EloSearchTypes.DuelMinSearch},
            new ServerBrowserEloItem {Name = DuelMaxSearchString, EloSearchGameType = EloSearchTypes.DuelMaxSearch},
            new ServerBrowserEloItem {Name = TeamOneTeamMinSearchString, EloSearchGameType = EloSearchTypes.TeamOneTeamMinSearch},
            new ServerBrowserEloItem {Name = TeamBothTeamsMinSearchString, EloSearchGameType = EloSearchTypes.TeamBothTeamsMinSearch},
            new ServerBrowserEloItem {Name = TeamOneTeamMaxSearchString, EloSearchGameType = EloSearchTypes.TeamOneTeamMaxSearch},
            new ServerBrowserEloItem {Name = TeamBothTeamsMaxSearchString, EloSearchGameType = EloSearchTypes.TeamBothTeamsMaxSearch},
        };

        private readonly List<ServerBrowserEloItem> _duelEloTypes = new List<ServerBrowserEloItem>
        {
            new ServerBrowserEloItem {Name = DuelMinSearchString, EloSearchGameType = EloSearchTypes.DuelMinSearch},
            new ServerBrowserEloItem {Name = DuelMaxSearchString, EloSearchGameType = EloSearchTypes.DuelMaxSearch}
        };

        private readonly IEventAggregator _events;

        private readonly StringBuilder _playerSearchFoundBuilder = new StringBuilder();

        private readonly List<ServerBrowserEloItem> _teamEloTypes = new List<ServerBrowserEloItem>
        {
            new ServerBrowserEloItem {Name = TeamOneTeamMinSearchString, EloSearchGameType = EloSearchTypes.TeamOneTeamMinSearch},
            new ServerBrowserEloItem {Name = TeamBothTeamsMinSearchString, EloSearchGameType = EloSearchTypes.TeamBothTeamsMinSearch},
            new ServerBrowserEloItem {Name = TeamOneTeamMaxSearchString, EloSearchGameType = EloSearchTypes.TeamOneTeamMaxSearch},
            new ServerBrowserEloItem {Name = TeamBothTeamsMaxSearchString, EloSearchGameType = EloSearchTypes.TeamBothTeamsMaxSearch}
        };

        private int _playerSearchFoundCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerBrowserSearch"/> class.
        /// </summary>
        /// <param name="sbvm">The associated <see cref="ServerBrowserViewModel"/>.</param>
        /// <param name="events">The events.</param>
        public ServerBrowserSearch(ServerBrowserViewModel sbvm, IEventAggregator events)
        {
            Sbvm = sbvm;
            _events = events;
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
        /// Gets all elo types that can be searched for the current server list.
        /// </summary>
        /// <value>
        /// All elo types that can be searched for the current server list.
        /// </value>
        public List<ServerBrowserEloItem> AllEloTypes
        {
            get
            {
                return _allEloTypes;
            }
        }

        /// <summary>
        /// Gets duel elo types that can be searched for the current server list.
        /// </summary>
        /// <value>
        /// Duel elo types that can be searched for the current server list.
        /// </value>
        public List<ServerBrowserEloItem> DuelEloTypes
        {
            get
            {
                return _duelEloTypes;
            }
        }

        /// <summary>
        /// Gets or sets the elo search found count.
        /// </summary>
        /// <value>
        /// The elo search found count.
        /// </value>
        public int EloSearchFoundCount
        {
            get;
            set;
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
        /// Gets or sets the player search found count.
        /// </summary>
        /// <value>
        /// The player search found count.
        /// </value>
        public int PlayerSearchFoundCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="ServerBrowserViewModel"/> associated with this class.
        /// </summary>
        /// <value>
        /// The <see cref="ServerBrowserViewModel"/> associated with this class.
        /// </value>
        public ServerBrowserViewModel Sbvm
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all team elo types that can be searched for the current server list.
        /// </summary>
        /// <value>
        /// All team elo types that can be searched for the current server list.
        /// </value>
        public List<ServerBrowserEloItem> TeamEloTypes
        {
            get
            {
                return _teamEloTypes;
            }
        }

        /// <summary>
        /// Clears and resets the elo search value.
        /// </summary>
        public void ClearAndResetEloSearchValue()
        {
            Sbvm.EloSearchValue = string.Empty;
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
            Sbvm.PlayerSearchTerm = string.Empty;
            // Statusbar
            _events.PublishOnUIThread(new PlayerSearchingEvent(false, string.Empty));
            _events.PublishOnUIThread(new PlayerFoundCountEvent(0));
            _events.PublishOnUIThread(new PlayerFoundNameEvent(string.Empty));
            PlayerNameFoundBuilder.Clear();
        }

        /// <summary>
        /// Enables the elo search filter.
        /// </summary>
        public void EnableEloSearchFilter()
        {
            // Clear player search when now performing elo search.
            Sbvm.PlayerSearchTerm = string.Empty;
            Sbvm.ServersView.Filter = null;
            Sbvm.ServersView.Filter = EvaluateEloValueFilter;
        }

        /// <summary>
        /// Enables the player search filter.
        /// </summary>
        public void EnablePlayerSearchFilter()
        {
            // Clear elo search when now performing player search.
            Sbvm.EloSearchValue = string.Empty;
            Sbvm.ServersView.Filter = null;
            Sbvm.ServersView.Filter = EvaluatePlayerNameFilter;
        }

        /// <summary>
        /// Compares the numeric elo value that the user specifies with players elos (in the case of duel) or team
        /// average elos in the case of team games to see if there are any matches.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns><c>true</c> if a match is found, otherwise <c>false</c>.</returns>
        /// <remarks>Note: this method is executed for every single server in the collection when the player searches.</remarks>
        public bool EvaluateEloValueFilter(object o)
        {
            var server = o as ServerDetailsViewModel;
            if (server == null) { return false; }

            // Fully backspaced so reset...
            if (string.IsNullOrEmpty(Sbvm.EloSearchValue))
            {
                ClearAndResetEloSearchValue();
                return true;
            }
            // Remember, this is on a per server (ServerDetailsViewModel) basis.
            switch (ActiveEloSearchType)
            {
                case EloSearchTypes.DuelMinSearch:
                    if (server.GameType != 1) return false;
                    foreach (var player in server.FormattedPlayerList.Where(player => player.Team == 0).Where(player => player.PlayerDuelElo >= Convert.ToInt64(Sbvm.EloSearchValue)))
                    {
                        Debug.WriteLine("\n***Success! MINIMUM Duel Elo: " + Sbvm.EloSearchValue + " found for player: " + player.Name + " team: " + player.Team + " elo: " + player.PlayerDuelElo + " on server with id: " + server.PublicId + "***");
                        EloSearchFoundCount++;
                        HighlightFoundPlayer(player);
                        UpdateEloSearchStatus();
                        return true;
                    }
                    break;

                case EloSearchTypes.DuelMaxSearch:
                    if (server.GameType != 1) return false;
                    foreach (var player in server.FormattedPlayerList.Where(player => player.Team == 0).Where(player => player.PlayerDuelElo <= Convert.ToInt64(Sbvm.EloSearchValue)))
                    {
                        Debug.WriteLine("\n***Success! MAXIMUM Duel Elo: " + Sbvm.EloSearchValue + " found for player: " + player.Name + " team: " + player.Team + " elo: " + player.PlayerDuelElo + " on server with id: " + server.PublicId + "***");
                        EloSearchFoundCount++;
                        HighlightFoundPlayer(player);
                        UpdateEloSearchStatus();
                        return true;
                    }
                    break;

                case EloSearchTypes.TeamOneTeamMinSearch:
                    if (!server.IsQlRanksSupportedTeamGame) { return false; }
                    server.DoTeamEloRetrieval();
                    if (server.BlueTeamElo >= Convert.ToInt64(Sbvm.EloSearchValue) || server.RedTeamElo >= Convert.ToInt64(Sbvm.EloSearchValue))
                    {
                        Debug.WriteLine("\n***Success! At least one team has MINIMUM Elo of: " + Sbvm.EloSearchValue +
                                        " blue avg Elo: " + server.BlueTeamElo + ", red avg Elo: " + server.RedTeamElo + " on server with id: " + server.PublicId + "***");
                        EloSearchFoundCount++;
                        UpdateEloSearchStatus();
                        return true;
                    }
                    break;

                case EloSearchTypes.TeamBothTeamsMinSearch:
                    if (!server.IsQlRanksSupportedTeamGame) { return false; }
                    server.DoTeamEloRetrieval();
                    if ((server.BlueTeamElo >= Convert.ToInt64(Sbvm.EloSearchValue)) && (server.RedTeamElo >= Convert.ToInt64(Sbvm.EloSearchValue)))
                    {
                        Debug.WriteLine("\n***Success! Both teams have MINIMUM Elo of: " + Sbvm.EloSearchValue +
                                        " blue avg Elo: " + server.BlueTeamElo + ", red avg Elo: " + server.RedTeamElo + " on server with id: " + server.PublicId + "***");
                        EloSearchFoundCount++;
                        UpdateEloSearchStatus();
                        return true;
                    }
                    break;

                case EloSearchTypes.TeamOneTeamMaxSearch:
                    if (!server.IsQlRanksSupportedTeamGame) { return false; }
                    if (server.NumPlayers == 0) {return false; }
                    server.DoTeamEloRetrieval();
                    if (server.BlueTeamElo <= Convert.ToInt64(Sbvm.EloSearchValue) || server.RedTeamElo <= Convert.ToInt64(Sbvm.EloSearchValue))
                    {
                        Debug.WriteLine("\n***Success! At least one team has MAXIMUM Elo of: " + Sbvm.EloSearchValue +
                                        " blue avg Elo: " + server.BlueTeamElo + ", red avg Elo: " + server.RedTeamElo + " on server with id: " + server.PublicId + "***");
                        EloSearchFoundCount++;
                        UpdateEloSearchStatus();
                        return true;
                    }
                    break;

                case EloSearchTypes.TeamBothTeamsMaxSearch:
                    if (!server.IsQlRanksSupportedTeamGame) { return false; }
                    if (server.NumPlayers == 0) { return false; }
                    server.DoTeamEloRetrieval();
                    if ((server.BlueTeamElo <= Convert.ToInt64(Sbvm.EloSearchValue)) && (server.RedTeamElo <= Convert.ToInt64(Sbvm.EloSearchValue)))
                    {
                        Debug.WriteLine("\n***Success! Both teams have MAXIMUM Elo of: " + Sbvm.EloSearchValue +
                                        " blue avg Elo: " + server.BlueTeamElo + ", red avg Elo: " + server.RedTeamElo + " on server with id: " + server.PublicId + "***");
                        EloSearchFoundCount++;
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
        public bool EvaluatePlayerNameFilter(object o)
        {
            var server = o as ServerDetailsViewModel;
            if (server == null) { return false; }

            // Fully backspaced so reset...
            if (string.IsNullOrEmpty(Sbvm.PlayerSearchTerm))
            {
                ClearAndResetPlayerSearchTerm();
                return true;
            }
            // QL minimum name length is two, so show all results if user only enters 1 character.
            if (Sbvm.PlayerSearchTerm.Length < 2) { return true; }
            // Remember, this is on a per server (ServerDetailsViewModel) basis.
            foreach (var player in server.FormattedPlayerList.Where(player => player.Name.IndexOf(Sbvm.PlayerSearchTerm, StringComparison.InvariantCultureIgnoreCase) >= 0))
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
        /// Resets the elo search status.
        /// </summary>
        public void ResetEloSearchStatus()
        {
            EloSearchFoundCount = 0;
            ClearAllHighlightedPlayers();
        }

        /// <summary>
        /// Resets the player search status.
        /// </summary>
        /// <summary>
        /// Resets the player search status.
        /// </summary>
        public void ResetPlayerSearchStatus()
        {
            PlayerSearchFoundCount = 0;
            ClearAllHighlightedPlayers();
            PlayerNameFoundBuilder.Clear();
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
                    Sbvm.EloSearchItems = _allEloTypes;
                    ActiveEloSearchCategories = EloSearchCategoryTypes.BothDuelAndTeamGames;
                    break;

                case EloSearchCategoryTypes.DuelGamesOnly:
                    Sbvm.EloSearchItems = _duelEloTypes;
                    ActiveEloSearchCategories = EloSearchCategoryTypes.DuelGamesOnly;
                    break;

                case EloSearchCategoryTypes.TeamGamesOnly:
                    Sbvm.EloSearchItems = _teamEloTypes;
                    ActiveEloSearchCategories = EloSearchCategoryTypes.TeamGamesOnly;
                    break;
            }
        }

        /// <summary>
        /// Clears all highlighted players found in a player search.
        /// </summary>
        private void ClearAllHighlightedPlayers()
        {
            foreach (var player in Sbvm.Servers.SelectMany(server => server.FormattedPlayerList.Where(player => player.IsPlayerFoundInSearch)))
            {
                player.IsPlayerFoundInSearch = false;
            }
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
        /// Updates the elo search result status.
        /// </summary>
        /// <remarks>This sends a number of events to the <see cref="MainViewModel"/> to reflect the changes in the statusbar.</remarks>
        private void UpdateEloSearchStatus()
        {
            _events.PublishOnUIThread(new EloSearchingEvent(true, Sbvm.EloSearchValue));
            _events.PublishOnUIThread(new EloFoundCountEvent(EloSearchFoundCount));
            _events.PublishOnUIThread(new EloSearchTypeEvent(ActiveEloSearchType));
        }

        /// <summary>
        /// Updates the player search result status.
        /// </summary>
        /// <remarks>This sends a number of events to the <see cref="MainViewModel"/> to reflect the changes in the statusbar.</remarks>
        private void UpdatePlayerSearchStatus()
        {
            _events.PublishOnUIThread(new PlayerSearchingEvent(true, Sbvm.PlayerSearchTerm));
            _events.PublishOnUIThread(new PlayerFoundCountEvent(_playerSearchFoundCount));
            _events.PublishOnUIThread(new PlayerFoundNameEvent(PlayerNameFoundBuilder.ToString()));
        }
    }
}