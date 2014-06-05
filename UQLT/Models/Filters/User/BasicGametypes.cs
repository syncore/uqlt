namespace UQLT.Models.Filters.User
{
    /// <summary>
    /// Model representing the gametypes of type int that are actually encountered in the server browser, NOT the filter menu.
    /// This class is different from GameType.cs, as that class contains gametypes of both types int and string which are used to build filters.
    /// </summary>
    public class BasicGametypes
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
        /// Gets or sets the short_name.
        /// </summary>
        /// <value>
        /// The short_name.
        /// </value>
        public string short_name
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
        public int game_type
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