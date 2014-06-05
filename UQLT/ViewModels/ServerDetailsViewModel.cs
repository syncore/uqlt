﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using UQLT.Helpers;
using UQLT.Models.QLRanks;
using UQLT.Models.QuakeLiveAPI;

namespace UQLT.ViewModels
{
    [Export(typeof(ServerDetailsViewModel))]

    /// <summary>
    /// Individual server viewmodel. This class wraps a Server class and exposes additional
    /// properties specific to the View (in this case, ServerBrowserView).
    /// </summary>
    public class ServerDetailsViewModel : PropertyChangedBase
    {
        private long _blueTeamElo;

        private string _formattedGameState;

        private ObservableCollection<PlayerDetailsViewModel> _formattedPlayerList = new ObservableCollection<PlayerDetailsViewModel>();

        private long _redTeamElo;

        private string _timeRemaining;

        private QLFormatHelper FormatHelper = QLFormatHelper.Instance;

        // port regexp: colon with at least 4 numbers
        private Regex port = new Regex(@"[\:]\d{4,}");

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDetailsViewModel" /> class.
        /// </summary>
        /// <param name="server">The server wrapped by this viewmodel.</param>
        [ImportingConstructor]
        public ServerDetailsViewModel(Server server)
        {
            Server = server;
            FormattedPlayerList = AddFormattedPlayers(server.players);
            GroupScoresAndPlayers("Score", "Team");
        }

        /// <summary>
        /// Gets or sets the blue team's Elo.
        /// </summary>
        /// <value>The blue team's Elo.</value>
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

        /// <summary>
        /// Gets or sets the capture limit.
        /// </summary>
        /// <value>The capture limit.</value>
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

        /// <summary>
        /// Gets or sets the ecode.
        /// </summary>
        /// <value>The ecode.</value>
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
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/flags/" + LocationId.ToString() + ".gif", UriKind.RelativeOrAbsolute));
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
                return Server.fraglimit;
            }
            set
            {
                Server.fraglimit = value;
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

        /// <summary>
        /// Gets or sets the type of the game.
        /// </summary>
        /// <value>The type of the game.</value>
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
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/" + GameType.ToString() + ".gif", UriKind.RelativeOrAbsolute));
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
                return Server.game_type_title;
            }
            set
            {
                Server.game_type_title = value;
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
                return Server.g_bluescore;
            }
            set
            {
                Server.g_bluescore = value;
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
                return Server.g_customSettings;
            }
            set
            {
                Server.g_customSettings = value;
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
                return Server.g_gamestate;
            }
            set
            {
                Server.g_gamestate = value;
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
                return Server.g_instagib;
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
                return Server.g_levelstarttime;
            }
            set
            {
                Server.g_levelstarttime = value;
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
                return Server.g_needpass;
            }
            set
            {
                Server.g_needpass = value;
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
                return Server.g_redscore;
            }
            set
            {
                Server.g_redscore = value;
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
                return Server.host_address;
            }
            set
            {
                Server.host_address = value;
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
                return Server.host_name;
            }
            set
            {
                Server.host_name = value;
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
        /// Gets a value indicating whether this server's gametype is supported by QLRanks.
        /// </summary>
        /// <value><c>true</c> if this QLRanks supports this server's gametype otherwise, <c>false</c>.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public bool IsQLRanksSupported
        {
            get
            {
                return (GameType == 0 || GameType == 1 || GameType == 3 || GameType == 4 || GameType == 5) ? true : false;
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

        /// <summary>
        /// Gets a value indicating whether this server features a team game type.
        /// </summary>
        /// <value><c>true</c> if this server features a team game; otherwise, <c>false</c>.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public bool IsTeamGame
        {
            get
            {
                return (GameType == 3 || GameType == 4 || GameType == 5 || GameType == 6 || GameType == 8 || GameType == 9 || GameType == 10 || GameType == 11) ? true : false;
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

        /// <summary>
        /// Gets or sets the map setting.
        /// </summary>
        /// <value>The map setting.</value>
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
                return Server.map_title;
            }
            set
            {
                Server.map_title = value;
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
                return Server.max_clients;
            }
            set
            {
                Server.max_clients = value;
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
                return Server.num_clients;
            }
            set
            {
                Server.num_clients = value;
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
                return Server.num_players;
            }
            set
            {
                Server.num_players = value;
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
                return Server.owner;
            }
            set
            {
                Server.owner = value;
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
                return UQLTGlobals.IPAddressDict[cleanedip];
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
                return Server.players;
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
                return Server.premium;
            }
            set
            {
                Server.premium = value;
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
                return Server.public_id;
            }
            set
            {
                Server.public_id = value;
                NotifyOfPropertyChange(() => PublicId);
            }
        }

        /// <summary>
        /// Gets or sets the ranked setting.
        /// </summary>
        /// <value>The ranked setting.</value>
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

        /// <summary>
        /// Gets or sets the red team's Elo.
        /// </summary>
        /// <value>The red team's Elo.</value>
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

        /// <summary>
        /// Gets or sets the round limit.
        /// </summary>
        /// <value>The round limit.</value>
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

        /// <summary>
        /// Gets or sets the round time limit.
        /// </summary>
        /// <value>The round time limit.</value>
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

        /// <summary>
        /// Gets or sets the rule set.
        /// </summary>
        /// <value>The rule set.</value>
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

        /// <summary>
        /// Gets or sets the score limit.
        /// </summary>
        /// <value>The score limit.</value>
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

        /// <summary>
        /// Gets the server that this viewmodel wraps.
        /// </summary>
        /// <value>The server that this viewmodel wraps.</value>
        public Server Server
        {
            get;
            private set;
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
                return FormatHelper.Gametypes[GameType].ShortGametypeName;
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

        /// <summary>
        /// Gets or sets the skill delta.
        /// </summary>
        /// <value>The skill delta.</value>
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

        /// <summary>
        /// Gets or sets the size of the team.
        /// </summary>
        /// <value>The size of the team.</value>
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

        /// <summary>
        /// Gets or sets the time limit.
        /// </summary>
        /// <value>The time limit.</value>
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
        private ObservableCollection<PlayerDetailsViewModel> AddFormattedPlayers(List<Player> players)
        {
            _formattedPlayerList.Clear();
            foreach (var player in players)
            {
                _formattedPlayerList.Add(new PlayerDetailsViewModel(player));
            }

            return _formattedPlayerList;
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
    }
}