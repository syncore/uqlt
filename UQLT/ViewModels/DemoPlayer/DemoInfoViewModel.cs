using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UQLT.Models.DemoPlayer;

namespace UQLT.ViewModels.DemoPlayer
{
    /// <summary>
    /// Individual demo viewmodel. This class wraps a <see cref="Demo"/> class and exposes additional
    /// properties specific to the View (in this case, DemoPlayerView).
    /// </summary>
    /// <remarks>This viewmodel does not have a separate view.</remarks>
    [Export(typeof(DemoInfoViewModel))]
    public class DemoInfoViewModel
    {
        private static readonly Regex NameColors = new Regex(@"[\^]\d");
        private readonly Dictionary<string, string> _serverInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoInfoViewModel"/> class.
        /// </summary>
        /// <param name="demo">The demo.</param>
        [ImportingConstructor]
        public DemoInfoViewModel(Demo demo)
        {
            this.Demo = demo;
            FormattedDemoInfoPlayers = FormatPlayerCollection(GetAllPlayers());
            _serverInfo = CreateServerInfo();
        }

        /// <summary>
        /// Gets the demo's date.
        /// </summary>
        /// <value>
        /// The demo's date.
        /// </value>
        public string Date
        {
            get { return Demo.timestamp; }
        }

        /// <summary>
        /// Gets the demo associated with this viewmodel.
        /// </summary>
        /// <value>
        /// The demo associated with this viewmodel.
        /// </value>
        public Demo Demo
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the demo filename.
        /// </summary>
        /// <value>
        /// The demo filename.
        /// </value>
        public string Filename
        {
            get { return Demo.filename; }
        }

        /// <summary>
        /// Gets the filesize.
        /// </summary>
        /// <value>
        /// The filesize.
        /// </value>
        public string Filesize
        {
            get { return string.Format("{0} MB", Demo.size.ToString("F2")); }
        }

        /// <summary>
        /// Gets the formatted player list for the demo.
        /// </summary>
        /// <value>The formatted player list.</value>
        public List<DemoInfoPlayerViewModel> FormattedDemoInfoPlayers { get; set; }

