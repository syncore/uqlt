namespace UQLT.Models.Configuration
{
    /// <summary>
    /// Model representing the user's various application configuration settings saved as json on
    /// user's hard disk.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets or sets the serverbrowser_options.
        /// </summary>
        /// <value>The serverbrowser_options.</value>
        public ServerBrowserOptions serverbrowser_options
        {
            get;
            set;
        }
    }
}