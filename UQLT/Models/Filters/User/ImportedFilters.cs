using System.Collections.Generic;

namespace UQLT.Models.Filters.User
{
    /// <summary>
    /// Model representing the make up of the json file that is stored on the disk and contains the
    /// current updated filter information.
    /// </summary>
    public class ImportedFilters
    {
        /// <summary>
        /// Gets or sets the arenas.
        /// </summary>
        /// <value>The arenas.</value>
        public List<Arena> arenas
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the basic_gametypes.
        /// </summary>
        /// <value>The basic_gametypes.</value>
        public List<BasicGametypes> basic_gametypes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the difficulty.
        /// </summary>
        /// <value>The difficulty.</value>
        public List<Difficulty> difficulty
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the game_info.
        /// </summary>
        /// <value>The game_info.</value>
        public List<GameInfo> game_info
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the game_types.
        /// </summary>
        /// <value>The game_types.</value>
        public List<GameType> game_types
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the gamestate.
        /// </summary>
        /// <value>The gamestate.</value>
        public List<GameState> gamestate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the locations.
        /// </summary>
        /// <value>The locations.</value>
        public List<Location> locations
        {
            get;
            set;
        }
    }
}