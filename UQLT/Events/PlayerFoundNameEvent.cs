namespace UQLT.Events
{
    /// <summary>
    /// Event that is fired whenever we receive indication that a new match has been found.
    /// </summary>
    public class PlayerFoundNameEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerFoundNameEvent" /> class.
        /// </summary>
        /// <param name="players">The players.</param>

        public PlayerFoundNameEvent(string players)
        {
            Players = players;
        }

        /// <summary>
        /// Gets or sets the players.
        /// </summary>
        /// <value>
        /// The players.
        /// </value>
        public string Players
        {
            get;
            set;
        }
    }
}