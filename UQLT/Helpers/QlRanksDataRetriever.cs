using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using UQLT.Interfaces;
using UQLT.Models.QLRanks;
using UQLT.Models.QuakeLiveAPI;

namespace UQLT.Helpers
{
    /// <summary>
    /// This class is responsible for retrieving player elo data from the QLRanks API and populating
    /// the corresponding collections.
    /// </summary>
    public class QlRanksDataRetriever : IQlRanksUpdater
    {
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
            catch (JsonException jEx)
            {
                Debug.WriteLine(jEx.Message);
                return null;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Asynchronously prepares the player information, given a list of QL servers, to be sent
        /// to the QLRanks API in the <see cref="GetEloDataFromQlRanksApiAsync" /> method.
        /// </summary>
        /// <param name="servers">The list of servers.</param>
        /// <param name="maxPlayers">
        /// The maximum number of players to send to the limited QLRanks API per API call.
        /// </param>
        /// <returns></returns>
        public async Task GetQlRanksPlayersAsync(IList<Server> servers, int maxPlayers = 150)
        {
            var playerstoupdate = new List<string>();
            var qlrapicalls = new List<List<string>>();

            // Extract players, add to a list to update, split the list, then update.
            foreach (var server in servers)
            {
                foreach (var player in server.players)
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
                foreach (var qlrt in qlranksTasks)
                {
                    foreach (var qp in qlrt.Result.players)
                    {
                        UQltGlobals.PlayerEloInfo[qp.nick.ToLower()].DuelElo = qp.duel.elo;
                        UQltGlobals.PlayerEloInfo[qp.nick.ToLower()].CaElo = qp.ca.elo;
                        UQltGlobals.PlayerEloInfo[qp.nick.ToLower()].TdmElo = qp.tdm.elo;
                        UQltGlobals.PlayerEloInfo[qp.nick.ToLower()].FfaElo = qp.ffa.elo;
                        UQltGlobals.PlayerEloInfo[qp.nick.ToLower()].CtfElo = qp.ctf.elo;
                    }
                }
                // Player elos have been set in dictionary, now set on the Player object itself
                // TODO: this will allow using Properties and NotifyPropertyChange to update the
                //       player view in the server browser.
                foreach (var s in servers)
                {
                    foreach (var player in s.players)
                    {
                        s.setPlayerElos();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to load QLRanks player data. Refresh and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine(e);
            }
        }

        public void CreateEloData<T>(IEnumerable<T> players)
        {
            foreach (var player in players)
            {
                UQltGlobals.PlayerEloInfo[player.ToString().ToLower()] = new EloData
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
        /// Sets the ql ranks players asynchronous.
        /// </summary>
        /// <param name="qlRanks">The QLRanks object</param>
        /// <returns></returns>
        /// <remarks>This acts independently on a QLRanks object, whereas <see cref="GetQlRanksPlayersAsync"/> contains code that sets the data from a QL server object.</remarks>
        public void SetQlRanksPlayersAsync(QLRanks qlRanks)
        {
            foreach (var player in qlRanks.players)
            {
                UQltGlobals.PlayerEloInfo[player.nick.ToLower()].DuelElo = player.duel.elo;
                UQltGlobals.PlayerEloInfo[player.nick.ToLower()].CaElo = player.ca.elo;
                UQltGlobals.PlayerEloInfo[player.nick.ToLower()].TdmElo = player.tdm.elo;
                UQltGlobals.PlayerEloInfo[player.nick.ToLower()].FfaElo = player.ffa.elo;
                UQltGlobals.PlayerEloInfo[player.nick.ToLower()].CtfElo = player.ctf.elo; 
            }
            
        }
    }
}