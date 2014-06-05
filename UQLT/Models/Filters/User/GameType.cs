namespace UQLT.Models.Filters.User
{
    /// <summary>
    /// Model representing the game type settings in the user's filters
    /// </summary>
    public class GameType
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
        /// Gets or sets the game_type.
        /// </summary>
        /// <value>
        /// The game_type.
        /// </value>
        public string game_type
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