namespace UQLT.Events
{
    /// <summary>
    /// Event that is fired whenever we receive a new server count from the Server Browser
    /// </summary>
    public class ServerCountEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCountEvent"/> class.
        /// </summary>
        /// <param name="servercount">The server count.</param>
        public ServerCountEvent(int servercount)
        {
            ServerCount = servercount;
        }

        /// <summary>
        /// Gets or sets the server count.
        /// </summary>
        /// <value>
        /// The server count.
        /// </value>
        public int ServerCount
        {
            get;
            set;
        }
    }
}