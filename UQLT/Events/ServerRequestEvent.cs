namespace UQLT.Events
{
    /// <summary>
    /// Event that is fired whenever we receive a new default filter, either through the "make new
    /// default" button or "reset filters" button in the filterviewmodel
    /// </summary>
    public class ServerRequestEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerRequestEvent" /> class.
        /// </summary>
        /// <param name="filterUrl">The filter URL.</param>
        public ServerRequestEvent(string filterUrl)
        {
            ServerRequestUrl = filterUrl;
        }

        /// <summary>
        /// Gets or sets the server request URL.
        /// </summary>
        /// <value>The server request URL.</value>
        public string ServerRequestUrl
        {
            get;
            set;
        }
    }
}