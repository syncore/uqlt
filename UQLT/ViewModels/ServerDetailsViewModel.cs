using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using UQLT.Models;
using UQLT.Models.QuakeLiveAPI;

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

        private static Regex port = new Regex(@"[\:]\d{4,}"); // port regexp: colon with at least 4 numbers

        [ImportingConstructor]
        public ServerDetailsViewModel(Server server)
        {
            Server = server;
            _formattedPlayerList = new ObservableCollection<PlayerDetailsViewModel>();
            _formattedPlayerList = AddFormattedPlayers(server.players);
            GroupScoresAndPlayers("Score", "Team");
        }

        private ObservableCollection<PlayerDetailsViewModel> _formattedPlayerList;

        public ObservableCollection<PlayerDetailsViewModel> FormattedPlayerList
        {
            get
            { 
                return _formattedPlayerList;
            }

            set
            {
                _formattedPlayerList = value;
                NotifyOfPropertyChange(() => FormattedPlayerList);
            }
        }

        public List<Player> Players
        {
            get
            { 
                return Server.players;
            }
        }

        public int NumPlayers
        {
            get
            { 
                return Server.num_players;
            }
        }

        public int PublicId
        {
            get
            { 
                return Server.public_id;
            }
        }

        public int ECODE
        {
            get
            { 
                return Server.ECODE;
            }
        }

        public int TeamSize
        {
            get
            { 
                return Server.teamsize;
            }
        }

        public string GCustomSettings
        {
            get
            { 
                return Server.g_customSettings;
            }
        }

        public int GLevelStartTime
        {
            get
            { 
                return Server.g_levelstarttime;
            }
        }

        public int LocationId
        {
            get
            { 
                return Server.location_id;
            }
        }

        public int MaxClients
        {
            get
            {
                return Server.max_clients;
            }
        }

        public int RoundTimeLimit
        {
            get
            {
                return Server.roundtimelimit;
            }
        }

        public string MapTitle
        {
            get
            { 
                return Server.map_title;
            }
        }

        public string ScoreLimit
        {
            get
            {
                return Server.scorelimit;
            }
        }

        public string RuleSet
        {
            get
            {
                return Server.ruleset;
            }
        }

        public int SkillDelta
        {
            get
            { 
                return Server.skillDelta;
            }
        }

        public string GameTypeTitle
        {
            get
            { 
                return Server.game_type_title;
            }
        }

        public string Map
        {
            get
            {
                return Server.map;
            }
        }

        public object Premium
        {
            get
            { 
                return Server.premium;
            }
        }

        public int GNeedPass
        {
            get
            {
                return Server.g_needpass;
            }
        }

        public int Ranked
        {
            get
            {
                return Server.ranked;
            }
        }

        public int GInstagib
        {
            get
            {
                return Server.g_instagib;
            }            
        }

        public int GBlueScore
        {
            get
            {
                return Server.g_bluescore;
            }
        }

        public string GGameState
        {
            get
            {
                return Server.g_gamestate;
            }
        }

        public string HostAddress
        {
            get
            {
                return Server.host_address;
            }
        }

        public int FragLimit
        {
            get
            {
                return Server.fraglimit;
            }
        }

        public int NumClients
        {
            get
            {
                return Server.num_clients;
            }
        }

        public int CaptureLimit
        {
            get
            {
                return Server.capturelimit;
            }
        }

        public int GameType
        {
            get 
            {
                return Server.game_type;
            }
        }

        public int TimeLimit
        {
            get
            {
                return Server.timelimit;
            }
        }

        public int RoundLimit
        {
            get
            {
                return Server.roundlimit;
            }
        }

        public string HostName
        {
            get
            {
                return Server.host_name;
            }
        }

        public int GRedScore
        {
            get
            {
                return Server.g_redscore;
            }
        }

        public string Owner
        {
            get 
            {
                return Server.owner;
            }
        }

        // specially formatted properties for view:
        public long Ping
        {
            get
            {
                string cleanedip = port.Replace(HostAddress, string.Empty);
                return UQLTGlobals.IPAddressDict[cleanedip];
            }
        }

        public ImageSource FlagImage
        {
            get
            {
                try
                {
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/flags/" + LocationId.ToString() + ".gif", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/flags/unknown_flag.gif", UriKind.RelativeOrAbsolute));
                }
            }
        }

        public ImageSource GameTypeImage
        {
            get
            {
                try
                {
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/" + GameType.ToString() + ".gif", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/unknown_game_type.gif", UriKind.RelativeOrAbsolute));
                }
            }
        }

        public ImageSource MapImage
        {
            get
            {
                try
                {
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/maps/" + Map.ToString() + ".jpg", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/maps/unknown_map.jpg", UriKind.RelativeOrAbsolute));
                }
            }
        }

        public string TotalPlayers
        {
            get
            { 
                return string.Empty + NumPlayers + "/" + MaxClients;
            }
        }

        public string Modded
        {
            get
            { 
                return GCustomSettings.Equals("0") ? "No" : "Yes";
            }
        }

        public string Instagib
        {
            get
            { 
                return GInstagib == 0 ? "No" : "Yes";
            }
        }

        // QL does not include the physical location in the server detauls API, so this monstrosity is used for the info pane and for sorting the listview header by location
        private string _locationName;

        public string LocationName
        {
            get
            {
                switch (LocationId)
                {
                    case 6:
                        _locationName = "USA, Dallas";
                        break;
                    case 10:
                        _locationName = "USA, Palo Alto";
                        break;
                    case 11:
                        _locationName = "USA, Ashburn";
                        break;
                    case 12:
                        _locationName = "USA, Richardson, TX (LAN)";
                        break;
                    case 14:
                        _locationName = "AUS, Sydney";
                        break;
                    case 16:
                        _locationName = "USA, San Francisco";
                        break;
                    case 17:
                        _locationName = "NLD, Amsterdam";
                        break;
                    case 18:
                        _locationName = "DEU, Frankfurt";
                        break;
                    case 19:
                        _locationName = "GBR, Maidenhead";
                        break;
                    case 20:
                        _locationName = "FRA, Paris";
                        break;
                    case 21:
                        _locationName = "USA, Chicago";
                        break;
                    case 22:
                        _locationName = "USA, Atlanta";
                        break;
                    case 23:
                        _locationName = "USA, Seattle";
                        break;
                    case 24:
                        _locationName = "USA, New York";
                        break;
                    case 25:
                        _locationName = "USA, Los Angeles";
                        break;
                    case 26:
                        _locationName = "CAN, Toronto";
                        break;
                    case 27:
                        _locationName = "JPN, Tokyo";
                        break;
                    case 28:
                        _locationName = "ESP, Madrid";
                        break;
                    case 29:
                        _locationName = "SWE, Stockholm";
                        break;
                    case 30:
                        _locationName = "POL, Warsaw";
                        break;
                    case 31:
                        _locationName = "CHN, Hangzhou";
                        break;
                    case 32:
                        _locationName = "POL, Warsaw";
                        break;
                    case 33:
                        _locationName = "AUS, Sydney";
                        break;
                    case 34:
                        _locationName = "SWE, Malmo";
                        break;
                    case 35:
                        _locationName = "AUS, Perth";
                        break;
                    case 36:
                        _locationName = "SWE, Stockholm";
                        break;
                    case 37:
                        _locationName = "ROM, Bucharest";
                        break;
                    case 38:
                        _locationName = "CHL, Santiago";
                        break;
                    case 39:
                        _locationName = "ROM, Bucharest";
                        break;
                    case 40:
                        _locationName = "ARG, Buenos Aires";
                        break;
                    case 41:
                        _locationName = "ISL, Keflavik";
                        break;
                    case 42:
                        _locationName = "JPN, Tokyo";
                        break;
                    case 43:
                        _locationName = "RUS, Moscow";
                        break;
                    case 44:
                        _locationName = "RUS, Moscow";
                        break;
                    case 45:
                        _locationName = "SGP, Singapore";
                        break;
                    case 46:
                        _locationName = "ZAF, Johannesburg";
                        break;
                    case 47:
                        _locationName = "SRB, Beograd";
                        break;
                    case 48:
                        _locationName = "BGR, Sofia";
                        break;
                    case 49:
                        _locationName = "KOR, Seoul";
                        break;
                    case 50:
                        _locationName = "ITA, Milan";
                        break;
                    case 51:
                        _locationName = "AUS, Adelaide";
                        break;
                    case 52:
                        _locationName = "DEU, Cologne (LAN)";
                        break;
                    case 53:
                        _locationName = "USA, Dallas,TX (LAN)";
                        break;
                    case 54:
                        _locationName = "SWE, Jonkoping (LAN)";
                        break;
                    case 58:
                        _locationName = "UKR, Kiev";
                        break;
                    case 59:
                        _locationName = "ITA, Lignano Sabbiadoro (LAN)";
                        break;
                    case 60:
                        _locationName = "AUS, Adelaide (LAN)";
                        break;
                    case 61:
                        _locationName = "NLD, Benelux (LAN)";
                        break;
                    case 62:
                        _locationName = "USA, Washington DC";
                        break;
                    case 666:
                        _locationName = "USA, QuakeCon LAN";
                        break;
                    case 63:
                        _locationName = "USA, Indianapolis, IN";
                        break;
                    case 64:
                        _locationName = "NLD, Rotterdam";
                        break;
                    case 65:
                        _locationName = "NOR, Oslo";
                        break;
                    case 66:
                        _locationName = "BRA, Sao Paulo";
                        break;
                    case 67:
                        _locationName = "TUR, Istanbul";
                        break;
                    case 68:
                        _locationName = "NZL, Auckland";
                        break;
                    default:
                        _locationName = "Unknown";
                        break;
                }

                return _locationName;
            }
        }

        public bool IsTeamGame
        {
            get
            {
                return (GameType == 3 || GameType == 4 || GameType == 5 || GameType == 6 || GameType == 8 || GameType == 9 || GameType == 10 || GameType == 11) ? true : false;
            }
        }
        
        private string _formattedGameState;

        public string FormattedGameState
        {
            get
            {
                if (GGameState.Equals("IN_PROGRESS"))
                {
                    _formattedGameState = "In Progress";
                }
                else if (GGameState.Equals("PRE_GAME"))
                {
                    _formattedGameState = "Pre-Game Warmup";
                }

                return _formattedGameState;
            }
        }

        private string _timeRemaining;

        public string TimeRemaining
        {
            get
            {
                if (GGameState.Equals("IN_PROGRESS"))
                {
                    var now = (long)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
                    var secsLeft = (long)(TimeLimit * 60) - (now - GLevelStartTime);
                    if (secsLeft > 0)
                    {
                        var minsLeft = (long)(secsLeft / 60);
                        secsLeft -= minsLeft * 60;
                        _timeRemaining = string.Format("{0}:{1}", minsLeft.ToString(), secsLeft.ToString("D2"));
                    }
                    else
                    {
                        _timeRemaining = "None";
                    }
                }
                else
                {
                    _timeRemaining = "None";
                }

                return _timeRemaining;
            }

            set
            {
                _timeRemaining = value;
                NotifyOfPropertyChange(() => TimeRemaining);
            }
        }

        // This is a weird situation that occurs on some team servers, where the players are on the server,
        // yet they are not reported as being on red (team: 1) or blue (team: 2) but instead team 0
        public bool IsTeam0Condition
        {
            get
            {
                int redsize = 0, bluesize = 0, zerosize = 0;
                foreach (var p in Players)
                {
                    if (p.team == 0)
                    {
                        zerosize++;
                    }
                    else if (p.team == 1)
                    {
                        redsize++;
                    }
                    else if (p.team == 2)
                    {
                        bluesize++;
                    }
                }

                if (((redsize == 0) && (bluesize == 0)) && (zerosize > 0))
                { 
                    return true;
                }

                return false;
            }
        }

        private int _redTeamElo;

        public int RedTeamElo
        {
            get
            {
                int redtotalplayers = 0, totaleloredteam = 0, redplayerelo = 0;

                if (NumPlayers == 0 || IsTeam0Condition)
                {
                    return 0;
                }
                else
                {
                    if (GameType == 3 || GameType == 4 || GameType == 5)
                    {
                        foreach (var p in Players)
                        {
                            if (p.team == 1)
                            {
                                if (GameType == 3)
                                {
                                    redplayerelo = UQLTGlobals.PlayerEloTdm[p.name.ToLower()];
                                }
                                else if (GameType == 4)
                                {
                                    redplayerelo = UQLTGlobals.PlayerEloCa[p.name.ToLower()];
                                }
                                else if (GameType == 5)
                                {
                                    redplayerelo = UQLTGlobals.PlayerEloCtf[p.name.ToLower()];
                                }

                                redtotalplayers++;
                                totaleloredteam += redplayerelo;
                            }
                        }

                        if (redtotalplayers == 0)
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return 0;
                    }

                    _redTeamElo = totaleloredteam / redtotalplayers;
                    return _redTeamElo;
                }
            }

            set
            {
                _redTeamElo = value;
                NotifyOfPropertyChange(() => RedTeamElo);
            }
        }

        private int _blueTeamElo;

        public int BlueTeamElo
        {
            get
            {
                int bluetotalplayers = 0, totaleloblueteam = 0, blueplayerelo = 0;

                if (NumPlayers == 0 || IsTeam0Condition)
                {
                    return 0;
                }
                else
                {
                    if (GameType == 3 || GameType == 4 || GameType == 5)
                    {
                        foreach (var p in Players)
                        {
                            if (p.team == 2)
                            {
                                if (GameType == 3)
                                {
                                    blueplayerelo = UQLTGlobals.PlayerEloTdm[p.name.ToLower()];
                                }
                                else if (GameType == 4)
                                {
                                    blueplayerelo = UQLTGlobals.PlayerEloCa[p.name.ToLower()];
                                }
                                else if (GameType == 5)
                                {
                                    blueplayerelo = UQLTGlobals.PlayerEloCtf[p.name.ToLower()];
                                }

                                bluetotalplayers++;
                                totaleloblueteam += blueplayerelo;
                            }
                        }

                        if (bluetotalplayers == 0)
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return 0;
                    }

                    _blueTeamElo = totaleloblueteam / bluetotalplayers;
                    return _blueTeamElo;
                }
            }

            set
            {
                _blueTeamElo = value;
                NotifyOfPropertyChange(() => BlueTeamElo);
            }
        }

        private void GroupScoresAndPlayers(string sortBy, string groupBy)
        {
            var view = CollectionViewSource.GetDefaultView(FormattedPlayerList);
            var sortDescription = new SortDescription(sortBy, ListSortDirection.Descending);
            var groupDescription = new PropertyGroupDescription(groupBy);
            view.SortDescriptions.Add(sortDescription);
            view.GroupDescriptions.Add(groupDescription);
        }

        private ObservableCollection<PlayerDetailsViewModel> AddFormattedPlayers(List<Player> players)
        {
            _formattedPlayerList.Clear();
            foreach (var player in players)
            {
                _formattedPlayerList.Add(new PlayerDetailsViewModel(player));
            }

            return _formattedPlayerList;
        }
    }
}
