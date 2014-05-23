using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.QuakeLiveAPI
{
    /// <summary>
    /// This represents a player on a QL server contained within the Server class.
    /// </summary>
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
        
        // Custom properties
        public int player_game_type { get; set; }
        
        // QLRanks
        public long tdmelo { get; set; }
        public long caelo { get; set; }
        public long ffaelo { get; set; }
        public long ctfelo { get; set; }
        public long duelelo { get; set; }

    }
}
