namespace UQLT.Events
{
    /// <summary>
    /// Event that is fired whenever we receive indication that the number of players
    /// found in a elo search changes.
    /// </summary>
    public class EloFoundCountEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EloFoundCountEvent" /> class.
        /// </summary>
        /// <param name="searchresultcount">The number of matches found.</param>

        public EloFoundCountEvent(int searchresultcount)
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