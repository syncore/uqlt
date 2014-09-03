using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UQLT.Core;
using UQLT.Interfaces;
using UQLT.Models.QLRanks;
using UQLT.ViewModels.ServerBrowser;

namespace UQLT.Helpers
{
    /// <summary>
    /// This class is responsible for retrieving player elo data from the QLRanks API and populating
    /// the corresponding collections.
    /// </summary>
    public class QlRanksDataRetriever : IQlRanksUpdater
    {
        public void CreateEloData<T>(IEnumerable<T> players)
        {
            foreach (var player in players)
            {
                UQltGlobals.PlayerEloInfo[player.ToString()] = new EloData
                {
                    DuelElo = 0,
                    CaElo = 0,
                    TdmElo = 0,
                    FfaElo = 0,
                    CtfElo = 0
                };
            }
        }

        /// <summary>
        /// Asynchronously retrieves the player Elo information from the QLRanks API via HTTP GET request(s).
        /// </summary>
        /// <param name="players">The players.</param>
        /// <returns>QLRanks object</returns>
        public async Task<QLRanks> GetEloDataFromQlRanksApiAsync(string players)
        {
            string url = "http://www.qlranks.com/api.aspx?nick=" + players;
            //string url = "http://10.0.0.7/api.aspx?nick=" + players;

            try
            {
                var query = new RestApiQuery();
                var qlr = await (query.QueryRestApiAsync<QLRanks>(url));

                return qlr;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Asynchronously prepares the player information, given a list of QL servers, to be sent
        /// to the QLRanks API in the <see cref="GetEloDataFromQlRanksApiAsync" /> method.
        /// </summary>
        /// <param name="servers">The list of servers wrapped by ServerDetailsViewModels.</param>
        /// <param name="maxPlayers">The maximum number of players to send to the limited QLRanks API per API call.</param>
        /// <returns>
        /// Nothing
        /// </returns>
        public async Task GetQlRanksPlayersAsync(IList<ServerDetailsViewModel> servers, int maxPlayers = 150)
        {
            var playerstoupdate = new List<string>();
            var qlrapicalls = new List<List<string>>();

            // Extract players, add to a list to update, split the list, then update.
            foreach (var server in servers)
            {
                foreach (var player in server.Players)
                {
                    // Elo "caching"
                    EloData val;
                    if (UQltGlobals.PlayerEloInfo.TryGetValue(player.name.ToLower(), out val))
                    {
                        // If the player has our pre-defined default elo value of 0 (qlranks elo
                        // will never be 0) then add player to a list of players to be updated.
                        if (val.DuelElo == 0)
                        {
                            playerstoupdate.Add(player.name.ToLower());
                            Debug.WriteLine("Player: " + player.name.ToLower() + " was not previously indexed. Adding to list of players whose elo we need...");
                        }
                    }
                    else
                    {
                        playerstoupdate.Add(player.name.ToLower());
                        Debug.WriteLine("Player: " + player.name.ToLower() + " was not previously indexed. Adding to list of players whose elo we need...");
                    }
                }
            }

            // Split servers.
            for (int i = 0; i < playerstoupdate.Count; i += maxPlayers)
            {
                qlrapicalls.Add(playerstoupdate.GetRange(i, Math.Min(maxPlayers, playerstoupdate.Count - i)));
                Debug.WriteLine("QLRANKS: API Call Index: " + i);
            }

            // Perform the tasks.
            var qlranksTasks = new List<Task<QLRanks>>();

            for (int i = 0; i < qlrapicalls.Count; i++)
            {
                qlranksTasks.Add(GetEloDataFromQlRanksApiAsync(string.Join("+", qlrapicalls[i])));
                Debug.WriteLine("QLRANKS: API Task " + i + " URL: http://www.qlranks.com/api.aspx?nick=" + string.Join("+", qlrapicalls[i]));
            }

            // All the combined n API calls must finish.
            await Task.WhenAll(qlranksTasks.ToArray());

            // Set the player elos.
            try
            {
                foreach (var qp in qlranksTasks.SelectMany(qlrt => qlrt.Result.players))
                {
                    UQltGlobals.PlayerEloInfo[qp.nick].DuelElo = qp.duel.elo;
                    UQltGlobals.PlayerEloInfo[qp.nick].CaElo = qp.ca.elo;
                    UQltGlobals.PlayerEloInfo[qp.nick].TdmElo = qp.tdm.elo;
                    UQltGlobals.PlayerEloInfo[qp.nick].FfaElo = qp.ffa.elo;
                    UQltGlobals.PlayerEloInfo[qp.nick].CtfElo = qp.ctf.elo;
                }
                // Player elos have been set in dictionary, now set on the Player object itself.
                foreach (var s in servers)
                {
                    foreach (var player in s.Players)
                    {
                        s.Server.SetPlayerElos();
                    }

                    // Now set the elos on the PlayerDetailsViewModel so that the UI can automatically receive updates as they come in.
                    // Also, want this to be synchronous; suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
                    var set = s.SetPlayerElosView();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to load QLRanks player data. Refresh and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// Sets the ql ranks players' elo values.
        /// </summary>
        /// <param name="qlRanks">The QLRanks object</param>
        /// <returns></returns>
        /// <remarks>This acts independently on a QLRanks object, whereas <see cref="GetQlRanksPlayersAsync"/> contains code that sets the data from a QL server object.</remarks>
        public void SetQlRanksPlayers(QLRanks qlRanks)
        {
            try
            {
                foreach (var player in qlRanks.players)
                {
                    UQltGlobals.PlayerEloInfo[player.nick].DuelElo = player.duel.elo;
                    UQltGlobals.PlayerEloInfo[player.nick].CaElo = player.ca.elo;
                    UQltGlobals.PlayerEloInfo[player.nick].TdmElo = player.tdm.elo;
                    UQltGlobals.PlayerEloInfo[player.nick].FfaElo = player.ffa.elo;
                    UQltGlobals.PlayerEloInfo[player.nick].CtfElo = player.ctf.elo;
                }
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}