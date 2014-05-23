using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.User
{
    //-----------------------------------------------------------------------------------------------------
    /// <summary>
    /// Model used when building user filters to send to QL API. Sent in the object as an array.
    /// Used to get appropriate ig, GameTypes array, and ranked for a given gametype in filter
    /// </summary>
    public class GameInfo
    {
        public string type { get; set; }

        public object ig { get; set; }

        public List<int> gtarr { get; set; }

        public object ranked { get; set; }
    }
}