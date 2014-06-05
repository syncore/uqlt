namespace UQLT.Models.Filters.User
{
    /// <summary>
    /// Model representing the difficulty settings in the user's filters
    /// </summary>
    public class Difficulty
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
        /// Gets or sets the difficulty.
        /// </summary>
        /// <value>
        /// The difficulty.
        /// </value>
        public string difficulty
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