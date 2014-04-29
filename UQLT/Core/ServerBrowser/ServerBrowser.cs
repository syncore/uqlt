using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using UQLT.Models;
using UQLT.Models.Filters.Remote;
using UQLT.Models.Filters.User;
using UQLT.Models.QLRanks;
using UQLT.Models.QuakeLiveAPI;
using UQLT.ViewModels;

namespace UQLT.Core.ServerBrowser
{
    // Helper class responsible for server retrieval and elo details for a ServerBrowserViewModel
    public class ServerBrowser
    {
        private List<string> currentplayerlist = new List<string>(); // player list for elo updating

        private static Regex port = new Regex(@"[\:]\d{4,}"); // port regexp: colon with at least 4 numbers
        
        public ServerBrowserViewModel SBVM
        {
            get;
            private set;
        }

        public ServerBrowser(ServerBrowserViewModel sbvm)
        {
            SBVM = sbvm;
            SBVM.FilterURL = GetFilterUrlOnLoad();
            // Don't hit QL servers (debugging)
            //InitOrRefreshServers(SBVM.FilterURL);
        }

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

        public async void InitOrRefreshServers(string url)
        {
            url = SBVM.FilterURL;
            var detailsurl = await GetServerIdsFromFilter(url);
            var servers = await GetServerList(detailsurl);
            if (servers != null)
            {
                SBVM.Servers.Clear();
                foreach (var server in servers)
                {
                    SBVM.Servers.Add(new ServerDetailsViewModel(server));
                }
            }
        }

        // Get the list of server ids for a given filter, then return a nicely formatted details url
        private async Task<string> GetServerIdsFromFilter(string url)
        {
            url = SBVM.FilterURL;
            List<string> ids = new List<string>();

            try
            {

                HttpClientHandler gzipHandler = new HttpClientHandler();
                HttpClient client = new HttpClient(gzipHandler);

                // QL site sends gzip compressed responses
                if (gzipHandler.SupportsAutomaticDecompression)
                    gzipHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
                MessageBox.Show("Unable to load Quake Live server data. Try refreshing manually.");
                return null;
            }


        }

        // Get the actual server details for the list of servers based on the server ids
        private async Task<IList<Server>> GetServerList(string url)
        {
            try
            {
                // 1.json, 2.json, bigtest.json, hugetest.json, hugetest2.json
                HttpClientHandler gzipHandler = new HttpClientHandler();
                HttpClient client = new HttpClient(gzipHandler);

                // QL site sends gzip compressed responses
                if (gzipHandler.SupportsAutomaticDecompression)
                    gzipHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                UQLTGlobals.IPAddressDict.Clear();
                currentplayerlist.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", UQLTGlobals.QLUserAgent);
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throw on error code

                // QL site actually doesn't send "application/json", but "text/html" even though it is actually JSON
                // HtmlDecode replaces &gt;, &lt; same as quakelive.js's EscapeHTML function
                string serverdetailsjson = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

                ObservableCollection<Server> serverlist = JsonConvert.DeserializeObject<ObservableCollection<Server>>(serverdetailsjson);
                List<string> addresses = new List<string>();
                int elo;

                foreach (Server s in serverlist)
                {
                    string cleanedip = port.Replace(s.host_address, string.Empty);
                    addresses.Add(cleanedip);
                    UQLTGlobals.IPAddressDict.TryAdd(cleanedip, 0); // initially set ping at 0

                    foreach (Player p in s.players)
                    {
                        // in the interests of time, only check duel elo. player will be included in all dicts if in duel dict
                        if (!UQLTGlobals.PlayerEloDuel.TryGetValue(p.name.ToLower(), out elo))
                        {
                            currentplayerlist.Add(p.name.ToLower());

                            // temporarily set default elo to 0
                            SetQLranksDefaultElo(p.name.ToLower());
                        }
                        else
                        {
                            if (elo != 0)
                            {
                                Debug.WriteLine("Player: " + p.name.ToLower() + " has already been indexed. Elo info: [DUEL]: " + UQLTGlobals.PlayerEloDuel[p.name.ToLower()]
                                    + " [CA]: " + UQLTGlobals.PlayerEloCa[p.name.ToLower()] + " [TDM]: " + UQLTGlobals.PlayerEloTdm[p.name.ToLower()] + " [CTF]: "
                                    + UQLTGlobals.PlayerEloCtf[p.name.ToLower()] + " [FFA]: " + UQLTGlobals.PlayerEloFfa[p.name.ToLower()]);
                            }
                            else
                            {
                                SetQLranksDefaultElo(p.name.ToLower());
                            }
                        }
                    }

                    // set a custom property for game_type for each server's players
                    s.setPlayerGameTypeFromServer(s.game_type);
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
                    UQLTGlobals.IPAddressDict.TryUpdate(pingTask.Result.Address.ToString(), pingTask.Result.RoundtripTime, 0); // update based on ping response time
                    // Debug.WriteLine("IP Address: " + pingTask.Result.Address + " time: " + pingTask.Result.RoundtripTime + " ms ");
                }

                SplitPlayerList(currentplayerlist);
                return serverlist;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex); // TODO: debug log
                MessageBox.Show("Unable to load Quake Live server data. Try refreshing manually.");
                return null;
            }
        }
        private void SetQLranksDefaultElo(string player)
        {
            UQLTGlobals.PlayerEloDuel.TryAdd(player, 0);
            UQLTGlobals.PlayerEloCa.TryAdd(player, 0);
            UQLTGlobals.PlayerEloTdm.TryAdd(player, 0);
            UQLTGlobals.PlayerEloFfa.TryAdd(player, 0);
            UQLTGlobals.PlayerEloCtf.TryAdd(player, 0);
        }

