using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT;
using UQLT.Events;
using UQLT.Models;
using UQLT.Models.Filters.Remote;
using UQLT.Models.QLRanks;
using UQLT.Models.QuakeLiveAPI;
using UQLT.Models.Filters.User;


namespace UQLT.ViewModels
{
    [Export(typeof(ServerBrowserViewModel))]
    public class ServerBrowserViewModel : PropertyChangedBase, IHandle<ServerRequestEvent>
    {
        private static Regex port = new Regex(@"[\:]\d{4,}"); // port regexp: colon with at least 4 numbers
        
        private List<string> currentplayerlist = new List<string>(); // player list for elo updating
        
        private ObservableCollection<ServerDetailsViewModel> _servers;

        public ObservableCollection<ServerDetailsViewModel> Servers
        {
            get 
            { 
                return _servers;
            }
            
            set
            {
                _servers = value;
                NotifyOfPropertyChange(() => Servers);
            }
        }

        private ServerDetailsViewModel _selectedServer;

        public ServerDetailsViewModel SelectedServer
        {
            get 
            { 
                return _selectedServer;
            }
            
            set
            {
                _selectedServer = value;
                NotifyOfPropertyChange(() => SelectedServer);
            }
        }

        private string _filterURL;
 
        public string FilterURL
        {
            get 
            { 
                return _filterURL;
            }
            
            set
            {
                _filterURL = value + Math.Truncate((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
                NotifyOfPropertyChange(() => FilterURL);
            }
        }
        
        [ImportingConstructor]
        public ServerBrowserViewModel(IEventAggregator events)
        {
            events.Subscribe(this);
            _servers = new ObservableCollection<ServerDetailsViewModel>();
            DoServerBrowserAutoSort("LocationName");
            GetAndSetUserFilterUrl();
            InitOrRefreshServers(FilterURL);
        }

        public void Handle(ServerRequestEvent message)
        {
            FilterURL = message.ServerRequestURL;
            InitOrRefreshServers(FilterURL);
            Console.WriteLine("[EVENT RECEIVED] Filter URL Change: " + message.ServerRequestURL);
        }

        private void DoServerBrowserAutoSort(string property)
        {
            var view = CollectionViewSource.GetDefaultView(Servers);
            var sortDescription = new SortDescription(property, ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortDescription);
        }

        private async void InitOrRefreshServers(string url)
        {
            url = FilterURL;
            var detailsurl = await GetServerIdsFromFilter(url);
            var servers = await GetServerList(detailsurl); // TODO: proper filter url
            if (servers != null)
            {
                Servers.Clear();
                foreach (var server in servers)
                {
                    Servers.Add(new ServerDetailsViewModel(server));
                }
            }
        }

        // Get the list of server ids for a given filter, then return a nicely formatted details url
        private async Task<string> GetServerIdsFromFilter(string url)
        {
            url = FilterURL;
            HttpClient client = new HttpClient();
            List<string> ids = new List<string>();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1003.1 Safari/535.19 Awesomium/1.7.1");
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Throw on error code
            string serverfilterjson = await response.Content.ReadAsStringAsync();

            QLAPIFilterObject qlfilter = JsonConvert.DeserializeObject<QLAPIFilterObject>(serverfilterjson);

            foreach (QLAPIFilterServer qfs in qlfilter.servers)
            {
                ids.Add(qfs.public_id.ToString());
            }

            Console.WriteLine("Formatted details URL: " + UQLTGlobals.QLDomainDetailsIds + string.Join(",", ids));
            return UQLTGlobals.QLDomainDetailsIds + string.Join(",", ids);
        }
        
        // Get the actual server details for the list of servers based on the server ids
        // private async Task<IList<Server>> GetServerList(String FilterURL = "http://10.0.0.7/bigtest.json")
        private async Task<IList<Server>> GetServerList(string url)
        {
            // 1.json, 2.json, bigtest.json, hugetest.json, hugetest2.json
            HttpClient client = new HttpClient();

            UQLTGlobals.IPAddressDict.Clear();
            currentplayerlist.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1003.1 Safari/535.19 Awesomium/1.7.1");
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Throw on error code
            string serverdetailsjson = await response.Content.ReadAsStringAsync();

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
                            Console.WriteLine("Player: " + p.name.ToLower() + " has already been indexed. Elo info: [DUEL]: " + UQLTGlobals.PlayerEloDuel[p.name.ToLower()]
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
                // Console.WriteLine("IP Address: " + pingTask.Result.Address + " time: " + pingTask.Result.RoundtripTime + " ms ");
            }

            SplitPlayerList(currentplayerlist);
            return serverlist;
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
                Console.WriteLine("QLR API Call Index: " + i.ToString());
            }

            foreach (var x in list)
            {
                GetQlranksInfo(string.Join("+", x));
                
                // Console.WriteLine("http://www.qlranks.com/api.aspx?nick="+string.Join("+", x));
            }
        }

        private async void GetQlranksInfo(string players)
        {
            HttpClient client = new HttpClient();
            try
            {
                client.BaseAddress = new Uri("http://www.qlranks.com");
                //client.BaseAddress = new Uri("http://10.0.0.7");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1003.1 Safari/535.19 Awesomium/1.7.1");
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
                Console.WriteLine(jEx.Message);
                
                // MessageBox.Show(jEx.Message);
            }
            catch (HttpRequestException ex)
            { 
                Console.WriteLine(ex.Message);
                
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

        private void GetAndSetUserFilterUrl()
        {
            if (File.Exists(UQLTGlobals.SavedUserFilterPath))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(UQLTGlobals.SavedUserFilterPath))
                    {
                        string saved = sr.ReadToEnd();
                        SavedFilters savedFilterJson = JsonConvert.DeserializeObject<SavedFilters>(saved);
                        FilterURL = UQLTGlobals.QLDomainListFilter + savedFilterJson.fltr_enc;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to retrieve filter url: " + ex);
                    FilterURL = UQLTGlobals.QLDefaultFilter;
                }
            }
            else 
            {
                FilterURL = UQLTGlobals.QLDefaultFilter;
            }
        }
    }
}
