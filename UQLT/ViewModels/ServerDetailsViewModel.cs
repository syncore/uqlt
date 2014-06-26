using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using UQLT.Helpers;
using UQLT.Models.QLRanks;
using UQLT.Models.QuakeLiveAPI;

namespace UQLT.ViewModels
{
    /// <summary>
    /// Individual server viewmodel. This class wraps a Server class and exposes additional
    /// properties specific to the View (in this case, ServerBrowserView).
    /// </summary>
    [Export(typeof(ServerDetailsViewModel))]
    public class ServerDetailsViewModel : PropertyChangedBase
    {
        private readonly QlFormatHelper _formatHelper = QlFormatHelper.Instance;
        private readonly QlRanksDataRetriever _qlRanksDataRetriever = new QlRanksDataRetriever();
        private string _formattedGameState;
        private ObservableCollection<PlayerDetailsViewModel> _formattedPlayerList = new ObservableCollection<PlayerDetailsViewModel>();
        private string _timeRemaining;

        // port regexp: colon with at least 4 numbers
        private Regex port = new Regex(@"[\:]\d{4,}");
        private NotifyTaskCompletion<long> _blueTeamElo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDetailsViewModel" /> class.
        /// </summary>
        /// <param name="server">The server wrapped by this viewmodel.</param>
        /// <param name="isForServerBrowser">
        /// If <c>true</c> then this class is being instantiated for use with server browser, so
        /// perform additional operations such as sorting the servers and format the player list. If
        /// <c>false</c> then this class is being instantiated elsewhere and these operations do not
        /// need to be performed. Defaults to <c>true</c>
        /// </param>
        [ImportingConstructor]
        public ServerDetailsViewModel(Server server, bool isForServerBrowser = true)
        {
            QlServer = server;
            QlRanksPlayersToUpdate = new HashSet<string>();
            //RedTeamElo = new NotifyTaskCompletion<long>(CalculateTeamEloAsync(1));
            //BlueTeamElo = new NotifyTaskCompletion<long>(CalculateTeamEloAsync(2));
            if (isForServerBrowser)
            {
                SortServersAndFormatPlayers();
            }
        }

        /// <summary>
        /// Gets or sets the blue team's Elo.
        /// </summary>
        /// <value>The blue team's Elo.</value>
        public NotifyTaskCompletion<long> BlueTeamElo
        {
            get { return new NotifyTaskCompletion<long>(CalculateTeamEloAsync(2)); }
        }

        /// <summary>
        /// Gets or sets the capture limit.
        /// </summary>
        /// <value>The capture limit.</value>
        public int CaptureLimit
        {
            get
            {
                return QlServer.capturelimit;
            }
            set
            {
                QlServer.capturelimit = value;
                NotifyOfPropertyChange(() => CaptureLimit);
            }
        }

        /// <summary>
        /// Gets or sets the ecode.
        /// </summary>
        /// <value>The ecode.</value>
        public int ECODE
        {
            get
            {
                return QlServer.ECODE;
            }
            set
            {
                QlServer.ECODE = value;
                NotifyOfPropertyChange(() => ECODE);
            }
        }

        /// <summary>
        /// Gets the flag image.
        /// </summary>
        /// <value>The flag image.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public ImageSource FlagImage
        {
            get
            {
                try
                {
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/flags/" + LocationId + ".gif", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex);
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/flags/unknown_flag.gif", UriKind.RelativeOrAbsolute));
                }
            }
        }

        /// <summary>
        /// Gets or sets the state of the formatted game.
        /// </summary>
        /// <value>The state of the formatted game.</value>
        /// <remarks>This is a custom UI setting.</remarks>
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

        /// <summary>
        /// Gets or sets the formatted player list.
        /// </summary>
        /// <value>The formatted player list.</value>
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

        /// <summary>
        /// Gets or sets the frag limit.
        /// </summary>
        /// <value>The frag limit.</value>
        public int FragLimit
        {
            get
            {
                return QlServer.fraglimit;
            }
            set
            {
                QlServer.fraglimit = value;
                NotifyOfPropertyChange(() => FragLimit);
            }
        }

