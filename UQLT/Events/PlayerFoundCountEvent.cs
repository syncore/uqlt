namespace UQLT.Events
{
    /// <summary>
    /// Event that is fired whenever we receive indication that the number of players
    /// found in a player search changes.
    /// </summary>
    public class PlayerFoundCountEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerFoundCountEvent" /> class.
        /// </summary>
        /// <param name="searchresultcount">The number of matches found.</param>
        
        public PlayerFoundCountEvent(int searchresultcount)
        {
            SearchResultCount = searchresultcount;
        }

        /// <summary>
        /// Gets or sets the search result count.
        /// </summary>
        /// <value>
        /// The search result count.
        /// </value>
        public int SearchResultCount
        {
            get;
            set;
        }
    }
}