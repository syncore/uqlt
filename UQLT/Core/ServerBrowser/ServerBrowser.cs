using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Events;
using UQLT.Helpers;
using UQLT.Models.Filters.Remote;
using UQLT.Models.Filters.User;
using UQLT.Models.QLRanks;
using UQLT.Models.QuakeLiveAPI;
using UQLT.ViewModels;

namespace UQLT.Core.ServerBrowser
{
    /// <summary>
    /// Helper class responsible for server retrieval, pinging servers, and elo details for a ServerBrowserViewModel
    /// </summary>
    public class ServerBrowser
    {
        private readonly IEventAggregator _events;

        // port regexp: colon with at least 4 numbers
        private readonly Regex _port = new Regex(@"[\:]\d{4,}");

        private readonly Timer _serverRefreshTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerBrowser" /> class.
        /// </summary>
        /// <param name="sbvm">The ServerBrowserViewModel associated with this ServerBrowser.</param>
        /// <param name="events">The events that this class publishes and/or receives.</param>
        public ServerBrowser(ServerBrowserViewModel sbvm, IEventAggregator events)
        {
            Sbvm = sbvm;
            _events = events;
            Sbvm.FilterUrl = GetFilterUrlOnLoad();
            _serverRefreshTimer = new Timer();
            if (Sbvm.IsAutoRefreshEnabled)
            {
                StartServerRefreshTimer();
            }
            // Don't hit QL servers (debugging)
            // Async: suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
            var l = LoadServerListAsync(Sbvm.FilterUrl);
        }

        /// <summary>
        /// Gets the ServerBrowserViewModel associated with this ServerBrowser.
        /// </summary>
        /// <value>The ServerBrowserViewModel.</value>
        public ServerBrowserViewModel Sbvm
        {
            get;
            private set;
        }

        /// <summary>
        /// Asynchrounously loads the Quake Live server list from a given /browser/list?filter= URL
        /// for display in the UI.
        /// </summary>
        /// <param name="filterurl">The /browser/list?filter= URL.</param>
        /// <returns>Nothing.</returns>
        public async Task LoadServerListAsync(string filterurl)
        {
            filterurl = Sbvm.FilterUrl;
            Sbvm.IsUpdatingServers = true;
            string detailsurl = await MakeDetailsUrlAsync(filterurl);
            var servers = await GetServersFromDetailsUrlAsync(detailsurl);

            // Must be done on the UI thread since we're updating UI elements
            Execute.OnUIThread(() =>
            {
                Sbvm.Servers.Clear();

                if (servers == null) { return; }
                foreach (var server in servers)
                {
                    Sbvm.Servers.Add(new ServerDetailsViewModel(server));
                }
            });

            // Check if QLRanks supported for these servers.
            ResetServersForEloType();
            CheckServersForEloType();
            
            Sbvm.IsUpdatingServers = false;

            // Send a message (event) to the MainViewModel to update the server count in the statusbar.
            _events.PublishOnUIThread(new ServerCountEvent(Sbvm.Servers.Count));

            // Async: suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
            var qlranksRetriever = new QlRanksDataRetriever();
            // TODO: await?
            var g = qlranksRetriever.GetQlRanksPlayersAsync(servers);
        }

        /// <summary>
        /// Starts the server refresh timer.
        /// </summary>
        public void StartServerRefreshTimer()
        {
            _serverRefreshTimer.Elapsed += OnServerRefresh;
            _serverRefreshTimer.Interval = (Sbvm.AutoRefreshSeconds * 1000);
            _serverRefreshTimer.Enabled = true;
            _serverRefreshTimer.AutoReset = true;
        }

        /// <summary>
        /// Stops the server refresh timer.
        /// </summary>
        public void StopServerRefreshTimer()
        {
            //TODO: stop timer if we have launched a game, to prevent lag during game
            _serverRefreshTimer.Enabled = false;
        }

        /// <summary>
        /// Checks the servers to see what QLRanks Elo types may be represented.
        /// </summary>
        /// <remarks>This is used to correctly display elo search options in the <see cref="ServerBrowserViewModel"/></remarks>
        private void CheckServersForEloType()
        {
            var teamgamesfound = false;
            var duelgamesfound = false;
            foreach (var server in Sbvm.Servers)
            {
                // Server list contains team games that have QLRanks support.
                if (server.IsQlRanksSupportedTeamGame)
                {
                    teamgamesfound = true;
                }

                // Server list contains duel games.
                if (server.GameType == 1)
                {
                    duelgamesfound = true;
                }
            }

            if (teamgamesfound)
            {
                Sbvm.ServersContainQlRanksTeamGames = true;
                Sbvm.SetEloSearchCollectionSource(EloSearchCategoryTypes.TeamGamesOnly);
            }
            if (duelgamesfound)
            {
                Sbvm.ServersContainDuelGames = true;
                Sbvm.SetEloSearchCollectionSource(EloSearchCategoryTypes.DuelGamesOnly);
            }
            if ((teamgamesfound) && (duelgamesfound))
            {
                Sbvm.SetEloSearchCollectionSource(EloSearchCategoryTypes.BothDuelAndTeamGames);
            }

        }

