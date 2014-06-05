namespace UQLT.Models.Filters.User
{
    /// <summary>
    /// Model representing the user's saved filters stored as a json file on the user's hard disk
    /// </summary>
    public class SavedFilters
    {
        /// <summary>
        /// Gets or sets the type index
        /// </summary>
        /// <value>
        /// The type index
        /// </value>
        public int type_in
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the arena index
        /// </summary>
        /// <value>
        /// The arena index
        /// </value>
        public int arena_in
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location index
        /// </summary>
        /// <value>
        /// The location index
        /// </value>
        public int location_in
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the gamestate index
        /// </summary>
        /// <value>
        /// The gamestate index
        /// </value>
        public int state_in
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the visibility index
        /// </summary>
        /// <value>
        /// The visibility index
        /// </value>
        public int visibility_in
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the premium index
        /// </summary>
        /// <value>
        /// The premium index
        /// </value>
        public int premium_in
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the base64 encoded filter
        /// </summary>
        /// <value>
        /// The base64 encoded filter
        /// </value>
        public string fltr_enc
        {
            get;
            set;
        }
    }
}