using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.User
{
    public class SavedFilters
    {
        public int type_in { get; set; }
        public int arena_in { get; set; }
        public int location_in { get; set; }
        public int state_in { get; set; }
        public int visibility_in { get; set; }
        public int premium_in { get; set; }
        public string fltr_enc { get; set; }
    
    }
}
