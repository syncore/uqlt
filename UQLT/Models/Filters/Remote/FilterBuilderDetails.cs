namespace UQLT.Models.Filters.Remote
{
    /// <summary>
    /// Model used in specifying the details of a filter when creating object to be converted to
    /// json and to be sent to the QL API
    /// </summary>
    /// <remarks>
    /// In this filters object, stuff should be sent to the QL API in this order: group, game_type, arena, state, difficulty, location, private, premium_only, invitation_only
    /// So do not run CodeMaid on this file since it will automatically alphabetize it and mess up the order.    
    /// </remarks>
    public class FilterBuilderDetails
    {

        // NOTE: In this filters object, stuff should be sent to the QL API in this order: group, game_type, arena, state, difficulty, location, private, premium_only, invitation_only
        // NOTE: So do not run CodeMaid on this file since it will automatically alphabetize it and mess up the order.   


        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>The group.</value>
        public string group
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the game_type.
        /// </summary>
        /// <value>The game_type.</value>
        public string game_type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the arena.
        /// </summary>
        /// <value>The arena.</value>
        public string arena
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public string state
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the difficulty.
        /// </summary>
        /// <value>The difficulty.</value>
        public string difficulty
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        public object location
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the private.
        /// </summary>
        /// <value>The private.</value>
        /// <remarks>QL API quirk: This can be a string or int</remarks>
        public object @private
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the premium_only.
        /// </summary>
        /// <value>The premium_only.</value>
        public int premium_only
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the invitation_only.
        /// </summary>
        /// <value>The invitation_only.</value>
        public int invitation_only
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ranked.
        /// </summary>
        /// <value>The ranked.</value>
        public object ranked
        {
            get;
            set;
        }

    }
}