        /// <summary>
        /// Gets the full name of the location, since Quake Live does not include the physical
        /// location details in the server details API. This is used for the info pane and for
        /// sorting the listview header by location.
        /// </summary>
        /// <value>The full name of the location.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public string FullLocationName
        {
            get
            {
                LocationData value;
                return _formatHelper.Locations.TryGetValue(LocationId, out value) ? value.FullLocationName : "Unknown";
            }
        }

        /// <summary>
        /// Gets or sets the type of the game.
        /// </summary>
        /// <value>The type of the game.</value>
        public int GameType
        {
            get
            {
                return QlServer.game_type;
            }
            set
            {
                QlServer.game_type = value;
                NotifyOfPropertyChange(() => GameType);
                NotifyOfPropertyChange(() => ShortGameTypeName);
            }
        }

        /// <summary>
        /// Gets the game type image.
        /// </summary>
        /// <value>The game type image.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public ImageSource GameTypeImage
        {
            get
            {
                try
                {
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/" + GameType + ".gif", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex);
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/unknown_game_type.gif", UriKind.RelativeOrAbsolute));
                }
            }
        }

        /// <summary>
        /// Gets or sets the game type title.
        /// </summary>
        /// <value>The game type title.</value>
        public string GameTypeTitle
        {
            get
            {
                return QlServer.game_type_title;
            }
            set
            {
                QlServer.game_type_title = value;
                NotifyOfPropertyChange(() => GameTypeTitle);
            }
        }

        /// <summary>
        /// Gets or sets the blue score.
        /// </summary>
        /// <value>The blue score.</value>
        public int GBlueScore
        {
            get
            {
                return QlServer.g_bluescore;
            }
            set
            {
                QlServer.g_bluescore = value;
                NotifyOfPropertyChange(() => GBlueScore);
            }
        }

        /// <summary>
        /// Gets or sets the custom settings.
        /// </summary>
        /// <value>The custom settings.</value>
        public string GCustomSettings
        {
            get
            {
                return QlServer.g_customSettings;
            }
            set
            {
                QlServer.g_customSettings = value;
                NotifyOfPropertyChange(() => GCustomSettings);
            }
        }

        /// <summary>
        /// Gets or sets the state of the game.
        /// </summary>
        /// <value>The state of the game.</value>
        public string GGameState
        {
            get
            {
                return QlServer.g_gamestate;
            }
            set
            {
                QlServer.g_gamestate = value;
                NotifyOfPropertyChange(() => GGameState);
            }
        }

        /// <summary>
        /// Gets the instagib setting.
        /// </summary>
        /// <value>The instagib setting.</value>
        public int GInstagib
        {
            get
            {
                return QlServer.g_instagib;
            }
        }

        /// <summary>
        /// Gets or sets the level start time.
        /// </summary>
        /// <value>The level start time.</value>
        public int GLevelStartTime
        {
            get
            {
                return QlServer.g_levelstarttime;
            }
            set
            {
                QlServer.g_levelstarttime = value;
                NotifyOfPropertyChange(() => GLevelStartTime);
            }
        }

        /// <summary>
        /// Gets or sets whether this server needs a password.
        /// </summary>
        /// <value>The value representing whether this server needs a password.</value>
        public int GNeedPass
        {
            get
            {
                return QlServer.g_needpass;
            }
            set
            {
                QlServer.g_needpass = value;
                NotifyOfPropertyChange(() => GNeedPass);
            }
        }

        /// <summary>
        /// Gets or sets the red score.
        /// </summary>
        /// <value>The red score.</value>
        public int GRedScore
        {
            get
            {
                return QlServer.g_redscore;
            }
            set
            {
                QlServer.g_redscore = value;
                NotifyOfPropertyChange(() => GRedScore);
            }
        }

        /// <summary>
        /// Gets or sets the host address.
        /// </summary>
        /// <value>The host address.</value>
        public string HostAddress
        {
            get
            {
                return QlServer.host_address;
            }
            set
            {
                QlServer.host_address = value;
                NotifyOfPropertyChange(() => HostAddress);
            }
        }

