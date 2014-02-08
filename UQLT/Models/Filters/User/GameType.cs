using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.User
{
    public class GameType
    {
        public string display_name { get; set; }

        public string game_type { get; set; }

        public override string ToString()
        {
            return display_name;
        }
    }
}
