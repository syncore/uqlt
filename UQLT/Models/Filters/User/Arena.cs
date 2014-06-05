namespace UQLT.Models.Filters.User
{
    /// <summary>
    /// Model representing the arena settings in the user's filters
    /// </summary>
    public class Arena
    {
        /// <summary>
        /// Gets or sets the display_name.
        /// </summary>
        /// <value>
        /// The display_name.
        /// </value>
        public string display_name
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
        /// Gets or sets the arena.
        /// </summary>
        /// <value>
        /// The arena.
        /// </value>
        public string arena
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return display_name;
        }
    }
}