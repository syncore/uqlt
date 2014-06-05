namespace UQLT.Models.Chat
{
    /// <summary>
    /// Model for XMPP status information that is returned by QL that specifies game server information for a player on friend list
    /// </summary>
    public class StatusInfo
    {
        /// <summary>
        /// Gets or sets the server address.
        /// </summary>
        /// <value>
        /// The server's address.
        /// </value>
        public string address
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the given game is a bot_game.
        /// </summary>
        /// <value>
        /// The server's bot_game value.
        /// </value>
        public int bot_game
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the map of the server.
        /// </summary>
        /// <value>
        /// The server's map value.
        /// </value>
        public string map
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the public_id of the server.
        /// </summary>
        /// <value>
        /// The server's public_id.
        /// </value>
        public int public_id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the server's server_id.
        /// </summary>
        /// <value>
        /// The server's server_id.
        /// </value>
        public string server_id
        {
            get;
            set;
        }
    }
}