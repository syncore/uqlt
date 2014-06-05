using System.Collections.Generic;
using UQLT.Models.QLRanks;

namespace UQLT.Models.QuakeLiveAPI
{
    /// <summary>
    /// Model representing a Quake Live server.
    /// This is the format of an individual server returned from http://www.quakelive.com/browser/details?ids={server_id(s)} - it's different from that returned by /list?filter={base64_encoded_json_filter}
    /// Note: browser/details?ids={server_id(s)} actually includes a list of players for a given server, unlike /list?filter={base64_encoded_json_filter}
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Gets or sets the num_players.
        /// </summary>
        /// <value>
        /// The num_players.
        /// </value>
        public int num_players
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the public_id.
        /// </summary>
        /// <value>
        /// The public_id.
        /// </value>
        public int public_id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ecode.
        /// </summary>
        /// <value>
        /// The ecode.
        /// </value>
        public int ECODE
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the teamsize.
        /// </summary>
        /// <value>
        /// The teamsize.
        /// </value>
        public int teamsize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the g_custom settings.
        /// </summary>
        /// <value>
        /// The g_custom settings.
        /// </value>
        public string g_customSettings
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the g_levelstarttime.
        /// </summary>
        /// <value>
        /// The g_levelstarttime.
        /// </value>
        public int g_levelstarttime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location_id.
        /// </summary>
        /// <value>
        /// The location_id.
        /// </value>
        public long location_id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the players.
        /// </summary>
        /// <value>
        /// The players.
        /// </value>
        public List<Player> players
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the max_clients.
        /// </summary>
        /// <value>
        /// The max_clients.
        /// </value>
        public int max_clients
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the roundtimelimit.
        /// </summary>
        /// <value>
        /// The roundtimelimit.
        /// </value>
        public int roundtimelimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the map_title.
        /// </summary>
        /// <value>
        /// The map_title.
        /// </value>
        public string map_title
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the scorelimit.
        /// </summary>
        /// <value>
        /// The scorelimit.
        /// </value>
        public string scorelimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ruleset.
        /// </summary>
        /// <value>
        /// The ruleset.
        /// </value>
        public string ruleset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the skill delta.
        /// </summary>
        /// <value>
        /// The skill delta.
        /// </value>
        public int skillDelta
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the game_type_title.
        /// </summary>
        /// <value>
        /// The game_type_title.
        /// </value>
        public string game_type_title
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the map.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public string map
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>
        /// The premium.
        /// </value>
        public object premium
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the g_needpass.
        /// </summary>
        /// <value>
        /// The g_needpass.
        /// </value>
        public int g_needpass
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ranked.
        /// </summary>
        /// <value>
        /// The ranked.
        /// </value>
        public int ranked
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the g_instagib.
        /// </summary>
        /// <value>
        /// The g_instagib.
        /// </value>
        public int g_instagib
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the g_bluescore.
        /// </summary>
        /// <value>
        /// The g_bluescore.
        /// </value>
        public int g_bluescore
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the g_gamestate.
        /// </summary>
        /// <value>
        /// The g_gamestate.
        /// </value>
        public string g_gamestate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the host_address.
        /// </summary>
        /// <value>
        /// The host_address.
        /// </value>
        public string host_address
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the fraglimit.
        /// </summary>
        /// <value>
        /// The fraglimit.
        /// </value>
        public int fraglimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the num_clients.
        /// </summary>
        /// <value>
        /// The num_clients.
        /// </value>
        public int num_clients
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the capturelimit.
        /// </summary>
        /// <value>
        /// The capturelimit.
        /// </value>
        public int capturelimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the game_type.
        /// </summary>
        /// <value>
        /// The game_type.
        /// </value>
        public int game_type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the timelimit.
        /// </summary>
        /// <value>
        /// The timelimit.
        /// </value>
        public int timelimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the roundlimit.
        /// </summary>
        /// <value>
        /// The roundlimit.
        /// </value>
        public int roundlimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the host_name.
        /// </summary>
        /// <value>
        /// The host_name.
        /// </value>
        public string host_name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the g_redscore.
        /// </summary>
        /// <value>
        /// The g_redscore.
        /// </value>
        public int g_redscore
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        public string owner
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the player game type from the QL server gametype on a per-server basis.
        /// </summary>
        /// <param name="gt">The gametype.</param>
        /// <remarks>
        /// Needed because Player class has no access to the server's game_type. Player class will use this value to pull the correct Elo value from the elo dictionary.
        /// UI will also use this value especially when trying to determine the correct score format, most notably when RACE servers are involved.
        /// This could be better, but it was necessarry to: (1) eliminate UI visual/logic tree errors in master-detail scenario where Player class doesn't have direct access to
        /// the game_type of the server & can't accurately determine ServerBrowser ElementName (without x:Reference hackery) and: (2) enable ability to sort players by Elo column in UI.
        /// </remarks>
        public void setPlayerGameTypeFromServer(int gt)
        {
            foreach (var p in players)
            {
                p.player_game_type = gt;
            }
        }

        /// <summary>
        /// Creates the EloData object for each player on the server.
        /// </summary>
        public void createEloData()
        {
            EloData val;
            foreach (var p in players)
            {
                if (!UQLTGlobals.PlayerEloInfo.TryGetValue(p.name.ToLower(), out val))
                {
                    UQLTGlobals.PlayerEloInfo[p.name.ToLower()] = new EloData()
                    {
                        DuelElo = 0,
                        CaElo = 0,
                        TdmElo = 0,
                        FfaElo = 0,
                        CtfElo = 0
                    };
                }
            }
        }

        /// <summary>
        /// Sets the player Elo information on each Player object based on the dictionary containing all of the elo information.
        /// </summary>
        /// <remarks>
        /// This allows the elo to be set as a property in the PlayerDetailsViewModel,
        /// which enables automatic UI updating via NotifyOfPropertyChange when elo information is received.
        /// </remarks>
        public void setPlayerElos()
        {
            foreach (var p in players)
            {
                p.tdmelo = UQLTGlobals.PlayerEloInfo[p.name.ToLower()].TdmElo;
                p.caelo = UQLTGlobals.PlayerEloInfo[p.name.ToLower()].CaElo;
                p.ffaelo = UQLTGlobals.PlayerEloInfo[p.name.ToLower()].FfaElo;
                p.duelelo = UQLTGlobals.PlayerEloInfo[p.name.ToLower()].DuelElo;
                p.ctfelo = UQLTGlobals.PlayerEloInfo[p.name.ToLower()].CtfElo;
            }
        }
    }
}