namespace UQLT.Models.QuakeLiveAPI
{
    /// <summary>
    /// Model representing a player on a QL server contained within the Server class.
    /// </summary>
    public class Player
    {
        private string _name;
        
        /// <summary>
        /// Gets or sets the bot.
        /// </summary>
        /// <value>The bot.</value>
        public int bot
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ca elo.
        /// </summary>
        /// <value>The ca elo.</value>
        /// <remarks>This is a custom property.</remarks>
        public long caelo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the clan.
        /// </summary>
        /// <value>The clan.</value>
        public string clan
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ctf elo.
        /// </summary>
        /// <value>The ctf elo.</value>
        /// <remarks>This is a custom property.</remarks>
        public long ctfelo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the duel elo.
        /// </summary>
        /// <value>The duel elo.</value>
        /// <remarks>This is a custom property.</remarks>
        public long duelelo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ffa elo.
        /// </summary>
        /// <value>The ffa elo.</value>
        /// <remarks>This is a custom property.</remarks>
        public long ffaelo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public string model
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value.ToLowerInvariant();
            }
        }

        // <summary>
        // Gets or sets the player_game_type.
        // </summary>
        // <value>The player_game_type.</value>
        // <remarks>This is a custom property.</remarks>
        public int player_game_type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the rank.
        /// </summary>
        /// <value>The rank.</value>
        public int rank
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        /// <value>The score.</value>
        public int score
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the sub_level.
        /// </summary>
        /// <value>The sub_level.</value>
        public int sub_level
        {
            get;
            set;
        }

        public long tdmelo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the team.
        /// </summary>
        /// <value>The team.</value>
        public int team
        {
            get;
            set;
        }

        // <summary>
        // Gets or sets the tdm elo.
        // </summary>
        // <value>The tdm elo.</value>
        // <remarks>This is a custom property.</remarks>
    }
}