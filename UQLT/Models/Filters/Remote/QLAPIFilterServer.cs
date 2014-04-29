using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.Remote
{
    // this is the format of an individual server returned from http://www.quakelive.com/browser/list?filter={base64_encoded_json_filter} - it's different from that returned by /browser/details?ids={server_id(s)}
    // note: /list?filter={base64_encoded_json_filter} does not a list of players for a given server at all, unlike /browser/details?ids={server_id}
    
    public class QLAPIFilterServer
    {
        public int num_players { get; set; }

        public string map { get; set; }

        public int public_id { get; set; }

        public string ruleset { get; set; }

        public int location_id { get; set; }

        public int game_type { get; set; }

        public int g_needpass { get; set; }

        public int teamsize { get; set; }

        public string owner { get; set; }

        public int ranked { get; set; }

        public string host_name { get; set; }

        public int skillDelta { get; set; }

        public string g_customSettings { get; set; }

        public object premium { get; set; }

        public string host_address { get; set; }

        public int max_clients { get; set; }

        public int num_clients { get; set; }

        public int g_instagib { get; set; }
    }
}
