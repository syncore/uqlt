using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.ComponentModel.Composition;
using UQLT.Models;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Data;
using System.ComponentModel;

namespace UQLT.ViewModels
{
    [Export(typeof(ServerDetailsViewModel))]
    public class ServerDetailsViewModel : PropertyChangedBase
    {
        public Server Server
        {
            get;
            private set;
        }

        static Regex port = new Regex(@"[\:]\d{4,}"); // port regexp: colon with at least 4 numbers

        [ImportingConstructor]
        public ServerDetailsViewModel(Server server)
        {
            Server = server;
            _FormattedPlayerList = new ObservableCollection<PlayerDetailsViewModel>();
            _FormattedPlayerList = AddFormattedPlayers(server.players);
            GroupScoresAndPlayers("score", "team");
        }

        private void GroupScoresAndPlayers(string sortby, string groupby)
        {
            var view = CollectionViewSource.GetDefaultView(FormattedPlayerList);
            var sortDescription = new SortDescription(sortby, ListSortDirection.Descending);
            var groupDescription = new PropertyGroupDescription(groupby);
            view.SortDescriptions.Add(sortDescription);
            view.GroupDescriptions.Add(groupDescription);
        }

        private ObservableCollection<PlayerDetailsViewModel> AddFormattedPlayers(List<Player> players)
        {
            _FormattedPlayerList.Clear();
            foreach (var player in players)
            {
                _FormattedPlayerList.Add(new PlayerDetailsViewModel(player));
            }
            return _FormattedPlayerList;
        }

        private ObservableCollection<PlayerDetailsViewModel> _FormattedPlayerList;
        public ObservableCollection<PlayerDetailsViewModel> FormattedPlayerList
        {
            get { return _FormattedPlayerList; }
            set
            {
                _FormattedPlayerList = value;
                NotifyOfPropertyChange(() => FormattedPlayerList);
            }
        }

        public List<Player> Players
        {
            get { return Server.players; }
            // set { this._players = value; NotifyOfPropertyChange(() => Players); }
        }

        public int num_players
        {
            get { return Server.num_players; }
            // set { num_players = value; NotifyOfPropertyChange(() => num_players); }
        }

        public int public_id
        {
            get { return Server.public_id; }
            // set { public_id = value; NotifyOfPropertyChange(() => public_id); }
        }
        public int ECODE
        {
            get { return Server.ECODE; }
            // set { ECODE = value; NotifyOfPropertyChange(() => ECODE); }
        }
        public int teamsize
        {
            get { return Server.teamsize; }
            // set { teamsize = value; NotifyOfPropertyChange(() => teamsize); }
        }
        public string g_customSettings
        {
            get { return Server.g_customSettings; }
            // set { g_customSettings = value; NotifyOfPropertyChange(() => g_customSettings); }
        }
        public int g_levelstarttime
        {
            get { return Server.g_levelstarttime; }
            // set { g_levelstarttime = value; NotifyOfPropertyChange(() => g_levelstarttime); }
        }
        public int location_id
        {
            get { return Server.location_id; }
            // set { location_id = value; NotifyOfPropertyChange(() => location_id); }
        }

