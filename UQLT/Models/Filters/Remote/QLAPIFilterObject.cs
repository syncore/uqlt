using System.Collections.Generic;

namespace UQLT.Models.Filters.Remote
{
    /// <summary>
    /// Model representing the format of the object (including array) of servers (list of individual QLAPIFilterServer) returned from http://www.quakelive.com/browser/list?filter={base64_encoded_json_filter}
    /// </summary>
    public class QLAPIFilterObject
    {
        /// <summary>
        /// Gets or sets the lfg_requests.
        /// </summary>
        /// <value>
        /// The lfg_requests.
        /// </value>
        public int lfg_requests
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the matches_played.
        /// </summary>
        /// <value>
        /// The matches_played.
        /// </value>
        public int matches_played
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the servers.
        /// </summary>
        /// <value>
        /// The servers.
        /// </value>
        public List<QLAPIFilterServer> servers
        {
            get;
            set;
        }
    }
}