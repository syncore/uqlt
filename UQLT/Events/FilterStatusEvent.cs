namespace UQLT.Events
{
    /// <summary>
    /// Event that is fired whenever we receive a new default filter, either through the "make new
    /// default" button or "reset filters" button in the filterviewmodel
    /// </summary>
    public class FilterStatusEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterStatusEvent" /> class.
        /// </summary>
        /// <param name="gametype">The gametype.</param>
        /// <param name="arena">The arena.</param>
        /// <param name="location">The location.</param>
        /// <param name="gamestate">The gamestate.</param>
        /// <param name="gamevisibility">The game visibility.</param>
        /// <param name="premium">The premium.</param>
        public FilterStatusEvent(string gametype, string arena, string location, string gamestate, string gamevisibility, string premium)
        {
            Gametype = gametype;
            Arena = arena;
            Location = location;
            Gamestate = gamestate;
            GameVisibility = gamevisibility;
            Premium = premium;
        }

        /// <summary>
        /// Gets or sets the arena.
        /// </summary>
        /// <value>The arena.</value>
        public string Arena
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the gamestate.
        /// </summary>
        /// <value>The gamestate.</value>
        public string Gamestate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the gametype.
        /// </summary>
        /// <value>The gametype.</value>
        public string Gametype
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the game visibility.
        /// </summary>
        /// <value>The game visibility.</value>
        public string GameVisibility
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        public string Location
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        public string Premium
        {
            get;
            set;
        }
    }
}