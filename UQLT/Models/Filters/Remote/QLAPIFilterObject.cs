using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.Remote
{
    public class QLAPIFilterObject
    {
        public int lfg_requests { get; set; }

        public int matches_played { get; set; }

        public List<QLAPIFilterServer> servers { get; set; }
    }
}
