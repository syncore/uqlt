using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.QLRanks
{
    public class QLRanksPlayer
    {
        public string nick { get; set; }

        public Ca ca { get; set; }

        public Duel duel { get; set; }

        public Tdm tdm { get; set; }

        public Ctf ctf { get; set; }

        public Ffa ffa { get; set; }

    }
}
