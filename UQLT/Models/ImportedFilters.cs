using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models
{
    public class ImportedFilters
    {
        public List<ActiveLocation> active_locations { get; set; }
        public List<InactiveLocation> inactive_locations { get; set; }
        public List<ServerBrowserLocations> serverbrowser_locations { get; set; }
        public List<Arena> arenas { get; set; }

        public List<Difficulty> difficulty { get; set; }
        public List<GameState> gamestate { get; set; }
        public List<GameType> game_types { get; set; }
        public List<GameInfo> game_info { get; set; }
    }

    public class ActiveLocation
    {
        public string display_name { get; set; }
        public object location_id { get; set; }
    }

    public class ServerBrowserLocations
    {
        public string display_name { get; set; }
        public object location_id { get; set; }
    }

    public class InactiveLocation
    {
        public string display_name { get; set; }
        public int location_id { get; set; }
    }

    public class Arena
    {
        public string display_name { get; set; }
        public string arena_type { get; set; }
        public string arena { get; set; }
    }

    public class Difficulty
    {
        public string display_name { get; set; }
        public string difficulty { get; set; }
    }

    public class GameState
    {
        public string display_name { get; set; }
        public string state { get; set; }
    }

    public class GameType
    {
        public string display_name { get; set; }
        public string game_type { get; set; }
    }
    public class GameInfo
    {
        public string type { get; set; }
        public object ig { get; set; }
        public List<int> gtarr { get; set; }
        public object ranked { get; set; }
    }
}
