using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models
{
    public class QuakeLiveFilters
    {
        public QuakeLiveFilters() { }

        public String group { get; set; }
        public String game_type { get; set; }
        public String arena { get; set; }
        public String state { get; set; }
        public String difficulty { get; set; }
        public Object location { get; set; }
        public Object @private { get; set; }
        public int premium_only { get; set; }
        public Object ranked { get; set; }
        public int invitation_only { get; set; }
        
        
    }

    public class QuakeLiveFilterObject
    {
        public QuakeLiveFilterObject()
        {
        }

        public QuakeLiveFilters filters { get; set; }
        public String arena_type { get; set; }
        public List<String> players { get; set; }
        public List<int> game_types { get; set; }
        public Object ig { get; set; }
    }
    
    }
