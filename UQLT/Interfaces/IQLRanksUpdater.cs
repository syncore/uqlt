using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQLT.Models.QuakeLiveAPI;

namespace UQLT.Interfaces
{
    interface IQLRanksUpdater
    {
        Task GetQLRanksPlayers(IList<Server> servers, int maxPlayers = 150);
        void SetQLranksDefaultElo(string player);
    
    }
}