        private void SetQLranksInfo(string player, int duelElo, int caElo, int tdmElo, int ffaElo, int ctfElo)
        {
            UQLTGlobals.PlayerEloDuel.TryUpdate(player, duelElo, 0);
            UQLTGlobals.PlayerEloCa.TryUpdate(player, caElo, 0);
            UQLTGlobals.PlayerEloTdm.TryUpdate(player, tdmElo, 0);
            UQLTGlobals.PlayerEloFfa.TryUpdate(player, ffaElo, 0);
            UQLTGlobals.PlayerEloCtf.TryUpdate(player, ctfElo, 0);
        }

        private void SplitPlayerList(List<string> input, int maxPlayers = 150)
        {
            List<List<string>> list = new List<List<string>>();

            for (int i = 0; i < currentplayerlist.Count; i += maxPlayers)
            {
                list.Add(currentplayerlist.GetRange(i, Math.Min(maxPlayers, currentplayerlist.Count - i)));
                Debug.WriteLine("QLR API Call Index: " + i.ToString());
            }

            foreach (var x in list)
            {
                GetQlranksInfo(string.Join("+", x));

                // Debug.WriteLine("http://www.qlranks.com/api.aspx?nick="+string.Join("+", x));
            }
        }

        private async void GetQlranksInfo(string players)
        {
            HttpClient client = new HttpClient();
            try
            {
                //client.BaseAddress = new Uri("http://www.qlranks.com");
                client.BaseAddress = new Uri("http://10.0.0.7");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", UQLTGlobals.QLUserAgent);
                HttpResponseMessage response = await client.GetAsync("/api.aspx?nick=" + players);
                response.EnsureSuccessStatusCode(); // Throw on error code
                string eloinfojson = await response.Content.ReadAsStringAsync();

                QLRanks qlr = JsonConvert.DeserializeObject<QLRanks>(eloinfojson);

                foreach (QLRanksPlayer qp in qlr.players)
                {
                    SetQLranksInfo(qp.nick.ToLower(), qp.duel.elo, qp.ca.elo, qp.tdm.elo, qp.ffa.elo, qp.ctf.elo);
                }
            }
            catch (Newtonsoft.Json.JsonException jEx)
            {
                // This exception indicates a problem deserializing the request body.
                Debug.WriteLine(jEx.Message);

                // MessageBox.Show(jEx.Message);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.Message);

                // MessageBox.Show(ex.Message);
            }
        }

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

    

