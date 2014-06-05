namespace UQLT.Models.Filters.User
{
    /// <summary>
    /// Model representing the location settings in the user's filters
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Location" /> is active.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        public bool active
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>The city.</value>
        public string city
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
        /// Gets or sets the location_id.
        /// </summary>
        /// <value>The location_id.</value>
        public object location_id
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