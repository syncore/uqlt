using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.User
{
    public class ActiveLocation
    {
        public string display_name { get; set; }
        public object location_id { get; set; }
        public string city { get; set; }

        public override string ToString()
        {
            return display_name;
        }
    }
}
