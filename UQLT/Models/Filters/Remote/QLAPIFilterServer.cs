namespace UQLT.Models.Filters.Remote
{
    /// <summary>
    /// Model representing the format of an individual server returned from http://www.quakelive.com/browser/list?filter={base64_encoded_json_filter} - it's different from that returned by /browser/details?ids={server_id(s)}
    /// Note: /list?filter={base64_encoded_json_filter} does not include a list of players for a given server at all, unlike /browser/details?ids={server_id}
    /// </summary>
    public class QLAPIFilterServer
    {
        /// <summary>
        /// Gets or sets the num_players.
        /// </summary>
        /// <value>
        /// The num_players.
        /// </value>
        public int num_players
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the map.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public string map
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the public_id.
        /// </summary>
        /// <value>
        /// The public_id.
        /// </value>
        public int public_id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ruleset.
        /// </summary>
        /// <value>
        /// The ruleset.
        /// </value>
        public string ruleset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location_id.
        /// </summary>
        /// <value>
        /// The location_id.
        /// </value>
        public int location_id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the game_type.
        /// </summary>
        /// <value>
        /// The game_type.
        /// </value>
        public int game_type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the g_needpass.
        /// </summary>
        /// <value>
        /// The g_needpass.
        /// </value>
        public int g_needpass
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the teamsize.
        /// </summary>
        /// <value>
        /// The teamsize.
        /// </value>
        public int teamsize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        public string owner
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ranked.
        /// </summary>
        /// <value>
        /// The ranked.
        /// </value>
        public int ranked
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the host_name.
        /// </summary>
        /// <value>
        /// The host_name.
        /// </value>
        public string host_name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the skill delta.
        /// </summary>
        /// <value>
        /// The skill delta.
        /// </value>
        public int skillDelta
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the g_custom settings.
        /// </summary>
        /// <value>
        /// The g_custom settings.
        /// </value>
        public string g_customSettings
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>
        /// The premium.
        /// </value>
        public object premium
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the host_address.
        /// </summary>
        /// <value>
        /// The host_address.
        /// </value>
        public string host_address
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the max_clients.
        /// </summary>
        /// <value>
        /// The max_clients.
        /// </value>
        public int max_clients
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the num_clients.
        /// </summary>
        /// <value>
        /// The num_clients.
        /// </value>
        public int num_clients
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the g_instagib.
        /// </summary>
        /// <value>
        /// The g_instagib.
        /// </value>
        public int g_instagib
        {
            get;
            set;
        }
    }
}