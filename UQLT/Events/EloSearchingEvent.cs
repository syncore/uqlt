namespace UQLT.Events
{
    /// <summary>
    /// Event that is fired whenever we receive indication that user is searching for a player by elo.
    /// </summary>
    public class EloSearchingEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EloSearchingEvent" /> class.
        /// </summary>
        /// <param name="issearching">if set to <c>true</c> then a search is being conducted.</param>
        /// <param name="searchvalue">The search value.</param>
        public EloSearchingEvent(bool issearching, string searchvalue)
        {
            IsSearching = issearching;
            SearchValue = searchvalue;
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
        /// Gets or sets the search value.
        /// </summary>
        /// <value>
        /// The search value.
        /// </value>
        public string SearchValue
        {
            get;
            set;
        }
    }
}