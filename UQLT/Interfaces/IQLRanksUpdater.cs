using System.Collections.Generic;
using System.Threading.Tasks;
using UQLT.Models.QLRanks;
using UQLT.ViewModels.ServerBrowser;

namespace UQLT.Interfaces
{
    /// <summary>
    /// Necessary methods for performing QLRanks updates. Will likely be modified later.
    /// </summary>
    internal interface IQlRanksUpdater
    {
        /// <summary>
        /// Asynchronously retrieves the QLRanks players.
        /// </summary>
        /// <param name="servers">The servers.</param>
        /// <param name="maxPlayers">
        /// The maximum number of players sent to QLRanks API per API call.
        /// </param>
        /// <returns>Nothing</returns>
        Task GetQlRanksPlayersAsync(IList<ServerDetailsViewModel> servers, int maxPlayers = 150);

        void SetQlRanksPlayers(QLRanks qlRanks);

        //void SetQLranksDefaultElo(string player);
    }
}