        /// <summary> Gets the filter URL on load. </summary> <returns> The
        /// http: //www.quakelive.com/browser/list?filter= URL with the proper filters base64 encoded
        /// & appended to it. </returns>
        private string GetFilterUrlOnLoad()
        {
            string url;
            if (File.Exists(UQltGlobals.SavedUserFilterPath))
            {
                try
                {
                    using (var sr = new StreamReader(UQltGlobals.SavedUserFilterPath))
                    {
                        string saved = sr.ReadToEnd();
                        var json = JsonConvert.DeserializeObject<SavedFilters>(saved);
                        url = UQltGlobals.QlDomainListFilter + json.fltr_enc;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to retrieve filter url: " + ex);
                    url = UQltGlobals.QlDefaultFilter;
                }
            }
            else
            {
                url = UQltGlobals.QlDefaultFilter;
            }
            return url;
        }

        /// <summary>
        /// Asynchronously retrieves the actual servers from the /browser/details?ids= URL created
        /// by <see cref="MakeDetailsUrlAsync" />.
        /// </summary>
        /// <param name="url">The /browser/details?ids= URL.</param>
        /// <returns>A list of servers specified by the /browser/details?ids= URL</returns>
        /// <remarks>
        /// This method is primarily responsible for retrieving all of the server information that
        /// is seen in the server browser.
        /// </remarks>
        private async Task<IList<Server>> GetServersFromDetailsUrlAsync(string url)
        {
            try
            {
                UQltGlobals.IpAddressDict.Clear();

                var query = new RestApiQuery();
                var serverlist = await (query.QueryRestApiAsync<IList<Server>>(url));
                var playercount = 0;
                var addresses = new HashSet<string>();

                // Process the server and player information for each server.
                foreach (var s in serverlist)
                {
                    // Strip the port off of the ip address.
                    string cleanedip = _port.Replace(s.host_address, string.Empty);
                    addresses.Add(cleanedip);

                    // Set a custom property for game_type for each server's players.
                    s.SetPlayerGameTypeFromServer(s.game_type);

                    // Elo information.
                    foreach (var player in s.players)
                    {
                        EloData val;
                        if (!UQltGlobals.PlayerEloInfo.TryGetValue(player.name.ToLower(), out val))
                        {
                            s.CreateEloData();
                            s.SetPlayerElos();
                        }
                    }

                    // Determine the player count. Because QL is weird, this will not simply
                    // be num_players, but will depend on teamsize (source: ql js)
                    if (s.teamsize > 0)
                    {
                        playercount += s.num_players;
                    }
                    else
                    {
                        playercount += s.num_clients;
                    }
                }

                // Get the ping information.
                await PingServersAsync(addresses);

                // Send a message (event) to the MainViewModel to update the player count in the statusbar.
                _events.PublishOnUIThread(new PlayerCountEvent(playercount));

                return serverlist;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex); // TODO: create a debug log on the disk
                MessageBox.Show("Unable to load Quake Live server data. Refresh and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Asynchronosly retrieves the server ids (public_id's) from a filter in order to make a
        /// /browser/details?ids= URL.
        /// </summary>
        /// <param name="url">The /browser/list?filter= URL.</param>
        /// <returns>
        /// A formatted http://www.quakelive.com/browser/details?ids=id1...id2...idn URL with all of
        /// the server ids appended to it.
        /// </returns>
        private async Task<string> MakeDetailsUrlAsync(string url)
        {
            url = Sbvm.FilterUrl;
            var ids = new HashSet<string>();

            try
            {
                var query = new RestApiQuery();
                var filterdata = await (query.QueryRestApiAsync<QLAPIFilterObject>(url));

                foreach (QLAPIFilterServer qfs in filterdata.servers)
                {
                    ids.Add(qfs.public_id.ToString(CultureInfo.InvariantCulture));
                }

                Debug.WriteLine("Formatted details URL: " + UQltGlobals.QlDomainDetailsIds + string.Join(",", ids));
                return UQltGlobals.QlDomainDetailsIds + string.Join(",", ids);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("Unable to load Quake Live server data. Refresh and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Called every time that the server refresh timer elapses.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs" /> instance containing the event data.</param>
        private void OnServerRefresh(object source, ElapsedEventArgs e)
        {
            Debug.WriteLine("Performing automatic server refresh...");
            // Async: suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
            var l = LoadServerListAsync(Sbvm.FilterUrl);
        }

        /// <summary>
        /// Asynchronously pings a given address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>The round-trip time.</returns>
        private Task<PingReply> PingAsync(string address)
        {
            var tcs = new TaskCompletionSource<PingReply>();
            Ping ping = new Ping();
            ping.PingCompleted += (obj, sender) =>
            {
                tcs.SetResult(sender.Reply);
            };
            ping.SendAsync(address, new object());
            return tcs.Task;
        }

        /// <summary>
        /// Asynchronously pings a list of servers and updates a collection (dictionary) that
        /// contains the server addresses.
        /// </summary>
        /// <param name="servers">The list of servers to ping.</param>
        /// <returns>Nothing</returns>
        private async Task PingServersAsync(IEnumerable<string> servers)
        {
            var pingTasks = new List<Task<PingReply>>();

            foreach (var host in servers)
            {
                UQltGlobals.IpAddressDict[host] = 0;
                pingTasks.Add(PingAsync(host));
            }

            // Wait for all the tasks to complete.
            await Task.WhenAll(pingTasks.ToArray());

            // Iterate and update dictionary.
            foreach (var pingTask in pingTasks)
            {
                UQltGlobals.IpAddressDict[pingTask.Result.Address.ToString()] = pingTask.Result.RoundtripTime;
                // Debug.WriteLine("IP Address: " + pingTask.Result.Address + " time: " +
                // pingTask.Result.RoundtripTime + " ms ");
            }
        }

        /// <summary>
        /// Resets the QLRanks Elo type boolean values for the server list.
        /// </summary>
        private void ResetServersForEloType()
        {
            Sbvm.ServersContainQlRanksTeamGames = false;
            Sbvm.ServersContainDuelGames = false;
        }
    }
}