        public int max_clients
        {
            get { return Server.max_clients; }
            // set { max_clients = value; NotifyOfPropertyChange(() => max_clients); }
        }
        public int roundtimelimit
        {
            get { return Server.roundtimelimit; }
            // set { roundtimelimit = value; NotifyOfPropertyChange(() => roundtimelimit); }
        }
        public string map_title
        {
            get { return Server.map_title; }
            // set { map_title = value; NotifyOfPropertyChange(() => map_title); }
        }
        public string scorelimit
        {
            get { return Server.scorelimit; }
            // set { scorelimit = value; NotifyOfPropertyChange(() => scorelimit); }
        }
        public string ruleset
        {
            get { return Server.ruleset; }
            // set { ruleset = value; NotifyOfPropertyChange(() => ruleset); }
        }
        public int skillDelta
        {
            get { return Server.skillDelta; }
            // set { skillDelta = value; NotifyOfPropertyChange(() => skillDelta); }
        }
        public string game_type_title
        {
            get { return Server.game_type_title; }
            // set { game_type_title = value; NotifyOfPropertyChange(() => game_type_title); }
        }
        public string map
        {
            get { return Server.map; }
            // set { map = value; NotifyOfPropertyChange(() => map); }
        }
        public object premium
        {
            get { return Server.premium; }
            // set { premium = value; NotifyOfPropertyChange(() => premium); }
        }
        public int g_needpass
        {
            get { return Server.g_needpass; }
            // set { g_needpass = value; NotifyOfPropertyChange(() => g_needpass); }
        }
        public int ranked
        {
            get { return Server.ranked; }
            // set { ranked = value; NotifyOfPropertyChange(() => ranked); }
        }
        public int g_instagib
        {
            get { return Server.g_instagib; }
            // set { g_instagib = value; NotifyOfPropertyChange(() => g_instagib); }
        }
        public int g_bluescore
        {
            get { return Server.g_bluescore; }
            // set { g_bluescore = value; NotifyOfPropertyChange(() => g_bluescore); }
        }
        public string g_gamestate
        {
            get { return Server.g_gamestate; }
            // set { g_gamestate = value; NotifyOfPropertyChange(() => g_gamestate); }
        }
        public string host_address
        {
            get { return Server.host_address; }
            // set { host_address = value; NotifyOfPropertyChange(() => host_address); }
        }
        public int fraglimit
        {
            get { return Server.fraglimit; }
            // set { fraglimit = value; NotifyOfPropertyChange(() => fraglimit); }
        }
        public int num_clients
        {
            get { return Server.num_clients; }
            // set { num_clients = value; NotifyOfPropertyChange(() => num_clients); }
        }
        public int capturelimit
        {
            get { return Server.capturelimit; }
            // set { capturelimit = value; NotifyOfPropertyChange(() => capturelimit); }
        }
        public int game_type
        {
            get { return Server.game_type; }
            // set { game_type = value; NotifyOfPropertyChange(() => game_type); }
        }
        public int timelimit
        {
            get { return Server.timelimit; }
            // set { timelimit = value; NotifyOfPropertyChange(() => timelimit); }
        }
        public int roundlimit
        {
            get { return Server.roundlimit; }
            // set { roundlimit = value; NotifyOfPropertyChange(() => roundlimit); }
        }
        public string host_name
        {
            get { return Server.host_name; }
            // set { host_name = value; NotifyOfPropertyChange(() => host_name); }
        }
        public int g_redscore
        {
            get { return Server.g_redscore; }
            // set { g_redscore = value; NotifyOfPropertyChange(() => g_redscore); }
        }
        public string owner
        {
            get { return Server.owner; }
            // set { owner = value; NotifyOfPropertyChange(() => owner); }
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
        private string _location_name;
        public string location_name
        {
            get
            {
                switch (location_id)
                {
                    case 6:
                        _location_name = "USA, Dallas";
                        break;
                    case 10:
                        _location_name = "USA, Palo Alto";
                        break;
                    case 11:
                        _location_name = "USA, Ashburn";
                        break;
                    case 12:
                        _location_name = "USA, Richardson, TX (LAN)";
                        break;
                    case 14:
                        _location_name = "AUS, Sydney";
                        break;
                    case 16:
                        _location_name = "USA, San Francisco";
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
                        _location_name = "USA, Chicago";
                        break;
                    case 22:
                        _location_name = "USA, Atlanta";
                        break;
                    case 23:
                        _location_name = "USA, Seattle";
                        break;
                    case 24:
                        _location_name = "USA, New York";
                        break;
                    case 25:
                        _location_name = "USA, Los Angeles";
                        break;
                    case 26:
                        _location_name = "CAN, Toronto";
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

    }

}
