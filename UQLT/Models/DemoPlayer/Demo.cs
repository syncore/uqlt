using System.Collections.Generic;

namespace UQLT.Models.DemoPlayer
{
    /// <summary>
    /// Model for demo files.
    /// </summary>
    public class Demo
    {
        public Srvinfo srvinfo { get; set; }
        public string recorded_by { get; set; }
        public string protocol { get; set; }
        public string timestamp { get; set; }
        public string gametype_title { get; set; }
        public int gametype { get; set; }
        public List<Player> spectators { get; set; }
        public double size { get; set; }
        public string filename { get; set; }
        public List<Player> players { get; set; }
        public string map_name { get; set; }
    }
}
