using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.ComponentModel.Composition;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Collections.ObjectModel;
using UQLT.Models;
using UQLT.Events;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.ComponentModel;

namespace UQLT.ViewModels
{
    [Export(typeof(ServerBrowserViewModel))]
    public class ServerBrowserViewModel : PropertyChangedBase, IHandle<ServerRequestEvent>
    {

        static Regex port = new Regex(@"[\:]\d{4,}"); // port regexp: colon with at least 4 numbers
        List<String> currentplayerlist = new List<string>(); // player list for elo updating
        
        [ImportingConstructor]
        public ServerBrowserViewModel(IEventAggregator events)
        {
            events.Subscribe(this);
            _servers = new ObservableCollection<ServerDetailsViewModel>();
            DoServerBrowserAutoSort("location_name");
            InitOrRefreshServers(FilterURL);
        }

        private void DoServerBrowserAutoSort(string property)
        {
            var view = CollectionViewSource.GetDefaultView(Servers);
            var sortDescription = new SortDescription(property, ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortDescription);
        }

        private ObservableCollection<ServerDetailsViewModel> _servers;
        public ObservableCollection<ServerDetailsViewModel> Servers
        {
            get { return _servers; }
            set
            {
                _servers = value;
                NotifyOfPropertyChange(() => Servers);
            }
        }

