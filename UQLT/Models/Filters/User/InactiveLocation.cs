using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.User
{
    public class InactiveLocation
    {
        public string display_name { get; set; }
        public int location_id { get; set; }

        public override string ToString()
        {
            return display_name;
        }
    }
}
