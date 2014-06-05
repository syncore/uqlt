using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Events;
using UQLT.Interfaces;
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
    public class ServerBrowser : IQLRanksUpdater
    {
        // port regexp: colon with at least 4 numbers
        private Regex port = new Regex(@"[\:]\d{4,}");

        private Timer ServerRefreshTimer;
        private readonly IEventAggregator _events;

        /// <summary>
        /// Gets the ServerBrowserViewModel associated with this ServerBrowser.
        /// </summary>
        /// <value>
        /// The ServerBrowserViewModel.
        /// </value>
        public ServerBrowserViewModel SBVM
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerBrowser"/> class.
        /// </summary>
        /// <param name="sbvm">The ServerBrowserViewModel associated with this ServerBrowser.</param>
        /// <param name="events">The events that this class publishes and/or receives.</param>
        public ServerBrowser(ServerBrowserViewModel sbvm, IEventAggregator events)
        {
            SBVM = sbvm;
            _events = events;
            SBVM.FilterURL = GetFilterUrlOnLoad();
            ServerRefreshTimer = new Timer();
            if (SBVM.IsAutoRefreshEnabled)
            {
                StartServerRefreshTimer();
            }
            // Don't hit QL servers (debugging)
            //var l = LoadServerListAsync(SBVM.FilterURL);
        }

        /// <summary>
        /// Starts the server refresh timer.
        /// </summary>
        public void StartServerRefreshTimer()
        {
            ServerRefreshTimer.Elapsed += new ElapsedEventHandler(OnServerRefresh);
            ServerRefreshTimer.Interval = (SBVM.AutoRefreshSeconds * 1000);
            ServerRefreshTimer.Enabled = true;
            ServerRefreshTimer.AutoReset = true;
        }

        /// <summary>
        /// Stops the server refresh timer.
        /// </summary>
        public void StopServerRefreshTimer()
        {
            //TODO: stop timer if we have launched a game, to prevent lag during game
            ServerRefreshTimer.Enabled = false;
        }

        // This method is called every X seconds by the ServerRefreshTimer
        /// <summary>
        /// Called every time that the server refresh timer elapses.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void OnServerRefresh(object source, ElapsedEventArgs e)
        {
            Debug.WriteLine("Performing automatic server refresh...");
            var l = LoadServerListAsync(SBVM.FilterURL, true);
        }

        /// <summary>
        /// Gets the filter URL on load.
        /// </summary>
        /// <returns>
        /// The http://www.quakelive.com/browser/list?filter= URL with the proper filters base64 encoded & appended to it.
        /// </returns>
        private string GetFilterUrlOnLoad()
        {
            string url = null;
            if (File.Exists(UQLTGlobals.SavedUserFilterPath))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(UQLTGlobals.SavedUserFilterPath))
                    {
                        string saved = sr.ReadToEnd();
                        SavedFilters savedFilterJson = JsonConvert.DeserializeObject<SavedFilters>(saved);
                        url = UQLTGlobals.QLDomainListFilter + savedFilterJson.fltr_enc;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to retrieve filter url: " + ex);
                    url = UQLTGlobals.QLDefaultFilter;
                }
            }
            else
            {
                url = UQLTGlobals.QLDefaultFilter;
            }
            return url;
        }

        /// <summary>
        /// Asynchrounously loads the Quake Live server list from a given /browser/list?filter= URL for display in the UI.
        /// </summary>
        /// <param name="filterurl">The /browser/list?filter= URL.</param>
        /// <param name="doqlranksupdate">if set to <c>true</c> then perform a QLRanks update for all players within server list.</param>
        /// <returns>
        /// Nothing.
        /// </returns>
        public async Task LoadServerListAsync(string filterurl, bool doqlranksupdate = true)
        {
            filterurl = SBVM.FilterURL;

            SBVM.IsUpdatingServers = true;

            string detailsurl = await MakeDetailsUrlAsync(filterurl);

            IList<Server> servers = await GetServersFromDetailsUrlAsync(detailsurl);

            // Must be done on the UI thread since we're updating UI elements
            Execute.OnUIThread(() =>
            {
                SBVM.Servers.Clear();
                if (servers != null)
                {
                    foreach (var server in servers)
                    {
                        SBVM.Servers.Add(new ServerDetailsViewModel(server));
                    }
                }
            });

            SBVM.IsUpdatingServers = false;

            // Send a message (event) to the MainViewModel to update the server count in the statusbar.
            _events.Publish(new ServerCountEvent(SBVM.Servers.Count));

            if (doqlranksupdate)
            {
                var g = GetQLRanksPlayersAsync(servers);
            }
        }

        /// <summary>
        /// Asynchronosly retrieves the server ids (public_id's) from a filter in order to make a /browser/details?ids= URL.
        /// </summary>
        /// <param name="url">The /browser/list?filter= URL.</param>
        /// <returns>
        /// A formatted http://www.quakelive.com/browser/details?ids=id1...id2...idn URL with all of the server ids appended to it.
        /// </returns>
        private async Task<string> MakeDetailsUrlAsync(string url)
        {
            url = SBVM.FilterURL;
            List<string> ids = new List<string>();
            HttpClientHandler gzipHandler = new HttpClientHandler();
            HttpClient client = new HttpClient(gzipHandler);

            try
            {
                // QL site sends gzip compressed responses
                if (gzipHandler.SupportsAutomaticDecompression)
                {
                    gzipHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }

                client.DefaultRequestHeaders.Add("User-Agent", UQLTGlobals.QLUserAgent);
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throw on error code

                // TODO: Parse server ids from string as a stream, since its frequently larger than 85kb
                // QL site actually doesn't send "application/json", but "text/html" even though it is actually JSON
                // HtmlDecode replaces &gt;, &lt; same as quakelive.js's EscapeHTML function
                string serverfilterjson = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

                QLAPIFilterObject qlfilter = JsonConvert.DeserializeObject<QLAPIFilterObject>(serverfilterjson);

                foreach (QLAPIFilterServer qfs in qlfilter.servers)
                {
                    ids.Add(qfs.public_id.ToString());
                }

                Debug.WriteLine("Formatted details URL: " + UQLTGlobals.QLDomainDetailsIds + string.Join(",", ids));
                return UQLTGlobals.QLDomainDetailsIds + string.Join(",", ids);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("Unable to load Quake Live server data. Refresh and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Asynchronously retrieves the actual servers from the /browser/details?ids= URL created by <see cref="MakeDetailsUrlAsync"/>.
        /// </summary>
        /// <param name="url">The /browser/details?ids= URL.</param>
        /// <returns>
        /// A list of servers specified by the /browser/details?ids= URL
        /// </returns>
        /// <remarks>
        /// Kind of an ugly kitchen sink method, will almost certainly need to be refactored at some point.
        /// </remarks>
        private async Task<IList<Server>> GetServersFromDetailsUrlAsync(string url)
        {
            HttpClientHandler gzipHandler = new HttpClientHandler();
            HttpClient client = new HttpClient(gzipHandler);
            int totalplayercount = 0;
            EloData val;

            try
            {
                // QL site sends gzip compressed responses
                if (gzipHandler.SupportsAutomaticDecompression)
                {
                    gzipHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }

                UQLTGlobals.IPAddressDict.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", UQLTGlobals.QLUserAgent);
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throw on error code

                // QL site actually doesn't send "application/json", but "text/html" even though it is actually JSON
                // HtmlDecode replaces &gt;, &lt; same as quakelive.js's EscapeHTML function
                string serverdetailsjson = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
                ObservableCollection<Server> serverlist = JsonConvert.DeserializeObject<ObservableCollection<Server>>(serverdetailsjson);
                List<string> addresses = new List<string>();

                foreach (Server s in serverlist)
                {
                    string cleanedip = port.Replace(s.host_address, string.Empty);
                    addresses.Add(cleanedip);
                    UQLTGlobals.IPAddressDict[cleanedip] = 0;

                    // set a custom property for game_type for each server's players
                    s.setPlayerGameTypeFromServer(s.game_type);

                    // create EloData for each player in the given server
                    foreach (var player in s.players)
                    {
                        if (!UQLTGlobals.PlayerEloInfo.TryGetValue(player.name.ToLower(), out val))
                        {
                            s.createEloData();
                        }

                        // track the player count
                        totalplayercount++;
                    }

                    // Set the server's players' elo directly on the Player model
                    foreach (var player in s.players)
                    {
                        s.setPlayerElos();
                    }
                }

                List<Task<PingReply>> pingTasks = new List<Task<PingReply>>();

                foreach (string address in addresses)
                {
                    pingTasks.Add(PingAsync(address));
                }

                // wait for all the tasks to complete
                await Task.WhenAll(pingTasks.ToArray());

                // iterate over list of pingTasks
                foreach (var pingTask in pingTasks)
                {
                    UQLTGlobals.IPAddressDict[pingTask.Result.Address.ToString()] = pingTask.Result.RoundtripTime;
                    // Debug.WriteLine("IP Address: " + pingTask.Result.Address + " time: " + pingTask.Result.RoundtripTime + " ms ");
                }

                // Send a message (event) to the MainViewModel to update the player count in the statusbar
                _events.Publish(new PlayerCountEvent(totalplayercount));

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
        /// Asynchronously prepares the player information, given a list of QL servers, to be sent to the QLRanks API in the <see cref="GetEloDataFromQLRanksAPIAsync"/> method.
        /// </summary>
        /// <param name="servers">The list of servers.</param>
        /// <param name="maxPlayers">The maximum number of players to send to the limited QLRanks API per API call.</param>
        /// <returns></returns>
        public async Task GetQLRanksPlayersAsync(IList<Server> servers, int maxPlayers = 150)
        {
            List<string> playerstoupdate = new List<string>();
            List<List<string>> qlrapicalls = new List<List<string>>();
            EloData val;

            // extract players, add to a list to update, split the list, then update
            foreach (var server in servers)
            {
                foreach (var player in server.players)
                {
                    // Elo "caching"
                    if (UQLTGlobals.PlayerEloInfo.TryGetValue(player.name.ToLower(), out val))
                    {
                        // If the player has our pre-defined default elo value of 0 (qlranks elo will never be 0) then add player to a list of players to be updated
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

            // split servers
            for (int i = 0; i < playerstoupdate.Count; i += maxPlayers)
            {
                qlrapicalls.Add(playerstoupdate.GetRange(i, Math.Min(maxPlayers, playerstoupdate.Count - i)));
                Debug.WriteLine("QLRANKS: API Call Index: " + i);
            }

            // perform the tasks
            List<Task<QLRanks>> qlranksTasks = new List<Task<QLRanks>>();

            for (int i = 0; i < qlrapicalls.Count; i++)
            {
                qlranksTasks.Add(GetEloDataFromQLRanksAPIAsync(string.Join("+", qlrapicalls[i])));
                Debug.WriteLine("QLRANKS: API Task " + i + " URL: http://www.qlranks.com/api.aspx?nick=" + string.Join("+", qlrapicalls[i]));
            }

            // all the combined n API calls must finish
            await Task.WhenAll(qlranksTasks.ToArray());

            // set the player elos
            try
            {
                foreach (var qlrt in qlranksTasks)
                {
                    foreach (QLRanksPlayer qp in qlrt.Result.players)
                    {
                        UQLTGlobals.PlayerEloInfo[qp.nick.ToLower()].DuelElo = qp.duel.elo;
                        UQLTGlobals.PlayerEloInfo[qp.nick.ToLower()].CaElo = qp.ca.elo;
                        UQLTGlobals.PlayerEloInfo[qp.nick.ToLower()].TdmElo = qp.tdm.elo;
                        UQLTGlobals.PlayerEloInfo[qp.nick.ToLower()].FfaElo = qp.ffa.elo;
                        UQLTGlobals.PlayerEloInfo[qp.nick.ToLower()].CtfElo = qp.ctf.elo;
                    }
                }
                // Player elos have been set in dictionary, now set on the Player object itself
                // TODO: this will allow using Properties and NotifyPropertyChange to update the player view in the server browser.
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

        /// <summary>
        /// Asynchronously retrieves the player Elo information from the QLRanks API via HTTP GET request(s).
        /// </summary>
        /// <param name="players">The players.</param>
        /// <returns>
        /// The elo information array.
        /// </returns>
        public async Task<QLRanks> GetEloDataFromQLRanksAPIAsync(string players)
        {
            HttpClientHandler gzipHandler = new HttpClientHandler();
            HttpClient client = new HttpClient(gzipHandler);

            try
            {
                client.BaseAddress = new Uri("http://www.qlranks.com");
                //client.BaseAddress = new Uri("http://10.0.0.7");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", UQLTGlobals.QLUserAgent);

                HttpResponseMessage response = await client.GetAsync("/api.aspx?nick=" + players);
                response.EnsureSuccessStatusCode(); // Throw on error code
                string eloinfojson = await response.Content.ReadAsStringAsync();

                QLRanks qlr = JsonConvert.DeserializeObject<QLRanks>(eloinfojson);

                return qlr;
            }
            catch (Newtonsoft.Json.JsonException jEx)
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
        /// Asynchronously pings a given address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>
        /// The round-trip time.
        /// </returns>
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
    }
}