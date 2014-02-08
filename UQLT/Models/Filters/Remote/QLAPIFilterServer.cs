using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.Remote
{
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
