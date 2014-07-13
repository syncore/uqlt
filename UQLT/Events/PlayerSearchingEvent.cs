namespace UQLT.Events
{
    /// <summary>
    /// Event that is fired whenever we receive indication that user is searching for a player by name.
    /// </summary>
    public class PlayerSearchingEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerSearchingEvent" /> class.
        /// </summary>
        /// <param name="issearching">if set to <c>true</c> then a search is being conducted.</param>
        /// <param name="searchterm">The search term.</param>
        public PlayerSearchingEvent(bool issearching, string searchterm)
        {
            IsSearching = issearching;
            SearchTerm = searchterm;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a player search is currently being conducted.
        /// </summary>
        /// <value>
        /// <c>true</c> if searching; otherwise, <c>false</c>.
        /// </value>
        public bool IsSearching
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the search term.
        /// </summary>
        /// <value>
        /// The search term.
        /// </value>
        public string SearchTerm
        {
            get;
            set;
        }
    }
}