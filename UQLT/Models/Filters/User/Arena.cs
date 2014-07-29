using System.Collections.Generic;

namespace UQLT.Models.Filters.User
{
    /// <summary>
    /// Model representing the arena settings in the user's filters
    /// </summary>
    public class Arena
    {
        /// <summary>
        /// Gets or sets the arena.
        /// </summary>
        /// <value>The arena.</value>
        public string arena
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
        /// Gets or sets the display_name.
        /// </summary>
        /// <value>The display_name.</value>
        public string display_name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the max_players.
        /// </summary>
        /// <value>
        /// The max_players.
        /// </value>
        /// <remarks>This is only used for purposes of Start A Match.
        /// It has no effect on the server browser filters. </remarks>
        public int max_players
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the min_players.
        /// </summary>
        /// <value>
        /// The min_players.
        /// </value>
        /// <remarks>This is only used for purposes of Start A Match.
        /// It has no effect on the server browser filters. </remarks>
        public int min_players
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the tag_list.
        /// </summary>
        /// <value>
        /// The tag_list.
        /// </value>
        /// <remarks>This is only used for purposes of Start A Match.
        /// It has no effect on the server browser filters. </remarks>
        public List<string> tag_list
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the valid_gametypes.
        /// </summary>
        /// <value>
        /// The valid_gametypes.
        /// </value>
        /// <remarks>This is only used for purposes of Start A Match.
        /// It has no effect on the server browser filters. </remarks>
        public List<int> valid_gametypes
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return display_name;
        }
    }
}