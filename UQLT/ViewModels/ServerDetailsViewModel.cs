using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using UQLT.Helpers;
using UQLT.Interfaces;
using UQLT.Models;
using UQLT.Models.QLRanks;
using UQLT.Models.QuakeLiveAPI;

namespace UQLT.ViewModels
{
    [Export(typeof(ServerDetailsViewModel))]

    // Individual Server viewmodel wrapper; no associated View

    public class ServerDetailsViewModel : PropertyChangedBase
    {
        public Server Server
        {
            get;
            private set;
        }

        private QLFormatHelper FormatHelper = QLFormatHelper.Instance;

        private Regex port = new Regex(@"[\:]\d{4,}"); // port regexp: colon with at least 4 numbers

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
            set
            {
                Server.num_players = value;
                NotifyOfPropertyChange(() => NumPlayers);
                NotifyOfPropertyChange(() => TotalPlayers);
            }
        }

        public int PublicId
        {
            get
            {
                return Server.public_id;
            }
            set
            {
                Server.public_id = value;
                NotifyOfPropertyChange(() => PublicId);
            }
        }

        public int ECODE
        {
            get
            {
                return Server.ECODE;
            }
            set
            {
                Server.ECODE = value;
                NotifyOfPropertyChange(() => ECODE);
            }
        }

        public int TeamSize
        {
            get
            {
                return Server.teamsize;
            }
            set
            {
                Server.teamsize = value;
                NotifyOfPropertyChange(() => TeamSize);
            }
        }

        public string GCustomSettings
        {
            get
            {
                return Server.g_customSettings;
            }
            set
            {
                Server.g_customSettings = value;
                NotifyOfPropertyChange(() => GCustomSettings);
            }
        }

        public int GLevelStartTime
        {
            get
            {
                return Server.g_levelstarttime;
            }
            set
            {
                Server.g_levelstarttime = value;
                NotifyOfPropertyChange(() => GLevelStartTime);
            }
        }

        public long LocationId
        {
            get
            {
                return Server.location_id;
            }
            set
            {
                Server.location_id = value;
                NotifyOfPropertyChange(() => LocationId);
                // These are read-only so need to be notified of location id changes:
                NotifyOfPropertyChange(() => FlagImage);
                NotifyOfPropertyChange(() => FullLocationName);
                NotifyOfPropertyChange(() => ShortLocationName);
            }
        }

        public int MaxClients
        {
            get
            {
                return Server.max_clients;
            }
            set
            {
               Server.max_clients = value;
                NotifyOfPropertyChange(() => MaxClients);
                NotifyOfPropertyChange(() => TotalPlayers);
            }
        }

        public int RoundTimeLimit
        {
            get
            {
                return Server.roundtimelimit;
            }
            set
            {
               Server.roundtimelimit = value;
                NotifyOfPropertyChange(() => RoundLimit);
            }
        }

        public string MapTitle
        {
            get
            {
                return Server.map_title;
            }
            set
            {
               Server.map_title = value;
                NotifyOfPropertyChange(() => MapTitle);
            }
        }

        public string ScoreLimit
        {
            get
            {
                return Server.scorelimit;
            }
            set
            {
               Server.scorelimit = value;
                NotifyOfPropertyChange(() => ScoreLimit);
            }
        }

        public string RuleSet
        {
            get
            {
                return Server.ruleset;
            }
            set
            {
               Server.ruleset = value;
                NotifyOfPropertyChange(() => RuleSet);
            }
        }

        public int SkillDelta
        {
            get
            {
                return Server.skillDelta;
            }
            set
            {
               Server.skillDelta = value;
                NotifyOfPropertyChange(() => SkillDelta);
            }
        }

        public string GameTypeTitle
        {
            get
            {
                return Server.game_type_title;
            }
            set
            {
                Server.game_type_title = value;
                NotifyOfPropertyChange(() => GameTypeTitle);
            }
        }

        public string Map
        {
            get
            {
                return Server.map;
            }
            set
            {
                Server.map = value;
                NotifyOfPropertyChange(() => Map);
            }
        }

        public object Premium
        {
            get
            {
                return Server.premium;
            }
            set
            {
                Server.premium = value;
                NotifyOfPropertyChange(() => Premium);
            }
        }

        public int GNeedPass
        {
            get
            {
                return Server.g_needpass;
            }
            set
            {
                Server.g_needpass = value;
                NotifyOfPropertyChange(() => GNeedPass);
            }
        }

