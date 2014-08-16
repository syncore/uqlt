using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using UQLT.Core;
using UQLT.Helpers;
using UQLT.Models.QLRanks;
using UQLT.Models.QuakeLiveAPI;

namespace UQLT.ViewModels
{
    /// <summary>
    /// This class specifically handles viewmodel-wrapped server information for display in the buddy list's game information tooltip.
    /// </summary>
    [Export(typeof(ChatGameInfoViewModel))]
    public class ChatGameInfoViewModel : ServerDetailsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatGameInfoViewModel"/> class.
        /// </summary>
        /// <param name="server">The server wrapped by this viewmodel.</param>
        [ImportingConstructor]
        public ChatGameInfoViewModel(Server server)
            : base(server)
        {
            ToolTipPlayers = SortServerPlayersForBuddyList(server.players);
            NotifyTaskCompletion.Create(CheckToolTipPlayerElosAsync());
        }

        /// <summary>
        /// Gets or sets the formatted player list for use with the tooltip.
        /// </summary>
        /// <value>The formatted player list.</value>
        public List<PlayerDetailsViewModel> ToolTipPlayers { get; set; }

        /// <summary>
        /// Retrieves elo information for use in the buddy list game information tooltip.
        /// </summary>
        /// <returns>Task</returns>
        public async Task CheckToolTipPlayerElosAsync()
        {
            if (NumPlayers == 0) { return; }
            if (!IsQlRanksSupportedAllGames) { return; }

            var qlRanksPlayersToUpdate = new HashSet<string>();

            foreach (var p in ToolTipPlayers)
            {
                EloData val;
                switch (GameType)
                {
                    // FFA
                    case 0:
                        if (UQltGlobals.PlayerEloInfo.TryGetValue(p.Name, out val))
                        {
                            p.PlayerElo = UQltGlobals.PlayerEloInfo[p.Name].FfaElo;
                        }
                        else
                        {
                            Debug.WriteLine(
                                "Key doesn't exist - no FFA elo data found for player: " + p.Name);
                            qlRanksPlayersToUpdate.Add(p.Name);
                        }
                        break;
                    // DUEL
                    case 1:
                        if (UQltGlobals.PlayerEloInfo.TryGetValue(p.Name, out val))
                        {
                            p.PlayerElo = UQltGlobals.PlayerEloInfo[p.Name].DuelElo;
                        }
                        else
                        {
                            Debug.WriteLine(
                                "Key doesn't exist - no DUEL elo data found for player: " + p.Name);
                            qlRanksPlayersToUpdate.Add(p.Name);
                        }
                        break;
                    // TDM
                    case 3:
                        if (UQltGlobals.PlayerEloInfo.TryGetValue(p.Name, out val))
                        {
                            p.PlayerElo = UQltGlobals.PlayerEloInfo[p.Name].TdmElo;
                        }
                        else
                        {
                            Debug.WriteLine(
                                "Key doesn't exist - no TDM elo data found for player: " + p.Name);
                            qlRanksPlayersToUpdate.Add(p.Name);
                        }
                        break;
                    // CA
                    case 4:
                        if (UQltGlobals.PlayerEloInfo.TryGetValue(p.Name, out val))
                        {
                            p.PlayerElo = UQltGlobals.PlayerEloInfo[p.Name].CaElo;
                        }
                        else
                        {
                            qlRanksPlayersToUpdate.Add(p.Name);
                            Debug.WriteLine(
                                "Key doesn't exist - no CA elo data found for player: " + p.Name);
                        }
                        break;
                    // CTF
                    case 5:
                        if (UQltGlobals.PlayerEloInfo.TryGetValue(p.Name, out val))
                        {
                            p.PlayerElo = UQltGlobals.PlayerEloInfo[p.Name].CtfElo;
                        }
                        else
                        {
                            qlRanksPlayersToUpdate.Add(p.Name);
                            Debug.WriteLine(
                                "Key doesn't exist - no TDM elo data found for player: " + p.Name);
                        }
                        break;
                }
            }
            // If there are players to update, then perform the update.
            if (qlRanksPlayersToUpdate.Count != 0)
            {
                // Create
                QlRanksDataRetriever.CreateEloData(qlRanksPlayersToUpdate);

                Debug.WriteLine("Retrieving missing elo information for player(s): " + string.Join("+", qlRanksPlayersToUpdate));
                var qlr = await QlRanksDataRetriever.GetEloDataFromQlRanksApiAsync(string.Join("+", qlRanksPlayersToUpdate));
                QlRanksDataRetriever.SetQlRanksPlayers(qlr);

                foreach (var p in ToolTipPlayers.Where(p => qlRanksPlayersToUpdate.Contains(p.Name)))
                {
                    EloData val;
                    switch (GameType)
                    {
                        case 0:
                            if (UQltGlobals.PlayerEloInfo.TryGetValue(p.Name.ToLower(), out val))
                            {
                                p.PlayerElo = UQltGlobals.PlayerEloInfo[p.Name].FfaElo;
                            }
                            else
                            {
                                Debug.WriteLine("...Key still does not exist yet for " + p.Name + " [FFA]......");
                            }
                            break;

                        case 1:
                            if (UQltGlobals.PlayerEloInfo.TryGetValue(p.Name.ToLower(), out val))
                            {
                                p.PlayerElo = UQltGlobals.PlayerEloInfo[p.Name].DuelElo;
                            }
                            else
                            {
                                Debug.WriteLine("...Key still does not exist yet for " + p.Name + " [DUEL]......");
                            }
                            break;

                        case 3:
                            if (UQltGlobals.PlayerEloInfo.TryGetValue(p.Name.ToLower(), out val))
                            {
                                p.PlayerElo = UQltGlobals.PlayerEloInfo[p.Name].TdmElo;
                            }
                            else
                            {
                                Debug.WriteLine("...Key still does not exist yet for " + p.Name + " [TDM]......");
                            }
                            break;

                        case 4:
                            if (UQltGlobals.PlayerEloInfo.TryGetValue(p.Name.ToLower(), out val))
                            {
                                p.PlayerElo = UQltGlobals.PlayerEloInfo[p.Name].CaElo;
                            }
                            else
                            {
                                Debug.WriteLine("...Key still does not exist yet for " + p.Name + " [CA]......");
                            }
                            break;

                        case 5:
                            if (UQltGlobals.PlayerEloInfo.TryGetValue(p.Name.ToLower(), out val))
                            {
                                p.PlayerElo = UQltGlobals.PlayerEloInfo[p.Name].CtfElo;
                            }
                            else
                            {
                                Debug.WriteLine("...Key still does not exist yet for " + p.Name + " [CTF]......");
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Sorts and groups the game server's players for game server buddy list tooltip.
        /// </summary>
        /// <param name="players">The players to be sourted</param>
        /// <returns>A sorted list of players.</returns>
        /// <remarks>A List Sort is used rather than the <see cref="GridViewSort"/> helper class because the players are displayed in a tooltip, thus the listview is not selectable. Also used due <see cref="GridViewSort"/>'s inability to auto-sort with this particular use case.</remarks>
        private List<PlayerDetailsViewModel> SortServerPlayersForBuddyList(IEnumerable<Player> players)
        {
            var sorted = players.Select(player => new PlayerDetailsViewModel(player, QlServer)).ToList();
            return sorted.OrderByDescending(a => a.Score).GroupBy(a => a.Team).SelectMany(a => a.ToList()).ToList();
        }
    }
}