        /// <summary>
        /// Gets the type of the game.
        /// </summary>
        /// <value>
        /// The type of the game.
        /// </value>
        public int GameType
        {
            get { return Demo.gametype; }
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
                    Debug.WriteLine("Error loading gametype image {0}, info: {1}", GameType, ex.Message);
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/unknown_game_type.gif", UriKind.RelativeOrAbsolute));
                }
            }
        }

        /// <summary>
        /// Gets the game type title.
        /// </summary>
        /// <value>
        /// The game type title.
        /// </value>
        public string GameTypeTitle
        {
            get { return Demo.gametype_title; }
        }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public string Location
        {
            get { return Demo.srvinfo.sv_location; }
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public string Map
        {
            get { return Demo.map_name; }
        }

        /// <summary>
        /// Gets the map image.
        /// </summary>
        /// <value>The map image.</value>
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
                    Debug.WriteLine("Error loading map image {0}, info: {1}", Map, ex.Message);
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/maps/unknown_map.jpg", UriKind.RelativeOrAbsolute));
                }
            }
        }

        /// <summary>
        /// Gets the players.
        /// </summary>
        /// <value>
        /// The players.
        /// </value>
        public List<Player> Players
        {
            get { return Demo.players; }
        }

        /// <summary>
        /// Gets the protocol.
        /// </summary>
        /// <value>
        /// The protocol.
        /// </value>
        public string Protocol
        {
            get { return Demo.protocol; }
        }

        /// <summary>
        /// Gets the name of the player who recorded the demo.
        /// </summary>
        /// <value>
        /// The name of the player who recorded the demo.
        /// </value>
        public string RecordedBy
        {
            get
            {
                return !string.IsNullOrEmpty(Demo.recorded_by) ? NameColors.Replace(Demo.recorded_by, string.Empty) : string.Empty;
            }
        }

        /// <summary>
        /// Gets the demo server information.
        /// </summary>
        /// <value>
        /// The demo server information.
        /// </value>
        public Dictionary<string, string> ServerInfo
        {
            get { return _serverInfo; }
        }

        /// <summary>
        /// Gets the spectators.
        /// </summary>
        /// <value>
        /// The spectators.
        /// </value>
        public List<Player> Spectators
        {
            get { return Demo.spectators; }
        }

        /// <summary>
        /// Creates a dictionary for the server information for the cvar-value binding in the UI
        /// </summary>
        /// <returns>A string dictionary containing the server info cvars and corresponding values.</returns>
        private Dictionary<string, string> CreateServerInfo()
        {
            var serverInfo = new Dictionary<string, string>();
            // Create only the necessarry key-values; not all demos contain each cvar.
            if (!string.IsNullOrEmpty(Demo.srvinfo.bot_minplayers))
                serverInfo["bot_minplayers"] = Demo.srvinfo.bot_minplayers;
            if (!string.IsNullOrEmpty(Demo.srvinfo.capturelimit))
                serverInfo["capturelimit"] = Demo.srvinfo.capturelimit;
            if (!string.IsNullOrEmpty(Demo.srvinfo.dmflags))
                serverInfo["dmflags"] = Demo.srvinfo.dmflags;
            if (!string.IsNullOrEmpty(Demo.srvinfo.fraglimit))
                serverInfo["fraglimit"] = Demo.srvinfo.fraglimit;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_adCaptureScoreBonus))
                serverInfo["g_adCaptureScoreBonus"] = Demo.srvinfo.g_adCaptureScoreBonus;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_adElimScoreBonus))
                serverInfo["g_adElimScoreBonus"] = Demo.srvinfo.g_adElimScoreBonus;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_adTouchScoreBonus))
                serverInfo["g_adTouchScoreBonus"] = Demo.srvinfo.g_adTouchScoreBonus;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_aspectEnable))
                serverInfo["g_aspectEnable"] = Demo.srvinfo.g_aspectEnable;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_battleSuitDampen))
                serverInfo["g_battleSuitDampen"] = Demo.srvinfo.g_battleSuitDampen;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_blueteam))
                serverInfo["g_blueteam"] = Demo.srvinfo.g_blueteam;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_compmode))
                serverInfo["g_compmode"] = Demo.srvinfo.g_compmode;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_compMode))
                serverInfo["g_compMode"] = Demo.srvinfo.g_compMode;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_customSettings))
                serverInfo["g_customSettings"] = Demo.srvinfo.g_customSettings;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_domCapTime))
                serverInfo["g_domCapTime"] = Demo.srvinfo.g_domCapTime;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_domination))
                serverInfo["g_domination"] = Demo.srvinfo.g_domination;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_domScoreRate))
                serverInfo["g_domScoreRate"] = Demo.srvinfo.g_domScoreRate;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_enableBreath))
                serverInfo["g_enableBreath"] = Demo.srvinfo.g_enableBreath;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_enableDust))
                serverInfo["g_enableDust"] = Demo.srvinfo.g_enableDust;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_freeze))
                serverInfo["g_freeze"] = Demo.srvinfo.g_freeze;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_freezeRoundDelay))
                serverInfo["g_freezeRoundDelay"] = Demo.srvinfo.g_freezeRoundDelay;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_gameState))
                serverInfo["g_gameState"] = Demo.srvinfo.g_gameState;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_gametype))
                serverInfo["g_gametype"] = Demo.srvinfo.g_gametype;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_gravity))
                serverInfo["g_gravity"] = Demo.srvinfo.g_gravity;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_holiday))
                serverInfo["g_holiday"] = Demo.srvinfo.g_holiday;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_instaGib))
                serverInfo["g_instaGib"] = Demo.srvinfo.g_instaGib;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_instagib))
                serverInfo["g_instagib"] = Demo.srvinfo.g_instagib;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_levelStartTime))
                serverInfo["g_levelStartTime"] = Demo.srvinfo.g_levelStartTime;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_loadout))
                serverInfo["g_loadout"] = Demo.srvinfo.g_loadout;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_maxDeferredSpawns))
                serverInfo["g_maxDeferredSpawns"] = Demo.srvinfo.g_maxDeferredSpawns;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_maxGameClients))
                serverInfo["g_maxGameClients"] = Demo.srvinfo.g_maxGameClients;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_maxSkillTier))
                serverInfo["g_maxSkillTier"] = Demo.srvinfo.g_maxSkillTier;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_maxStandardClients))
                serverInfo["g_maxStandardClients"] = Demo.srvinfo.g_maxStandardClients;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_needpass))
                serverInfo["g_needpass"] = Demo.srvinfo.g_needpass;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_obeliskRespawnDelay))
                serverInfo["g_obeliskRespawnDelay"] = Demo.srvinfo.g_obeliskRespawnDelay;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_overtime))
                serverInfo["g_overtime"] = Demo.srvinfo.g_overtime;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_quadDamageFactor))
                serverInfo["g_quadDamageFactor"] = Demo.srvinfo.g_quadDamageFactor;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_raceAllowStandard))
                serverInfo["g_raceAllowStandard"] = Demo.srvinfo.g_raceAllowStandard;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_redteam))
                serverInfo["g_redteam"] = Demo.srvinfo.g_redteam;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_roundWarmupDelay))
                serverInfo["g_roundWarmupDelay"] = Demo.srvinfo.g_roundWarmupDelay;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_startingHealth))
                serverInfo["g_startingHealth"] = Demo.srvinfo.g_startingHealth;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_teamForceBalance))
                serverInfo["g_teamForceBalance"] = Demo.srvinfo.g_teamForceBalance;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_teamSizeMin))
                serverInfo["g_teamSizeMin"] = Demo.srvinfo.g_teamSizeMin;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_teamSpecFreeCam))
                serverInfo["g_teamSpecFreeCam"] = Demo.srvinfo.g_teamSpecFreeCam;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_timeoutCount))
                serverInfo["g_timeoutCount"] = Demo.srvinfo.g_timeoutCount;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_voteFlags))
                serverInfo["g_voteFlags"] = Demo.srvinfo.g_voteFlags;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_weaponrespawn))
                serverInfo["g_weaponrespawn"] = Demo.srvinfo.g_weaponrespawn;
            if (!string.IsNullOrEmpty(Demo.srvinfo.g_weaponRespawn))
                serverInfo["g_weaponRespawn"] = Demo.srvinfo.g_weaponRespawn;
            if (!string.IsNullOrEmpty(Demo.srvinfo.gamename))
                serverInfo["gamename"] = Demo.srvinfo.gamename;
            if (!string.IsNullOrEmpty(Demo.srvinfo.gt_realm))
                serverInfo["gt_realm"] = Demo.srvinfo.gt_realm;
            if (!string.IsNullOrEmpty(Demo.srvinfo.mapname))
                serverInfo["mapname"] = Demo.srvinfo.mapname;
            if (!string.IsNullOrEmpty(Demo.srvinfo.mercylimit))
                serverInfo["mercylimit"] = Demo.srvinfo.mercylimit;
            if (!string.IsNullOrEmpty(Demo.srvinfo.protocol))
                serverInfo["protocol"] = Demo.srvinfo.protocol;
            if (!string.IsNullOrEmpty(Demo.srvinfo.roundlimit))
                serverInfo["roundlimit"] = Demo.srvinfo.roundlimit;
            if (!string.IsNullOrEmpty(Demo.srvinfo.roundtimelimit))
                serverInfo["roundtimelimit"] = Demo.srvinfo.roundtimelimit;
            if (!string.IsNullOrEmpty(Demo.srvinfo.ruleset))
                serverInfo["ruleset"] = Demo.srvinfo.ruleset;
            if (!string.IsNullOrEmpty(Demo.srvinfo.scorelimit))
                serverInfo["scorelimit"] = Demo.srvinfo.scorelimit;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_advertising))
                serverInfo["sv_advertising"] = Demo.srvinfo.sv_advertising;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_adXmitDelay))
                serverInfo["sv_adXmitDelay"] = Demo.srvinfo.sv_adXmitDelay;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_allowDownload))
                serverInfo["sv_allowDownload"] = Demo.srvinfo.sv_allowDownload;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_floodProtect))
                serverInfo["sv_floodProtect"] = Demo.srvinfo.sv_floodProtect;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_gtid))
                serverInfo["sv_gtid"] = Demo.srvinfo.sv_gtid;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_hostname))
                serverInfo["sv_hostname"] = Demo.srvinfo.sv_hostname;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_location))
                serverInfo["sv_location"] = Demo.srvinfo.sv_location;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_maxclients))
                serverInfo["sv_maxclients"] = Demo.srvinfo.sv_maxclients;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_maxPing))
                serverInfo["sv_maxPing"] = Demo.srvinfo.sv_maxPing;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_maxRate))
                serverInfo["sv_maxRate"] = Demo.srvinfo.sv_maxRate;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_minPing))
                serverInfo["sv_minPing"] = Demo.srvinfo.sv_minPing;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_monkeysOnly))
                serverInfo["sv_monkeysOnly"] = Demo.srvinfo.sv_monkeysOnly;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_owner))
                serverInfo["sv_owner"] = Demo.srvinfo.sv_owner;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_premium))
                serverInfo["sv_premium"] = Demo.srvinfo.sv_premium;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_privateClients))
                serverInfo["sv_privateClient"] = Demo.srvinfo.sv_privateClients;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_punkbuster))
                serverInfo["sv_punkbuster"] = Demo.srvinfo.sv_punkbuster;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_ranked))
                serverInfo["sv_ranked"] = Demo.srvinfo.sv_ranked;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_skillRating))
                serverInfo["sv_skillRating"] = Demo.srvinfo.sv_skillRating;
            if (!string.IsNullOrEmpty(Demo.srvinfo.sv_warmupReadyPercentage))
                serverInfo["sv_warmupReadyPercentage"] = Demo.srvinfo.sv_warmupReadyPercentage;
            if (!string.IsNullOrEmpty(Demo.srvinfo.teamsize))
                serverInfo["teamsize"] = Demo.srvinfo.teamsize;
            if (!string.IsNullOrEmpty(Demo.srvinfo.timelimit))
                serverInfo["timelimit"] = Demo.srvinfo.timelimit;
            if (!string.IsNullOrEmpty(Demo.srvinfo.version))
                serverInfo["version"] = Demo.srvinfo.version;

            return serverInfo;
        }

        /// <summary>
        /// Adds the players to a list of players that will be cleanly wrapped by a DemoPlayerInfoViewModel, orders the list by player name, and groups it by team.
        /// </summary>
        /// <param name="players">The players.</param>
        /// <returns>A formatted player list.</returns>
        private List<DemoInfoPlayerViewModel> FormatPlayerCollection(IEnumerable<Player> players)
        {
            var sorted = players.Select(player => new DemoInfoPlayerViewModel(player)).ToList();
            return sorted.OrderByDescending(a => a.Name).GroupBy(a => a.Team).SelectMany(a => a.ToList()).ToList();
        }

        /// <summary>
        /// Gets all players. The demo dumper treats players and spectators as separate lists, so this combines them.
        /// </summary>
        private IEnumerable<Player> GetAllPlayers()
        {
            var allPlayers = Players.ToList();
            allPlayers.AddRange(Spectators);
            return allPlayers;
        }
    }
}