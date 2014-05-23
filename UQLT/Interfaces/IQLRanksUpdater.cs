using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQLT.Models.QuakeLiveAPI;

namespace UQLT.Interfaces
{
    //-----------------------------------------------------------------------------------------------------
    /// <summary>
    /// Necessary methods for performing QLRanks updates. Will likely be modified later.
    /// </summary>
    interface IQLRanksUpdater
    {
        Task GetQLRanksPlayersAsync(IList<Server> servers, int maxPlayers = 150);
        void SetQLranksDefaultElo(string player);

    }
}