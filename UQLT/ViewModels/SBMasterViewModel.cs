using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.ComponentModel.Composition;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using UQLT.Models;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.NetworkInformation;

namespace UQLT.ViewModels
{
    [Export(typeof(SBMasterViewModel))]
    public class SBMasterViewModel : PropertyChangedBase
    {
        // fields
        static Regex port = new Regex(@"[\:]\d{4,}"); // port regexp: colon with at least 4 numbers
        private ObservableCollection<ServerDetails> _server_details;
        private ObservableCollection<Player> _players;
        private int _num_players;
        private int _public_id;
        private int _ECODE;
        private int _teamsize;
        private string _g_customSettings;
        private int _g_levelstarttime;
        private int _location_id;
        private int _max_clients;
        private int _roundtimelimit;
        private string _map_title;
        private string _scorelimit;
        private string _ruleset;
        private int _skillDelta;
        private string _game_type_title;
        private string _map;
        private object _premium;
        private int _g_needpass;
        private int _ranked;
        private int _g_instagib;
        private int _g_bluescore;
        private string _g_gamestate;
        private string _host_address;
        private int _fraglimit;
        private int _num_clients;
        private int _capturelimit;
        private int _game_type;
        private int _timelimit;
        private int _roundlimit;
        private string _host_name;
        private int _g_redscore;
        private string _owner;
        private string _location_name;
        // player list for elo updating
        List<String> currentplayerlist = new List<string>();

        [ImportingConstructor]
        public SBMasterViewModel()
        {
            // nothing here yet - in the future it will be the server filter url
            _server_details = new ObservableCollection<ServerDetails>();
            GetServerList();
        }

        public ObservableCollection<ServerDetails> server_details
        {
            get { return _server_details;  }
            set { _server_details = value; NotifyOfPropertyChange(() => server_details); }
        }
        
        public ObservableCollection<Player> Players
        {
            get { return this._players; }
            set { this._players = value; NotifyOfPropertyChange(() => Players); }
        }

        public int num_players
        {
            get { return _num_players; }
            set { num_players = value; NotifyOfPropertyChange(() => num_players); }
        }

        public int public_id
        {
            get { return _public_id; }
            set { public_id = value; NotifyOfPropertyChange(() => public_id); }
        }
        public int ECODE
        {
            get { return _ECODE; }
            set { ECODE = value; NotifyOfPropertyChange(() => ECODE); }
        }
        public int teamsize
        {
            get { return _teamsize; }
            set { teamsize = value; NotifyOfPropertyChange(() => teamsize); }
        }
        public string g_customSettings
        {
            get { return _g_customSettings; }
            set { g_customSettings = value; NotifyOfPropertyChange(() => g_customSettings); }
        }
        public int g_levelstarttime
        {
            get { return _g_levelstarttime; }
            set { g_levelstarttime = value; NotifyOfPropertyChange(() => g_levelstarttime); }
        }
        public int location_id
        {
            get { return _location_id; }
            set { location_id = value; NotifyOfPropertyChange(() => location_id); }
        }

        public int max_clients
        {
            get { return _max_clients; }
            set { max_clients = value; NotifyOfPropertyChange(() => max_clients); }
        }
        public int roundtimelimit
        {
            get { return _roundtimelimit; }
            set { roundtimelimit = value; NotifyOfPropertyChange(() => roundtimelimit); }
        }
        public string map_title
        {
            get { return _map_title; }
            set { map_title = value; NotifyOfPropertyChange(() => map_title); }
        }
        public string scorelimit
        {
            get { return _scorelimit; }
            set { scorelimit = value; NotifyOfPropertyChange(() => scorelimit); }
        }
        public string ruleset
        {
            get { return _ruleset; }
            set { ruleset = value; NotifyOfPropertyChange(() => ruleset); }
        }
        public int skillDelta
        {
            get { return _skillDelta; }
            set { skillDelta = value; NotifyOfPropertyChange(() => skillDelta); }
        }
        public string game_type_title
        {
            get { return _game_type_title; }
            set { game_type_title = value; NotifyOfPropertyChange(() => game_type_title); }
        }
        public string map
        {
            get { return _map; }
            set { map = value; NotifyOfPropertyChange(() => map); }
        }
        public object premium
        {
            get { return _premium; }
            set { premium = value; NotifyOfPropertyChange(() => premium); }
        }
        public int g_needpass
        {
            get { return _g_needpass; }
            set { g_needpass = value; NotifyOfPropertyChange(() => g_needpass); }
        }
        public int ranked
        {
            get { return _ranked; }
            set { ranked = value; NotifyOfPropertyChange(() => ranked); }
        }
        public int g_instagib
        {
            get { return _g_instagib; }
            set { g_instagib = value; NotifyOfPropertyChange(() => g_instagib); }
        }
        public int g_bluescore
        {
            get { return _g_bluescore; }
            set { g_bluescore = value; NotifyOfPropertyChange(() => g_bluescore); }
        }
        public string g_gamestate
        {
            get { return _g_gamestate; }
            set { g_gamestate = value; NotifyOfPropertyChange(() => g_gamestate); }
        }
        public string host_address
        {
            get { return _host_address; }
            set { host_address = value; NotifyOfPropertyChange(() => host_address); }
        }
        public int fraglimit
        {
            get { return _fraglimit; }
            set { fraglimit = value; NotifyOfPropertyChange(() => fraglimit); }
        }
        public int num_clients
        {
            get { return _num_clients; }
            set { num_clients = value; NotifyOfPropertyChange(() => num_clients); }
        }
        public int capturelimit
        {
            get { return _capturelimit; }
            set { capturelimit = value; NotifyOfPropertyChange(() => capturelimit); }
        }
        public int game_type
        {
            get { return _game_type; }
            set { game_type = value; NotifyOfPropertyChange(() => game_type); }
        }
        public int timelimit
        {
            get { return _timelimit; }
            set { timelimit = value; NotifyOfPropertyChange(() => timelimit); }
        }
        public int roundlimit
        {
            get { return _roundlimit; }
            set { roundlimit = value; NotifyOfPropertyChange(() => roundlimit); }
        }
        public string host_name
        {
            get { return _host_name; }
            set { host_name = value; NotifyOfPropertyChange(() => host_name); }
        }
        public int g_redscore
        {
            get { return _g_redscore; }
            set { g_redscore = value; NotifyOfPropertyChange(() => g_redscore); }
        }
        public string owner
        {
            get { return _owner; }
            set { owner = value; NotifyOfPropertyChange(() => owner); }
        }

