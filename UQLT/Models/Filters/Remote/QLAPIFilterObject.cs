using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.Remote
{
    //-----------------------------------------------------------------------------------------------------
    /// <summary>
    /// Model representing the format of the object (including array) of servers (list of individual QLAPIFilterServer) returned from http://www.quakelive.com/browser/list?filter={base64_encoded_json_filter}
    /// </summary>
    public class QLAPIFilterObject
    {
        public int lfg_requests { get; set; }

        public int matches_played { get; set; }

        public List<QLAPIFilterServer> servers { get; set; }
    }
}