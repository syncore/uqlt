using UQLT.Core.Modules.ServerBrowser;

namespace UQLT.Events
{
    /// <summary>
    /// Event that is fired that describes the type of elo search being conducted
    ///  whenever we receive indication that user is searching for a player by elo.
    /// </summary>
    public class EloSearchTypeEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EloSearchTypeEvent" /> class.
        /// </summary>

        private string _searchTypeText;

        public EloSearchTypeEvent(EloSearchTypes searchtype)
        {
            SearchType = searchtype;
        }

        /// <summary>
        /// Gets or sets the type of search.
        /// </summary>
        /// <value>
        /// The type of search.
        /// </value>
        public EloSearchTypes SearchType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the search type text.
        /// </summary>
        /// <value>
        /// The search type text.
        /// </value>
        public string SearchTypeText
        {
            get
            {
                return DetermineSearchTypeText(SearchType);
            }
        }

        /// <summary>
        /// Determines the search type text.
        /// </summary>
        /// <param name="searchtype">The search type.</param>
        /// <returns>A string containing the type of search in English.</returns>
        private string DetermineSearchTypeText(EloSearchTypes searchtype)
        {
            switch (searchtype)
            {
                case EloSearchTypes.DuelMinSearch:
                    _searchTypeText = ServerBrowserSearch.DuelMinSearchString;
                    break;

                case EloSearchTypes.DuelMaxSearch:
                    _searchTypeText = ServerBrowserSearch.DuelMaxSearchString;
                    break;

                case EloSearchTypes.TeamOneTeamMinSearch:
                    _searchTypeText = ServerBrowserSearch.TeamOneTeamMinSearchString;
                    break;

                case EloSearchTypes.TeamBothTeamsMinSearch:
                    _searchTypeText = ServerBrowserSearch.TeamBothTeamsMinSearchString;
                    break;

                case EloSearchTypes.TeamOneTeamMaxSearch:
                    _searchTypeText = ServerBrowserSearch.TeamOneTeamMaxSearchString;
                    break;

                case EloSearchTypes.TeamBothTeamsMaxSearch:
                    _searchTypeText = ServerBrowserSearch.TeamBothTeamsMaxSearchString;
                    break;
                // This is the only place that type None is used.
                case EloSearchTypes.None:
                    _searchTypeText = string.Empty;
                    break;
            }
            return _searchTypeText;
        }
    }
}