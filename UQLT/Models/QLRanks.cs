using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models
{
    public class QLRanks
    {
        public List<QLRanksPlayer> players { get; set; }
    }
    public class QLRanksPlayer
    {
        public string nick { get; set; }
        public Ca ca { get; set; }
        public Duel duel { get; set; }
        public Tdm tdm { get; set; }
        public Ctf ctf { get; set; }
        public Ffa ffa { get; set; }
    }
    public class Ca
    {
        public int rank { get; set; }
        public int elo { get; set; }
    }

    public class Duel
    {
        public int rank { get; set; }
        public int elo { get; set; }
    }

    public class Tdm
    {
        public int rank { get; set; }
        public int elo { get; set; }
    }

    public class Ctf
    {
        public int rank { get; set; }
        public int elo { get; set; }
    }

    public class Ffa
    {
        public int rank { get; set; }
        public int elo { get; set; }
    }
}