        public int Ranked
        {
            get
            {
                return Server.ranked;
            }
            set
            {
                Server.ranked = value;
                NotifyOfPropertyChange(() => Ranked);
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
            set
            {
                Server.g_bluescore = value;
                NotifyOfPropertyChange(() => GBlueScore);
            }
        }

        public string GGameState
        {
            get
            {
                return Server.g_gamestate;
            }
            set
            {
                Server.g_gamestate = value;
                NotifyOfPropertyChange(() => GGameState);
            }
        }

        public string HostAddress
        {
            get
            {
                return Server.host_address;
            }
            set
            {
                Server.host_address = value;
                NotifyOfPropertyChange(() => HostAddress);
            }
        }

        public int FragLimit
        {
            get
            {
                return Server.fraglimit;
            }
            set
            {
                Server.fraglimit = value;
                NotifyOfPropertyChange(() => FragLimit);
            }
        }

        public int NumClients
        {
            get
            {
                return Server.num_clients;
            }
            set
            {
                Server.num_clients = value;
                NotifyOfPropertyChange(() => NumClients);
            }
        }

        public int CaptureLimit
        {
            get
            {
                return Server.capturelimit;
            }
            set
            {
                Server.capturelimit = value;
                NotifyOfPropertyChange(() => CaptureLimit);
            }
        }

        public int GameType
        {
            get
            {
                return Server.game_type;
            }
            set
            {
                Server.game_type = value;
                NotifyOfPropertyChange(() => GameType);
                NotifyOfPropertyChange(() => ShortGameTypeName);
            }
        }

        public int TimeLimit
        {
            get
            {
                return Server.timelimit;
            }
            set
            {
                Server.timelimit = value;
                NotifyOfPropertyChange(() => TimeLimit);
            }
        }

        public int RoundLimit
        {
            get
            {
                return Server.roundlimit;
            }
            set
            {
                Server.roundlimit = value;
                NotifyOfPropertyChange(() => RoundLimit);
            }
        }

        public string HostName
        {
            get
            {
                return Server.host_name;
            }
            set
            {
                Server.host_name = value;
                NotifyOfPropertyChange(() => HostName);
            }
        }

        public int GRedScore
        {
            get
            {
                return Server.g_redscore;
            }
            set
            {
                Server.g_redscore = value;
                NotifyOfPropertyChange(() => GRedScore);
            }
        }

        public string Owner
        {
            get
            {
                return Server.owner;
            }
            set
            {
                Server.owner = value;
                NotifyOfPropertyChange(() => Owner);
            }
        }

        // specially formatted properties for view:
        //private string _shortGameTypeName;
        public string ShortGameTypeName
        {
            get
            {
                //return _shortGameTypeName;
                return FormatHelper.Gametypes[GameType].ShortGametypeName;
            }
            /*set
            {
                _shortGameTypeName = value;
                NotifyOfPropertyChange(() => ShortGameTypeName);
            }*/
        }
        
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
                    Debug.WriteLine("Error: " + ex);
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
                    Debug.WriteLine("Error: " + ex);
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
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/maps/"+Map+".jpg", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex);
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

        // QL does not include the physical location in the server details API (this is used for the info pane and for sorting the listview header by location)
        public string FullLocationName
        {
            get
            {
                LocationData value = null;
                if (FormatHelper.Locations.TryGetValue(LocationId, out value))
                {
                    return value.FullLocationName;
                }
                else
                {
                    return "Unknown";
                }
            }
        }

        // QL does not include the physical location in the server details API. ShortLocationName only displays the server's city, not country information.
        public string ShortLocationName
        {
            get
            {
                LocationData value = null;
                if (FormatHelper.Locations.TryGetValue(LocationId, out value))
                {
                    return value.City;
                }
                else
                {
                    return "Unknown";
                }
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
               return _formattedGameState;
            }
            set
            {
                if (GGameState.Equals("IN_PROGRESS"))
                {
                    _formattedGameState = "In Progress";
                }
                else if (GGameState.Equals("PRE_GAME"))
                {
                    _formattedGameState = "Pre-Game Warmup";
                }
                NotifyOfPropertyChange(() => FormattedGameState);
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

        private long _redTeamElo;

        public long RedTeamElo
        {
            get
            {
                long redtotalplayers = 0, totaleloredteam = 0, redplayerelo = 0;
                EloData val = null;

                if (NumPlayers == 0 || IsTeam0Condition)
                {
                    return 0;
                }
                else if (NumPlayers != 0 || !IsTeam0Condition)
                {
                    if (GameType == 3 || GameType == 4 || GameType == 5)
                    {
                        foreach (var p in Players)
                        {
                            if (p.team == 1)
                            {
                                if (GameType == 3)
                                {

                                    if (UQLTGlobals.PlayerEloInfo.TryGetValue(p.name.ToLower(), out val))
                                    {
                                        redplayerelo = UQLTGlobals.PlayerEloInfo[p.name.ToLower()].TdmElo;
                                    }
                                    else
                                    {
                                        redplayerelo = 0;
                                        Debug.WriteLine("Key doesn't exist - error retrieving [RED] player Elo for [TDM] RED team Elo calculation. {0}", val);
                                    }
                                }
                                else if (GameType == 4)
                                {
                                    if (UQLTGlobals.PlayerEloInfo.TryGetValue(p.name.ToLower(), out val))
                                    {
                                        redplayerelo = UQLTGlobals.PlayerEloInfo[p.name.ToLower()].CaElo;
                                    }
                                    else
                                    {
                                        redplayerelo = 0;
                                        Debug.WriteLine("Key doesn't exist - error retrieving [RED] player Elo for [CA] RED team Elo calculation. {0}", val);
                                    }
                                }
                                else if (GameType == 5)
                                {
                                    if (UQLTGlobals.PlayerEloInfo.TryGetValue(p.name.ToLower(), out val))
                                    {
                                        redplayerelo = UQLTGlobals.PlayerEloInfo[p.name.ToLower()].CtfElo;
                                    }
                                    else
                                    {
                                        redplayerelo = 0;
                                        Debug.WriteLine("Key doesn't exist - error retrieving [RED] player Elo for [CTF] RED team Elo calculation. {0}", val);
                                    }
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
                else
                {
                    return 0;
                }
            }

            set
            {
                _redTeamElo = value;
                NotifyOfPropertyChange(() => RedTeamElo);
            }
        }

        private long _blueTeamElo;

        public long BlueTeamElo
        {
            get
            {
                long bluetotalplayers = 0, totaleloblueteam = 0, blueplayerelo = 0;
                EloData val = null;
                
                if (NumPlayers == 0 || IsTeam0Condition)
                {
                    return 0;
                }
                else if (NumPlayers != 0 || !IsTeam0Condition)
                {
                    if (GameType == 3 || GameType == 4 || GameType == 5)
                    {
                        foreach (var p in Players)
                        {
                            if (p.team == 2)
                            {
                                if (GameType == 3)
                                {

                                    if (UQLTGlobals.PlayerEloInfo.TryGetValue(p.name.ToLower(), out val))
                                    {
                                        blueplayerelo = UQLTGlobals.PlayerEloInfo[p.name.ToLower()].TdmElo;
                                    }
                                    else
                                    {
                                        blueplayerelo = 0;
                                        Debug.WriteLine("Key doesn't exist - error retrieving [BLUE] player Elo for [TDM] BLUE team Elo calculation. {0}", val);
                                    }
                                }
                                else if (GameType == 4)
                                {
                                    if (UQLTGlobals.PlayerEloInfo.TryGetValue(p.name.ToLower(), out val))
                                    {
                                        blueplayerelo = UQLTGlobals.PlayerEloInfo[p.name.ToLower()].CaElo;
                                    }
                                    else
                                    {
                                        blueplayerelo = 0;
                                        Debug.WriteLine("Key doesn't exist - error retrieving [BLUE] player Elo for [CA] BLUE team Elo calculation. {0}", val);
                                    }
                                }
                                else if (GameType == 5)
                                {
                                    if (UQLTGlobals.PlayerEloInfo.TryGetValue(p.name.ToLower(), out val))
                                    {
                                        blueplayerelo = UQLTGlobals.PlayerEloInfo[p.name.ToLower()].CtfElo;
                                    }
                                    else
                                    {
                                        blueplayerelo = 0;
                                        Debug.WriteLine("Key doesn't exist - error retrieving [BLUE] player Elo for [CTF] BLUE team Elo calculation. {0}", val);
                                    }
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
                else
                {
                    return 0;
                }
            }

            set
            {
                _blueTeamElo = value;
                NotifyOfPropertyChange(() => BlueTeamElo);
            }
        }

        /*private bool AllTeamMembersHaveElo(List<Player> players) {
            
            players = Players;
            EloData val;
            List<string> PlayersNeedingUpdate = new List<string>();
            foreach (var player in players)
            {
                if (!UQLTGlobals.PlayerEloInfo.TryGetValue(player.name.ToLower(), out val))
                {
                    PlayersNeedingUpdate.Add(player.name.ToLower());
                }
            }
            if (PlayersNeedingUpdate.Count == 0)
            {
                return true;
            }
            else
            {
                // send to update
            }
        }
        */
        

        
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
