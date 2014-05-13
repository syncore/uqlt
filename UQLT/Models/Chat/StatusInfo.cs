using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Chat
{
    // XMPP status information that specifies game server information for a player
    
    public class StatusInfo
    {
        public string address { get; set; }
        public int bot_game { get; set; }
        public string map { get; set; }
        public int public_id { get; set; }
        public string server_id { get; set; }
    }
}