        private ServerDetailsViewModel _selectedServer;
        public ServerDetailsViewModel SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                _selectedServer = value;
                NotifyOfPropertyChange(() => SelectedServer);
            }
        }

        // default value for filter URL
        private string _filterURL = "http://www.quakelive.com/browser/list?filter=eyJmaWx0ZXJzIjp7Imdyb3VwIjoiYW55IiwiZ2FtZV90eXBlIjoiYW55IiwiYXJlbmEiOiJhbnkiLCJzdGF0ZSI6ImFueSIsImRpZmZpY3VsdHkiOiJhbnkiLCJsb2NhdGlvbiI6ImFueSIsInByaXZhdGUiOjAsInByZW1pdW1fb25seSI6MCwiaW52aXRhdGlvbl9vbmx5IjowfSwiYXJlbmFfdHlwZSI6IiIsInBsYXllcnMiOltdLCJnYW1lX3R5cGVzIjpbXSwiaWciOjB9&_=1391549611901";
        // TODO: save the filter url in the savedfilters on disk, also code a failsafe default url
        public string FilterURL
        {
            get { return _filterURL; }
            set
            {
                _filterURL = value;
                NotifyOfPropertyChange(() => FilterURL);
            }
        }
        
        private async void InitOrRefreshServers(string url)
        {
            url = FilterURL;
            var detailsurl = await GetServerIdsFromFilter(url);
            var servers = await GetServerList(detailsurl); //TODO: proper filter url
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
            List<String> ids = new List<string>();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1003.1 Safari/535.19 Awesomium/1.7.1");
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Throw on error code
            String serverfilterjson = await response.Content.ReadAsStringAsync();

            QLFilterObject qlfilter = JsonConvert.DeserializeObject<QLFilterObject>(serverfilterjson);

            foreach (QLFilterServer qfs in qlfilter.servers) {
                ids.Add(qfs.public_id.ToString());
            }

            Console.WriteLine("Formatted details URL: " + "http://www.quakelive.com/browser/details?ids=" + string.Join(",", ids));
            return "http://www.quakelive.com/browser/details?ids=" + string.Join(",", ids);
        }
        
        
        // Get the actual server details for the list of servers based on the server ids
        //private async Task<IList<Server>> GetServerList(String FilterURL = "http://10.0.0.7/bigtest.json")
        private async Task<IList<Server>> GetServerList(string url)
        {
            // 1.json, 2.json, bigtest.json, hugetest.json, hugetest2.json
            HttpClient client = new HttpClient();

            UQLTGlobals.ipdict.Clear();
            currentplayerlist.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1003.1 Safari/535.19 Awesomium/1.7.1");
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Throw on error code
            String serverdetailsjson = await response.Content.ReadAsStringAsync();

            ObservableCollection<Server> serverlist = JsonConvert.DeserializeObject<ObservableCollection<Server>>(serverdetailsjson);
            List<String> addresses = new List<string>();
            int elo;

            foreach (Server s in serverlist)
            {
                string cleanedip = port.Replace(s.host_address, "");
                addresses.Add(cleanedip);
                UQLTGlobals.ipdict.TryAdd(cleanedip, 0); // initially set ping at 0

                foreach (Player p in s.players)
                {
                    // in the interests of time, only check duel elo. player will be included in all dicts if in duel dict
                    if (!UQLTGlobals.playereloduel.TryGetValue(p.name.ToLower(), out elo))
                    {
                        currentplayerlist.Add(p.name.ToLower());
                        //temporarily set default elo to 0
                        SetQLranksDefaultElo(p.name.ToLower());
                    }
                    else
                    {
                        if (elo != 0)
                        {
                            Console.WriteLine("Player: " + p.name.ToLower() + " has already been indexed. Elo info: [DUEL]: " + UQLTGlobals.playereloduel[p.name.ToLower()]
                                + " [CA]: " + UQLTGlobals.playereloca[p.name.ToLower()] + " [TDM]: " + UQLTGlobals.playerelotdm[p.name.ToLower()] + " [CTF]: "
                                + UQLTGlobals.playereloctf[p.name.ToLower()] + " [FFA]: " + UQLTGlobals.playereloffa[p.name.ToLower()]);
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
            foreach (String address in addresses)
            {
                pingTasks.Add(PingAsync(address));
            }

            //wait for all the tasks to complete
            await Task.WhenAll(pingTasks.ToArray());

            //iterate over list of pingTasks
            foreach (var pingTask in pingTasks)
            {
                UQLTGlobals.ipdict.TryUpdate(pingTask.Result.Address.ToString(), pingTask.Result.RoundtripTime, 0); // update based on ping response time
                //Console.WriteLine("IP Address: " + pingTask.Result.Address + " time: " + pingTask.Result.RoundtripTime + " ms ");
            }
            splitPlayerList(currentplayerlist);
            return serverlist;
        }


        private void SetQLranksDefaultElo(String player)
        {
            UQLTGlobals.playereloduel.TryAdd(player, 0);
            UQLTGlobals.playereloca.TryAdd(player, 0);
            UQLTGlobals.playerelotdm.TryAdd(player, 0);
            UQLTGlobals.playereloffa.TryAdd(player, 0);
            UQLTGlobals.playereloctf.TryAdd(player, 0);
        }
        private void SetQLranksInfo(String player, int duelelo, int caelo, int tdmelo, int ffaelo, int ctfelo)
        {
            UQLTGlobals.playereloduel.TryUpdate(player, duelelo, 0);
            UQLTGlobals.playereloca.TryUpdate(player, caelo, 0);
            UQLTGlobals.playerelotdm.TryUpdate(player, tdmelo, 0);
            UQLTGlobals.playereloffa.TryUpdate(player, ffaelo, 0);
            UQLTGlobals.playereloctf.TryUpdate(player, ctfelo, 0);
        }
        private void splitPlayerList(List<string> input, int maxPlayers = 150)
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
                //Console.WriteLine("http://www.qlranks.com/api.aspx?nick="+string.Join("+", x));
            }

        }

        private async void GetQlranksInfo(String players)
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
                String eloinfojson = await response.Content.ReadAsStringAsync();

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
                //MessageBox.Show(jEx.Message);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                //MessageBox.Show(ex.Message);
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
        public void Handle(ServerRequestEvent message)
        {
            FilterURL = message.ServerRequestURL;
            InitOrRefreshServers(FilterURL);
            Console.WriteLine("[EVENT] Filter URL: " + message.ServerRequestURL);
        }
    }
}
