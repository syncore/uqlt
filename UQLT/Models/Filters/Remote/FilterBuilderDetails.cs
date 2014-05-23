using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.Remote
{
    //-----------------------------------------------------------------------------------------------------
    /// <summary>
    /// Model used in specifying the details of a filter when creating object to be converted to json and to be sent to the QL API
    /// </summary>
    public class FilterBuilderDetails
    {
        public string group { get; set; }

        public string game_type { get; set; }

        public string arena { get; set; }

        public string state { get; set; }

        public string difficulty { get; set; }

        public object location { get; set; }

        public object @private { get; set; }

        public int premium_only { get; set; }

        public object ranked { get; set; }

        public int invitation_only { get; set; }

    }
}