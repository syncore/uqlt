using System.Collections.Generic;

namespace UQLT.Models.Filters.Remote
{
    /// <summary>
    /// Model used in actually building the filter object that will be converted to json and to be sent to the QL API
    /// The individual, specific details of the filters are handled by FilterBuilderDetails
    /// </summary>
    public class FilterBuilderObject
    {
        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        /// <value>
        /// The filters.
        /// </value>
        public FilterBuilderDetails filters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the arena_type.
        /// </summary>
        /// <value>
        /// The arena_type.
        /// </value>
        public string arena_type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the players.
        /// </summary>
        /// <value>
        /// The players.
        /// </value>
        public List<string> players
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the game_types.
        /// </summary>
        /// <value>
        /// The game_types.
        /// </value>
        public List<int> game_types
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ig.
        /// </summary>
        /// <value>
        /// The ig.
        /// </value>
        public object ig
        {
            get;
            set;
        }
    }
}