        // specially formatted properties for view:
        public long ping
        {
            get
            {
                string cleanedip = port.Replace(host_address, "");
                return UQLTGlobals.ipdict[cleanedip];
            }
        }
        
        public ImageSource flag_image
        {
            get
            {
                try
                {
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/flags/" + location_id.ToString() + ".gif", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/flags/unknown_flag.gif", UriKind.RelativeOrAbsolute));
                }
            }
        }
        public ImageSource gametype_image
        {
            get
            {
                try
                {
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/" + game_type.ToString() + ".gif", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/unknown_game_type.gif", UriKind.RelativeOrAbsolute));
                }
            }
        }
        public string totalplayers
        {
            get { return "" + num_players + "/" + max_clients; }
        }

        public string modded
        {
            get { return (g_customSettings.Equals("0")) ? "No" : "Yes"; }
        }

        public string instagib
        {
            get { return (g_instagib == 0) ? "No" : "Yes"; }
        }

        // QL does not include the physical location in the server detauls API, so this monstrosity is used for the info pane and for sorting the listview header by location
        public string location_name
        {
            get
            {
                switch (location_id)
                {
                    case 6:
                        _location_name = "USA, Dallas, TX";
                        break;
                    case 10:
                        _location_name = "USA, Palo Alto, CA";
                        break;
                    case 11:
                        _location_name = "USA, Ashburn, VA";
                        break;
                    case 12:
                        _location_name = "USA, Richardson, TX (LAN)";
                        break;
                    case 14:
                        _location_name = "AUS, Sydney";
                        break;
                    case 16:
                        _location_name = "USA, San Francisco, CA";
                        break;
                    case 17:
                        _location_name = "NLD, Amsterdam";
                        break;
                    case 18:
                        _location_name = "DEU, Frankfurt";
                        break;
                    case 19:
                        _location_name = "GBR, Maidenhead";
                        break;
                    case 20:
                        _location_name = "FRA, Paris";
                        break;
                    case 21:
                        _location_name = "USA, Chicago, IL";
                        break;
                    case 22:
                        _location_name = "USA, Atlanta, GA";
                        break;
                    case 23:
                        _location_name = "USA, Seattle, WA";
                        break;
                    case 24:
                        _location_name = "USA, New York, NY";
                        break;
                    case 25:
                        _location_name = "USA, Los Angeles, CA";
                        break;
                    case 26:
                        _location_name = "CAN, Toronto, ON";
                        break;
                    case 27:
                        _location_name = "JPN, Tokyo";
                        break;
                    case 28:
                        _location_name = "ESP, Madrid";
                        break;
                    case 29:
                        _location_name = "SWE, Stockholm";
                        break;
                    case 30:
                        _location_name = "POL, Warsaw";
                        break;
                    case 31:
                        _location_name = "CHN, Hangzhou";
                        break;
                    case 32:
                        _location_name = "POL, Warsaw";
                        break;
                    case 33:
                        _location_name = "AUS, Sydney";
                        break;
                    case 34:
                        _location_name = "SWE, Malmo";
                        break;
                    case 35:
                        _location_name = "AUS, Perth";
                        break;
                    case 36:
                        _location_name = "SWE, Stockholm";
                        break;
                    case 37:
                        _location_name = "ROM, Bucharest";
                        break;
                    case 38:
                        _location_name = "CHL, Santiago";
                        break;
                    case 39:
                        _location_name = "ROM, Bucharest";
                        break;
                    case 40:
                        _location_name = "ARG, Buenos Aires";
                        break;
                    case 41:
                        _location_name = "ISL, Keflavik";
                        break;
                    case 42:
                        _location_name = "JPN, Tokyo";
                        break;
                    case 43:
                        _location_name = "RUS, Moscow";
                        break;
                    case 44:
                        _location_name = "RUS, Moscow";
                        break;
                    case 45:
                        _location_name = "SGP, Singapore";
                        break;
                    case 46:
                        _location_name = "ZAF, Johannesburg";
                        break;
                    case 47:
                        _location_name = "SRB, Beograd";
                        break;
                    case 48:
                        _location_name = "BGR, Sofia";
                        break;
                    case 49:
                        _location_name = "KOR, Seoul";
                        break;
                    case 50:
                        _location_name = "ITA, Milan";
                        break;
                    case 51:
                        _location_name = "AUS, Adelaide";
                        break;
                    case 52:
                        _location_name = "DEU, Cologne (LAN)";
                        break;
                    case 53:
                        _location_name = "USA, Dallas,TX (LAN)";
                        break;
                    case 54:
                        _location_name = "SWE, Jonkoping (LAN)";
                        break;
                    case 58:
                        _location_name = "UKR, Kiev";
                        break;
                    case 59:
                        _location_name = "ITA, Lignano Sabbiadoro (LAN)";
                        break;
                    case 60:
                        _location_name = "AUS, Adelaide (LAN)";
                        break;
                    case 61:
                        _location_name = "NLD, Benelux (LAN)";
                        break;
                    case 62:
                        _location_name = "USA, Washington DC";
                        break;
                    case 666:
                        _location_name = "USA, QuakeCon LAN";
                        break;
                    case 63:
                        _location_name = "USA, Indianapolis, IN";
                        break;
                    case 64:
                        _location_name = "NLD, Rotterdam";
                        break;
                    case 65:
                        _location_name = "NOR, Oslo";
                        break;
                    case 66:
                        _location_name = "BRA, Sao Paulo";
                        break;
                    case 67:
                        _location_name = "TUR, Istanbul";
                        break;
                    case 68:
                        _location_name = "NZL, Auckland";
                        break;
                    default:
                        _location_name = "Unknown";
                        break;
                }
                return _location_name;
            }
        }
        
        private async void GetServerList(String FilterURL = "http://10.0.0.7/2.json")
        {
            HttpClient client = new HttpClient();
            try
            {
                UQLTGlobals.ipdict.Clear();
                currentplayerlist.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1003.1 Safari/535.19 Awesomium/1.7.1");
                //HttpResponseMessage response1 = await client.GetAsync("1.json"); // filter url that provides server ids (sans players)
                //HttpResponseMessage response2 = await client.GetAsync("2.json");
                //HttpResponseMessage response2 = await client.GetAsync("bigtest.json");
                //HttpResponseMessage response2 = await client.GetAsync("hugetest.json");
                //HttpResponseMessage response2 = await client.GetAsync("hugetest2.json");
                //response1.EnsureSuccessStatusCode(); // Throw on error code
                HttpResponseMessage response = await client.GetAsync(FilterURL);
                response.EnsureSuccessStatusCode(); // Throw on error code
                //var serverlistjson = await response1.Content.ReadAsStringAsync();
                String serverdetailsjson = await response.Content.ReadAsStringAsync();

                ObservableCollection<ServerDetails> serverdetails = JsonConvert.DeserializeObject<ObservableCollection<ServerDetails>>(serverdetailsjson);
                List<String> addresses = new List<string>();
                int elo;

                foreach (ServerDetails s in serverdetails)
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

                // set the viewmodel properties to the model
                server_details = serverdetails;
                
                //_server_details = serverdetails;
                



                // UI: automatically sort the server browser view by location
                //CollectionViewSource.GetDefaultView(serverdetails).SortDescriptions.Add(new SortDescription("cust_location", ListSortDirection.Ascending));

                // UI: automatically sort the server details view by score and group by team
                //((ItemCollection)(ServerDetailList.Items)).SortDescriptions.Add(new SortDescription("score", ListSortDirection.Descending));
                //((ItemCollection)(ServerDetailList.Items)).GroupDescriptions.Add(new PropertyGroupDescription("team"));

                //Console.WriteLine("Current playerlist List: " + string.Join(",", currentplayerlist));
                splitPlayerList(currentplayerlist);


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
                //client.BaseAddress = new Uri("http://www.qlranks.com");
                client.BaseAddress = new Uri("http://10.0.0.7");
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
    }
}
