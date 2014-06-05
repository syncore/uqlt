namespace UQLT.Events
{
    /// <summary>
    /// Event that is fired whenever we receive a new player count from the Server Browser
    /// </summary>
    public class PlayerCountEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerCountEvent"/> class.
        /// </summary>
        /// <param name="playercount">The player count.</param>
        public PlayerCountEvent(int playercount)
        {
            PlayerCount = playercount;
        }

        /// <summary>
        /// Gets or sets the player count.
        /// </summary>
        /// <value>
        /// The player count.
        /// </value>
        public int PlayerCount
        {
            get;
            set;
        }
    }
}