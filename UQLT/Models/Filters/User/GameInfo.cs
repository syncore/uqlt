using System.Collections.Generic;

namespace UQLT.Models.Filters.User
{
    /// <summary>
    /// Model used when building user filters to send to QL API. Sent in the object as an array.
    /// Used to get appropriate ig, GameTypes array, and ranked for a given gametype in filter
    /// </summary>
    public class GameInfo
    {
        /// <summary>
        /// Gets or sets the gtarr.
        /// </summary>
        /// <value>The gtarr.</value>
        public List<int> gtarr
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ig.
        /// </summary>
        /// <value>The ig.</value>
        public object ig
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ranked.
        /// </summary>
        /// <value>The ranked.</value>
        public object ranked
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public string type
        {
            get;
            set;
        }
    }
}