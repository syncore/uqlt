using System.Collections.Generic;

namespace UQLT.Models.QLRanks
{
    /// <summary>
    /// Model representing the outer container object returned from the QLRanks API
    /// </summary>
    public class QLRanks
    {
        /// <summary>
        /// Gets or sets the players.
        /// </summary>
        /// <value>The players.</value>
        public List<QLRanksPlayer> players
        {
            get;
            set;
        }
    }
}