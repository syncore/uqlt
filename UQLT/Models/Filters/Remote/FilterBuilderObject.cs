using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.Remote
{
    public class FilterBuilderObject
    {
        public FilterBuilderDetails filters { get; set; }

        public string arena_type { get; set; }

        public List<string> players { get; set; }

        public List<int> game_types { get; set; }

        public object ig { get; set; }
    }
}