        /// <summary>
        /// Gets or sets the name of the host.
        /// </summary>
        /// <value>The name of the host.</value>
        public string HostName
        {
            get
            {
                return QlServer.host_name;
            }
            set
            {
                QlServer.host_name = value;
                NotifyOfPropertyChange(() => HostName);
            }
        }

        /// <summary>
        /// Gets the instagib setting.
        /// </summary>
        /// <value>The instagib setting.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public string Instagib
        {
            get
            {
                return GInstagib == 0 ? "No" : "Yes";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this server's gametype is supported by QLRanks for all
        /// game types.
        /// </summary>
        /// <value><c>true</c> if this QLRanks supports this server's gametype otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Currently, game modes 0, 1, 3, 4, 5 are supported by QLRanks API. This is a custom UI setting.
        /// </remarks>
        public bool IsQlRanksSupportedAllGames
        {
            get
            {
                return (GameType == 0 || GameType == 1 || GameType == 3 || GameType == 4 || GameType == 5);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this server's gametype is supported by QLRanks for team
        /// games only.
        /// </summary>
        /// <value><c>true</c> if this QLRanks supports this server's gametype otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Currently, only team game modes 3, 4, 5 are supported by QLRanks API. This is a custom
        /// UI setting.
        /// </remarks>
        public bool IsQlRanksSupportedTeamGame
        {
            get
            {
                return (GameType == 3 || GameType == 4 || GameType == 5);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this server has players who have the mysterious "team 0 condition".
        /// </summary>
        /// <value>
        /// <c>true</c> if this server has players with the team 0 condition; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This is a weird situation that occurs on some team servers, where the players are on the
        /// server, yet they are not reported as being on red (team: 1) or blue (team: 2) but
        /// instead team 0. This is a custom UI setting.
        /// </remarks>
        public bool IsTeam0Condition
        {
            get
            {
                int redsize = 0, bluesize = 0, zerosize = 0;
                foreach (var p in Players)
                {
                    switch (p.team)
                    {
                        case 0:
                            zerosize++;
                            break;

                        case 1:
                            redsize++;
                            break;

                        case 2:
                            bluesize++;
                            break;
                    }
                }

                return ((redsize == 0) && (bluesize == 0)) && (zerosize > 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this server features a team game type.
        /// </summary>
        /// <value><c>true</c> if this server features a team game; otherwise, <c>false</c>.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public bool IsTeamGame
        {
            get
            {
                return (GameType == 3 || GameType == 4 || GameType == 5 || GameType == 6 || GameType == 8 || GameType == 9 || GameType == 10 || GameType == 11);
            }
        }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>The location identifier.</value>
        public long LocationId
        {
            get
            {
                return QlServer.location_id;
            }
            set
            {
                QlServer.location_id = value;
                NotifyOfPropertyChange(() => LocationId);
                // These are read-only so need to be notified of location id changes:
                NotifyOfPropertyChange(() => FlagImage);
                NotifyOfPropertyChange(() => FullLocationName);
                NotifyOfPropertyChange(() => ShortLocationName);
            }
        }

        /// <summary>
        /// Gets or sets the map setting.
        /// </summary>
        /// <value>The map setting.</value>
        public string Map
        {
            get
            {
                return QlServer.map;
            }
            set
            {
                QlServer.map = value;
                NotifyOfPropertyChange(() => Map);
            }
        }

        /// <summary>
        /// Gets the map image.
        /// </summary>
        /// <value>The map image.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public ImageSource MapImage
        {
            get
            {
                try
                {
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/maps/" + Map + ".jpg", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex);
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/maps/unknown_map.jpg", UriKind.RelativeOrAbsolute));
                }
            }
        }

        /// <summary>
        /// Gets or sets the map title.
        /// </summary>
        /// <value>The map title.</value>
        public string MapTitle
        {
            get
            {
                return QlServer.map_title;
            }
            set
            {
                QlServer.map_title = value;
                NotifyOfPropertyChange(() => MapTitle);
            }
        }

        /// <summary>
        /// Gets or sets the maximum clients.
        /// </summary>
        /// <value>The maximum clients.</value>
        public int MaxClients
        {
            get
            {
                return QlServer.max_clients;
            }
            set
            {
                QlServer.max_clients = value;
                NotifyOfPropertyChange(() => MaxClients);
                NotifyOfPropertyChange(() => TotalPlayers);
            }
        }

        /// <summary>
        /// Gets whether or not this server is modded.
        /// </summary>
        /// <value>The modded setting.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public string Modded
        {
            get
            {
                return GCustomSettings.Equals("0") ? "No" : "Yes";
            }
        }

        /// <summary>
        /// Gets or sets the number clients.
        /// </summary>
        /// <value>The number clients.</value>
        public int NumClients
        {
            get
            {
                return QlServer.num_clients;
            }
            set
            {
                QlServer.num_clients = value;
                NotifyOfPropertyChange(() => NumClients);
            }
        }

        /// <summary>
        /// Gets or sets the number players.
        /// </summary>
        /// <value>The number players.</value>
        public int NumPlayers
        {
            get
            {
                return QlServer.num_players;
            }
            set
            {
                QlServer.num_players = value;
                NotifyOfPropertyChange(() => NumPlayers);
                NotifyOfPropertyChange(() => TotalPlayers);
            }
        }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>The owner.</value>
        public string Owner
        {
            get
            {
                return QlServer.owner;
            }
            set
            {
                QlServer.owner = value;
                NotifyOfPropertyChange(() => Owner);
            }
        }

        /// <summary>
        /// Gets the ping.
        /// </summary>
        /// <value>The ping.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public long Ping
        {
            get
            {
                string cleanedip = port.Replace(HostAddress, string.Empty);
                return UQltGlobals.IpAddressDict[cleanedip];
            }
        }

        /// <summary>
        /// Gets the players.
        /// </summary>
        /// <value>The players.</value>
        public List<Player> Players
        {
            get
            {
                return QlServer.players;
            }
        }

        /// <summary>
        /// Gets or sets the premium setting.
        /// </summary>
        /// <value>The premium setting.</value>
        public object Premium
        {
            get
            {
                return QlServer.premium;
            }
            set
            {
                QlServer.premium = value;
                NotifyOfPropertyChange(() => Premium);
            }
        }

        /// <summary>
        /// Gets or sets the public identifier.
        /// </summary>
        /// <value>The public identifier.</value>
        public int PublicId
        {
            get
            {
                return QlServer.public_id;
            }
            set
            {
                QlServer.public_id = value;
                NotifyOfPropertyChange(() => PublicId);
            }
        }

        /// <summary>
        /// Gets or sets the players whose missing QLRanks Elo data is to be updated.
        /// </summary>
        /// <value>The players to update.</value>
        public HashSet<string> QlRanksPlayersToUpdate { get; set; }

        /// <summary>
        /// Gets or sets the QLRanks data retriever.
        /// </summary>
        /// <value>The QLRanksdata retriever.</value>

        /// <summary>
        /// Gets the server that this viewmodel wraps.
        /// </summary>
        /// <value>The server that this viewmodel wraps.</value>
        public Server QlServer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the ranked setting.
        /// </summary>
        /// <value>The ranked setting.</value>
        public int Ranked
        {
            get
            {
                return QlServer.ranked;
            }
            set
            {
                QlServer.ranked = value;
                NotifyOfPropertyChange(() => Ranked);
            }
        }

        /// <summary>
        /// Gets or sets the red team's Elo.
        /// </summary>
        /// <value>The red team's Elo.</value>
        public NotifyTaskCompletion<long> RedTeamElo
        {
            get { return new NotifyTaskCompletion<long>(CalculateTeamEloAsync(1)); }
        }

        /// <summary>
        /// Gets or sets the round limit.
        /// </summary>
        /// <value>The round limit.</value>
        public int RoundLimit
        {
            get
            {
                return QlServer.roundlimit;
            }
            set
            {
                QlServer.roundlimit = value;
                NotifyOfPropertyChange(() => RoundLimit);
            }
        }

        /// <summary>
        /// Gets or sets the round time limit.
        /// </summary>
        /// <value>The round time limit.</value>
        public int RoundTimeLimit
        {
            get
            {
                return QlServer.roundtimelimit;
            }
            set
            {
                QlServer.roundtimelimit = value;
                NotifyOfPropertyChange(() => RoundLimit);
            }
        }

        /// <summary>
        /// Gets or sets the rule set.
        /// </summary>
        /// <value>The rule set.</value>
        public string RuleSet
        {
            get
            {
                return QlServer.ruleset;
            }
            set
            {
                QlServer.ruleset = value;
                NotifyOfPropertyChange(() => RuleSet);
            }
        }

        /// <summary>
        /// Gets or sets the score limit.
        /// </summary>
        /// <value>The score limit.</value>
        public string ScoreLimit
        {
            get
            {
                return QlServer.scorelimit;
            }
            set
            {
                QlServer.scorelimit = value;
                NotifyOfPropertyChange(() => ScoreLimit);
            }
        }

        /// <summary>
        /// Gets the short name of the game type.
        /// </summary>
        /// <value>The short name of the game type.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public string ShortGameTypeName
        {
            get
            {
                return _formatHelper.Gametypes[GameType].ShortGametypeName;
            }
        }

        /// <summary>
        /// Gets the short name (city) of the location, since Quake Live does not include the
        /// physical location details in the server details API.
        /// </summary>
        /// <value>The short name of the location represented as the city name.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public string ShortLocationName
        {
            get
            {
                LocationData value;
                return _formatHelper.Locations.TryGetValue(LocationId, out value) ? value.City : "Unknown";
            }
        }

        /// <summary>
        /// Gets or sets the skill delta.
        /// </summary>
        /// <value>The skill delta.</value>
        public int SkillDelta
        {
            get
            {
                return QlServer.skillDelta;
            }
            set
            {
                QlServer.skillDelta = value;
                NotifyOfPropertyChange(() => SkillDelta);
            }
        }

        /// <summary>
        /// Gets or sets the size of the team.
        /// </summary>
        /// <value>The size of the team.</value>
        public int TeamSize
        {
            get
            {
                return QlServer.teamsize;
            }
            set
            {
                QlServer.teamsize = value;
                NotifyOfPropertyChange(() => TeamSize);
            }
        }

        /// <summary>
        /// Gets or sets the time limit.
        /// </summary>
        /// <value>The time limit.</value>
        public int TimeLimit
        {
            get
            {
                return QlServer.timelimit;
            }
            set
            {
                QlServer.timelimit = value;
                NotifyOfPropertyChange(() => TimeLimit);
            }
        }

        /// <summary>
        /// Gets or sets the time remaining.
        /// </summary>
        /// <value>The time remaining.</value>
        /// <remarks>This is a custom UI setting.</remarks>
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

        // <summary>
        // Gets the total players.
        // </summary>
        // <value>The total players.</value>
        // <remarks>This is a custom UI setting.</remarks>
        public string TotalPlayers
        {
            get
            {
                return string.Empty + NumPlayers + "/" + MaxClients;
            }
        }

        /// <summary>
        /// Adds the players to a list of players that will be cleanly wrapped and formatted by a PlayerDetailsViewModel.
        /// </summary>
        /// <param name="players">The players.</param>
        /// <returns>A formatted player list.</returns>
        private ObservableCollection<PlayerDetailsViewModel> AddFormattedPlayers(IEnumerable<Player> players)
        {
            _formattedPlayerList.Clear();
            foreach (var player in players)
            {
                _formattedPlayerList.Add(new PlayerDetailsViewModel(player));
            }

            return _formattedPlayerList;
        }

        /// <summary>
        /// Asynchronously calculates the team's average elo for QLRanks-supported team gametypes.
        /// </summary>
        /// <param name="team">The team. <c>1</c> is blue, <c>2</c> is red.</param>
        /// <returns>The team's average elo as a 64-bit signed integer.</returns>
        private async Task<long> CalculateTeamEloAsync(int team)
        {
            if (NumPlayers == 0 || IsTeam0Condition) { return 0; }
            if (!IsQlRanksSupportedTeamGame) { return 0; }

            QlRanksPlayersToUpdate.Clear();

            long totalplayers = 0, totaleloteam = 0, playerelo = 0;

            foreach (var p in Players.Where(p => p.team == team))
            {
                EloData val;
                switch (GameType)
                {
                    // TDM
                    case 3:
                        if (UQltGlobals.PlayerEloInfo.TryGetValue(p.name.ToLower(), out val))
                        {
                            playerelo = UQltGlobals.PlayerEloInfo[p.name.ToLower()].TdmElo;
                        }
                        else
                        {
                            Debug.WriteLine(
                                "Key doesn't exist - no TDM elo data found for player: " + p.name.ToLower());
                            QlRanksPlayersToUpdate.Add(p.name.ToLower());
                        }
                        break;
                    // CA
                    case 4:
                        if (UQltGlobals.PlayerEloInfo.TryGetValue(p.name.ToLower(), out val))
                        {
                            playerelo = UQltGlobals.PlayerEloInfo[p.name.ToLower()].CaElo;
                        }
                        else
                        {
                            QlRanksPlayersToUpdate.Add(p.name.ToLower());
                            Debug.WriteLine(
                                "Key doesn't exist - no CA elo data found for player: " + p.name.ToLower());
                        }
                        break;
                    // CTF
                    case 5:
                        if (UQltGlobals.PlayerEloInfo.TryGetValue(p.name.ToLower(), out val))
                        {
                            playerelo = UQltGlobals.PlayerEloInfo[p.name.ToLower()].CtfElo;
                        }
                        else
                        {
                            QlRanksPlayersToUpdate.Add(p.name.ToLower());
                            Debug.WriteLine(
                                "Key doesn't exist - no TDM elo data found for player: " + p.name.ToLower());
                        }
                        break;
                }

                totalplayers++;
                totaleloteam += playerelo;
            }

            // There are players with missing elo information... Run an update.
            if (QlRanksPlayersToUpdate.Count != 0)
            {
                Debug.WriteLine("Retrieving missing elo information for team (" + team + ") calculation for player(s): " + string.Join("+", QlRanksPlayersToUpdate));
                var qlr = await _qlRanksDataRetriever.GetEloDataFromQlRanksApiAsync(string.Join("+", QlRanksPlayersToUpdate));
                _qlRanksDataRetriever.SetQlRanksPlayersAsync(qlr);

                foreach (var ptoupdate in QlRanksPlayersToUpdate)
                {
                    switch (GameType)
                    {
                        case 3:
                            totaleloteam += UQltGlobals.PlayerEloInfo[ptoupdate].TdmElo;
                            break;

                        case 4:
                            totaleloteam += UQltGlobals.PlayerEloInfo[ptoupdate].CaElo;
                            break;

                        case 5:
                            totaleloteam += UQltGlobals.PlayerEloInfo[ptoupdate].CtfElo;
                            break;
                    }
                }
            }

            return (totaleloteam / totalplayers);
        }

        /// <summary>
        /// Groups the scores and players.
        /// </summary>
        /// <param name="sortBy">The criteria to sort by.</param>
        /// <param name="groupBy">The criteria to group by.</param>
        private void GroupScoresAndPlayers(string sortBy, string groupBy)
        {
            var view = CollectionViewSource.GetDefaultView(FormattedPlayerList);
            var sortDescription = new SortDescription(sortBy, ListSortDirection.Descending);
            var groupDescription = new PropertyGroupDescription(groupBy);
            view.SortDescriptions.Add(sortDescription);
            view.GroupDescriptions.Add(groupDescription);
        }

        /// <summary>
        /// Sorts the servers and formats the players for the view, if this class is being used in
        /// the server browser.
        /// </summary>
        private void SortServersAndFormatPlayers()
        {
            FormattedPlayerList = AddFormattedPlayers(QlServer.players);
            GroupScoresAndPlayers("Score", "Team");
        }
    }
}