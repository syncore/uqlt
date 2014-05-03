using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.User
{
    public class ImportedFilters
    {
        public List<Location> locations { get; set; }
        
        public List<BasicGametypes> basic_gametypes { get; set; }

        public List<Arena> arenas { get; set; }

        public List<Difficulty> difficulty { get; set; }

        public List<GameState> gamestate { get; set; }

        public List<GameType> game_types { get; set; }

        public List<GameInfo> game_info { get; set; }
    }
}
