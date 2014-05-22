﻿using Newtonsoft.Json;
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
using UQLT.Interfaces;

namespace UQLT.Core.ServerBrowser
{
    /// <summary>
    /// Helper class responsible for server retrieval, pinging servers, and elo details for a ServerBrowserViewModel
    /// </summary>
    public class ServerBrowser : IQLRanksUpdater
    {
        private Regex port = new Regex(@"[\:]\d{4,}"); // port regexp: colon with at least 4 numbers

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
            InitOrRefreshServers(SBVM.FilterURL);
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
            SBVM.IsUpdatingServers = true;
            var detailsurl = await GetServerIdsFromFilter(url);
            var servers = await GetServerList(detailsurl);
            SBVM.NumberOfServersToUpdate = servers.Count;
            await GetQLRanksPlayers(servers);
            if (servers != null)
            {
                SBVM.Servers.Clear();
                foreach (var server in servers)
                {   
                    SBVM.Servers.Add(new ServerDetailsViewModel(server));
                }
            }
            SBVM.IsUpdatingServers = false;
            SBVM.NumberOfServersToUpdate = 0;
            SBVM.NumberOfPlayersToUpdate = 0;
            
            //await GetQLRanksPlayers(servers);
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
                    UQLTGlobals.IPAddressDict[cleanedip] = 0; // initially set ping at 0

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
                    UQLTGlobals.IPAddressDict[pingTask.Result.Address.ToString()] = pingTask.Result.RoundtripTime;
                    // Debug.WriteLine("IP Address: " + pingTask.Result.Address + " time: " + pingTask.Result.RoundtripTime + " ms ");
                }

                return serverlist;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex); // TODO: create a debug log on the disk
                MessageBox.Show("Unable to load Quake Live server data. Try refreshing manually.");
                return null;
            }
        }
        public void SetQLranksDefaultElo(string player)
        {
            UQLTGlobals.PlayerEloInfo[player] = new EloData() { DuelElo = 0, CaElo = 0, TdmElo = 0, FfaElo = 0, CtfElo = 0 };
        }

        // Extract the players from the server list
        public async Task GetQLRanksPlayers(IList<Server> servers, int maxPlayers = 150)
        {
            List<string> playerstoupdate = new List<string>();
            List<List<string>> qlrapicalls = new List<List<string>>();
            // extract players, add to a list to update, then split the list, then update

            foreach (var s in servers)
            {
                foreach (var player in s.players)
                {
                    // add to a list of players to be updated
                    playerstoupdate.Add(player.name.ToLower());
                    // create the QLRanks elo data object for the player and set elo to 0
                    SetQLranksDefaultElo(player.name.ToLower());
                }

            }
            SBVM.NumberOfPlayersToUpdate = playerstoupdate.Count;

            // split servers
            for (int i = 0; i < playerstoupdate.Count; i += maxPlayers)
            {
                qlrapicalls.Add(playerstoupdate.GetRange(i, Math.Min(maxPlayers, playerstoupdate.Count - i)));
                Debug.WriteLine("QLRANKS: API Call Index: " + i.ToString());
            }

            // perform the tasks
            List<Task<QLRanks>> qlrankstasks = new List<Task<QLRanks>>();

            for (int i = 0; i < qlrapicalls.Count; i++)
            {
                qlrankstasks.Add(GetEloDataFromQLRanksAPI(string.Join("+", qlrapicalls[i])));
                Debug.WriteLine("QLRANKS: API Task: " + i + " URL: http://www.qlranks.com/api.aspx?nick=" + string.Join("+", qlrapicalls[i]));
            }
            await Task.WhenAll(qlrankstasks.ToArray());
            
            
            try {
            // set the player elos
            foreach (var qlrt in qlrankstasks)
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
        } catch (Exception e) {
            MessageBox.Show("Error retrieving QLRanks data.");
            Debug.WriteLine(e);
        }
        }


        public async Task<QLRanks> GetEloDataFromQLRanksAPI(string players)
        {
            HttpClient client = new HttpClient();
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
                // This exception indicates a problem deserializing the request body.
                Debug.WriteLine(jEx.Message);
                return null;

                // MessageBox.Show(jEx.Message);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.Message);
                return null;

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


