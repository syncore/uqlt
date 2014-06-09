using System.Collections.Generic;

namespace UQLT.Models.Filters.Remote
{
    /// <summary>
    /// Model used in actually building the filter object that will be converted to json and to be
    /// sent to the QL API The individual, specific details of the filters are handled by FilterBuilderDetails
    /// </summary>
    /// <remarks>
    /// Stuff should be sent to the Quake Live API in this order: arena_Type, players, game_types, ig
    /// So do not run CodeMaid on this file since it will automatically alphabetize it and mess up the order.    
    /// </remarks>
    public class FilterBuilderObject
    {
        // NOTE: Stuff should be sent to the Quake Live API in this order: arena_Type, players, game_types, ig
        // NOTE: So do not run CodeMaid on this file since it will automatically alphabetize it and mess up the order.    
        
        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        /// <value>The filters.</value>
        public FilterBuilderDetails filters
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the arena_type.
        /// </summary>
        /// <value>The arena_type.</value>
        public string arena_type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the players.
        /// </summary>
        /// <value>The players.</value>
        public List<string> players
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the game_types.
        /// </summary>
        /// <value>The game_types.</value>
        public List<int> game_types
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

        
    }
}