using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models
{
    public class Player
    {
        public string clan { get; set; }
        public int sub_level { get; set; }
        public string name { get; set; }
        public int bot { get; set; }
        public int rank { get; set; }
        public int score { get; set; }
        public int team { get; set; }
        public string model { get; set; }
        // custom property
        public int player_game_type { get; set; }
    }
}
