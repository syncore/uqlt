using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Nito.AsyncEx;
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

        // port regexp: colon with at least 4 numbers
        private readonly Regex _port = new Regex(@"[\:]\d{4,}");

        private readonly QlRanksDataRetriever _qlRanksDataRetriever = new QlRanksDataRetriever();
        private string _formattedGameState;
        private string _timeRemaining;
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDetailsViewModel" /> class.
        /// </summary>
        /// <param name="server">The server wrapped by this viewmodel.</param>

        [ImportingConstructor]
        public ServerDetailsViewModel(Server server)
        {
            QlServer = server;
            FormattedPlayerList = FormatPlayerCollection(QlServer.players);
        }

        /// <summary>
        /// Gets the blue team's Elo.
        /// </summary>
        /// <value>The blue team's Elo.</value>
        /// <summary>
        public INotifyTaskCompletion<long> BlueTeamElo
        {
            get
            {
                return NotifyTaskCompletion.Create(CalculateTeamEloAsync(2));
            }
        }

        ///<summary>
        /// Gets the size of the blue team.
        /// </summary>
        /// <value>
        /// The size of the blue team.
        /// </value>
        /// <remarks>The Quake Live API does not provide this information by default.</remarks>
        public int BlueTeamSize
        {
            get
            {
                return Players.Count(p => p.team == 2);
            }
        }

        /// <summary>
        /// Gets the capture limit.
        /// </summary>
        /// <value>The capture limit.</value>
        public int CaptureLimit
        {
            get
            {
                return QlServer.capturelimit;
            }
        }

        /// <summary>
        /// Gets the ecode.
        /// </summary>
        /// <value>The ecode.</value>
        public int ECODE
        {
            get
            {
                return QlServer.ECODE;
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
        /// Gets the state of the formatted game.
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
        /// Gets the formatted player list.
        /// </summary>
        /// <value>The formatted player list.</value>
        public List<PlayerDetailsViewModel> FormattedPlayerList { get; set; }

        /// <summary>
        /// Gets the frag limit.
        /// </summary>
        /// <value>The frag limit.</value>
        public int FragLimit
        {
            get
            {
                return QlServer.fraglimit;
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
        /// Gets the type of the game.
        /// </summary>
        /// <value>The type of the game.</value>
        public int GameType
        {
            get
            {
                return QlServer.game_type;
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
        /// Gets the game type title.
        /// </summary>
        /// <value>The game type title.</value>
        public string GameTypeTitle
        {
            get
            {
                return QlServer.game_type_title;
            }
        }

        /// <summary>
        /// Gets the blue score.
        /// </summary>
        /// <value>The blue score.</value>
        public int GBlueScore
        {
            get
            {
                return QlServer.g_bluescore;
            }
        }

        /// <summary>
        /// Gets the custom settings.
        /// </summary>
        /// <value>The custom settings.</value>
        public string GCustomSettings
        {
            get
            {
                return QlServer.g_customSettings;
            }
        }

        /// <summary>
        /// Gets the state of the game.
        /// </summary>
        /// <value>The state of the game.</value>
        public string GGameState
        {
            get
            {
                return QlServer.g_gamestate;
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
        /// Gets the level start time.
        /// </summary>
        /// <value>The level start time.</value>
        public int GLevelStartTime
        {
            get
            {
                return QlServer.g_levelstarttime;
            }
        }

        /// <summary>
        /// Gets whether this server needs a password.
        /// </summary>
        /// <value>The value representing whether this server needs a password.</value>
        public int GNeedPass
        {
            get
            {
                return QlServer.g_needpass;
            }
        }

        /// <summary>
        /// Gets the red score.
        /// </summary>
        /// <value>The red score.</value>
        public int GRedScore
        {
            get
            {
                return QlServer.g_redscore;
            }
        }

        /// <summary>
        /// Gets the host address.
        /// </summary>
        /// <value>The host address.</value>
        public string HostAddress
        {
            get
            {
                return QlServer.host_address;
            }
        }

        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        /// <value>The name of the host.</value>
        public string HostName
        {
            get
            {
                return QlServer.host_name;
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
        /// Gets a value indicating whether the blue team is currently winning.
        /// </summary>
        /// <value>
        /// <c>true</c> if the blue team is leading; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>This is used to properly display the correct order on the team scoreboard. Greater than or equals required.</remarks>
        public bool IsBlueTeamLeading
        {
            get { return (GBlueScore >= GRedScore); }
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
        /// Gets a value indicating whether the red team is currently winning.
        /// </summary>
        /// <value>
        /// <c>true</c> if the red team is leading; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>This is used to properly display the correct order on the team scoreboard. Unlike the blue comparison, simply greater than is required.</remarks>
        public bool IsRedTeamLeading
        {
            get { return (GRedScore > GBlueScore); }
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
        /// Gets the location identifier.
        /// </summary>
        /// <value>The location identifier.</value>
        public long LocationId
        {
            get
            {
                return QlServer.location_id;
            }
        }

        /// <summary>
        /// Gets the map setting.
        /// </summary>
        /// <value>The map setting.</value>
        public string Map
        {
            get
            {
                return QlServer.map;
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
        /// Gets the map title.
        /// </summary>
        /// <value>The map title.</value>
        public string MapTitle
        {
            get
            {
                return QlServer.map_title;
            }
        }

        /// <summary>
        /// Gets the maximum clients.
        /// </summary>
        /// <value>The maximum clients.</value>
        public int MaxClients
        {
            get
            {
                return QlServer.max_clients;
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
        /// Gets the number clients.
        /// </summary>
        /// <value>The number clients.</value>
        public int NumClients
        {
            get
            {
                return QlServer.num_clients;
            }
        }

        /// <summary>
        /// Gets the number players.
        /// </summary>
        /// <value>The number players.</value>
        public int NumPlayers
        {
            get
            {
                return QlServer.num_players;
            }
        }

        /// <summary>
        /// Gets the owner.
        /// </summary>
        /// <value>The owner.</value>
        public string Owner
        {
            get
            {
                return QlServer.owner;
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
                long value;
                string cleanedip = _port.Replace(HostAddress, string.Empty);
                return UQltGlobals.IpAddressDict.TryGetValue(cleanedip, out value) ? value : 0;
            }
        }

        /// <summary>
        /// Gets the player count.
        /// </summary>
        /// <value>
        /// The player count.
        /// </value>
        /// <remarks>The player count will depend on the teamsize. This was
        /// determined by examining the QL JavaScript code.
        /// </remarks>
        public int PlayerCount
        {
            get
            {
                return TeamSize > 0 ? NumPlayers : NumClients;
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
        /// Gets the premium setting.
        /// </summary>
        /// <value>The premium setting.</value>
        public object Premium
        {
            get
            {
                return QlServer.premium;
            }
        }

        /// <summary>
        /// Gets the public identifier.
        /// </summary>
        /// <value>The public identifier.</value>
        public int PublicId
        {
            get
            {
                return QlServer.public_id;
            }
        }

        /// <summary>
        /// Gets the QLRanks data retriever.
        /// </summary>
        /// <value>
        /// The QLRanks data retriever.
        /// </value>
        public QlRanksDataRetriever QlRanksDataRetriever
        {
            get { return _qlRanksDataRetriever; }
        }

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
        /// Gets the QLRanks data retriever.
        /// </summary>
        /// <value>The QLRanksdata retriever.</value>
        /// <summary>
        /// Gets the ranked setting.
        /// </summary>
        /// <value>The ranked setting.</value>
        public int Ranked
        {
            get
            {
                return QlServer.ranked;
            }
        }

        /// <summary>
        /// Gets the red team's Elo.
        /// </summary>
        /// <value>
        /// The red team's Elo.
        /// </value>
        public INotifyTaskCompletion<long> RedTeamElo
        {
            get
            {
                return NotifyTaskCompletion.Create(CalculateTeamEloAsync(1));
            }
        }

        /// <summary>
        /// Gets the size of the red team.
        /// </summary>
        /// <value>
        /// The size of the red team.
        /// </value>
        /// <remarks>The Quake Live API does not provide this information by default.</remarks>
        public int RedTeamSize
        {
            get
            {
                return Players.Count(p => p.team == 1);
            }
        }

        /// <summary>
        /// Gets the round limit.
        /// </summary>
        /// <value>The round limit.</value>
        public int RoundLimit
        {
            get
            {
                return QlServer.roundlimit;
            }
        }

        /// <summary>
        /// Gets the round time limit.
        /// </summary>
        /// <value>The round time limit.</value>
        public int RoundTimeLimit
        {
            get
            {
                return QlServer.roundtimelimit;
            }
        }

        /// <summary>
        /// Gets the rule set.
        /// </summary>
        /// <value>The rule set.</value>
        public string RuleSet
        {
            get
            {
                return QlServer.ruleset;
            }
        }

        /// <summary>
        /// Gets the score limit.
        /// </summary>
        /// <value>The score limit.</value>
        public string ScoreLimit
        {
            get
            {
                return QlServer.scorelimit;
            }
        }

        /// <summary>
        /// Gets the server maximum capacity.
        /// </summary>
        /// <value>
        /// The server maximum capacity.
        /// </value>
        /// <remarks>The maximum server capacity is not always equal to
        ///  the max_clients value in the case where the gametype is FFA.
        ///  This was determined by examining the QL JavaScript code.</remarks>
        public int ServerMaxCapacity
        {
            get
            {
                if (TeamSize > 0)
                {
                    return ((TeamSize) * ((GameType == 0) ? 1 : 2));
                }
                return MaxClients;
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
        /// Gets the skill delta.
        /// </summary>
        /// <value>The skill delta.</value>
        public int SkillDelta
        {
            get
            {
                return QlServer.skillDelta;
            }
        }

        /// <summary>
        /// Gets the size of the team.
        /// </summary>
        /// <value>The size of the team.</value>
        public int TeamSize
        {
            get
            {
                return QlServer.teamsize;
            }
        }

        /// <summary>
        /// Gets the time limit.
        /// </summary>
        /// <value>The time limit.</value>
        public int TimeLimit
        {
            get
            {
                return QlServer.timelimit;
            }
        }

        /// <summary>
        /// Gets the time remaining.
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
                return string.Empty + PlayerCount + "/" + ServerMaxCapacity;
            }
        }

        /// <summary>
        /// Asynchronously calculates the team's average elo for QLRanks-supported team gametypes.
        /// </summary>
        /// <param name="team">The team. <c>1</c> is blue, <c>2</c> is red.</param>
        /// <returns>The team's average elo as a 64-bit signed integer.</returns>
        private async Task<long> CalculateTeamEloAsync(int team)
        {
            if (NumPlayers == 0 || IsTeam0Condition) { return 0; }
            if ((team == 1) && (RedTeamSize == 0)) { return 0; }
            if ((team == 2) && (BlueTeamSize == 0)) { return 0; }
            if (!IsQlRanksSupportedTeamGame) { return 0; }

            var qlRanksPlayersToUpdate = new HashSet<string>();

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
                            qlRanksPlayersToUpdate.Add(p.name.ToLower());
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
                            qlRanksPlayersToUpdate.Add(p.name.ToLower());
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
                            qlRanksPlayersToUpdate.Add(p.name.ToLower());
                            Debug.WriteLine(
                                "Key doesn't exist - no TDM elo data found for player: " + p.name.ToLower());
                        }
                        break;
                }

                totalplayers++;
                totaleloteam += playerelo;
            }

            // There are players with missing elo information... Run an update.
            if (qlRanksPlayersToUpdate.Count != 0)
            {
                // Create
                _qlRanksDataRetriever.CreateEloData(qlRanksPlayersToUpdate);

                Debug.WriteLine("Retrieving missing elo information for team (" + team + ") calculation for player(s): " + string.Join("+", qlRanksPlayersToUpdate));
                var qlr = await _qlRanksDataRetriever.GetEloDataFromQlRanksApiAsync(string.Join("+", qlRanksPlayersToUpdate));
                _qlRanksDataRetriever.SetQlRanksPlayersAsync(qlr);

                foreach (var ptoupdate in qlRanksPlayersToUpdate)
                {
                    EloData val;
                    switch (GameType)
                    {
                        case 3:
                            if (UQltGlobals.PlayerEloInfo.TryGetValue(ptoupdate.ToLower(), out val))
                            {
                                totaleloteam += UQltGlobals.PlayerEloInfo[ptoupdate].TdmElo;
                            }
                            else
                            {
                                Debug.WriteLine("...Key still does not exist yet for " + ptoupdate + " [TDM]......");
                            }
                            break;

                        case 4:
                            if (UQltGlobals.PlayerEloInfo.TryGetValue(ptoupdate.ToLower(), out val))
                            {
                                totaleloteam += UQltGlobals.PlayerEloInfo[ptoupdate].CaElo;
                            }
                            else
                            {
                                Debug.WriteLine("...Key still does not exist yet for " + ptoupdate + " [CA]......");
                            }
                            break;

                        case 5:
                            if (UQltGlobals.PlayerEloInfo.TryGetValue(ptoupdate.ToLower(), out val))
                            {
                                totaleloteam += UQltGlobals.PlayerEloInfo[ptoupdate].CtfElo;
                            }
                            else
                            {
                                Debug.WriteLine("...Key still does not exist yet for " + ptoupdate + " [CTF]......");
                            }
                            break;
                    }
                }
            }

            return (totaleloteam / totalplayers);
        }

        /// <summary>
        /// Adds the players to a list of players that will be cleanly wrapped by a PlayerDetailsViewModel, orders the list by score, and groups it by team.
        /// </summary>
        /// <param name="players">The players.</param>
        /// <returns>A formatted player list.</returns>
        /// <remarks>This replaced <see cref="GridViewSort"/>for the player details, which required an observable collection to work properly (more overhead) and did not always work correctly.</remarks>
        private List<PlayerDetailsViewModel> FormatPlayerCollection(IEnumerable<Player> players)
        {
            var sorted = players.Select(player => new PlayerDetailsViewModel(player)).ToList();
            return sorted.OrderByDescending(a => a.Score).GroupBy(a => a.Team).SelectMany(a => a.ToList()).ToList();
        }
    }
}