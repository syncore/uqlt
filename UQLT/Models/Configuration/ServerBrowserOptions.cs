namespace UQLT.Models.Configuration
{
    /// <summary>
    /// Model representing the user's server browser options saved as a json file on the user's hard disk.
    /// </summary>
    public class ServerBrowserOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ServerBrowserOptions" /> is auto_refresh.
        /// </summary>
        /// <value><c>true</c> if auto_refresh; otherwise, <c>false</c>.</value>
        public bool sb_auto_refresh
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the auto_refresh_index.
        /// </summary>
        /// <value>The auto_refresh_index.</value>
        public int sb_auto_refresh_index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the auto_refresh_seconds.
        /// </summary>
        /// <value>The auto_refresh_seconds.</value>
        public int sb_auto_refresh_seconds
        {
            get;
            set;
        }
    }
}