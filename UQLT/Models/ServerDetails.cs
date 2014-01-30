﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models
{
    public class ServerDetails
    {
        public int num_players { get; set; }
        public int public_id { get; set; }
        public int ECODE { get; set; }
        public int teamsize { get; set; }
        public string g_customSettings { get; set; }
        public int g_levelstarttime { get; set; }
        public int location_id { get; set; }
        public List<Player> players { get; set; }
        public int max_clients { get; set; }
        public int roundtimelimit { get; set; }
        public string map_title { get; set; }
        public string scorelimit { get; set; }
        public string ruleset { get; set; }
        public int skillDelta { get; set; }
        public string game_type_title { get; set; }
        public string map { get; set; }
        public object premium { get; set; }
        public int g_needpass { get; set; }
        public int ranked { get; set; }
        public int g_instagib { get; set; }
        public int g_bluescore { get; set; }
        public string g_gamestate { get; set; }
        public string host_address { get; set; }
        public int fraglimit { get; set; }
        public int num_clients { get; set; }
        public int capturelimit { get; set; }
        public int game_type { get; set; }
        public int timelimit { get; set; }
        public int roundlimit { get; set; }
        public string host_name { get; set; }
        public int g_redscore { get; set; }
        public string owner { get; set; }
        
        public void setPlayerGameTypeFromServer(int gt)
        {
            // This will set the player's gametype from the QL server gametype on a per-server basis.
            // Needed because Player class has no access to the server's game_type. Player class will use this value to pull the correct Elo value from the elo dictionary.
            // UI will also use this value especially when trying to determine the correct score format, most notably when RACE servers are involved.
            // This could be better, but it was necessarry to: (1) eliminate UI visual/logic tree errors in master-detail scenario where Player class doesn't have direct access to
            // the game_type of the server & can't accurately determine ServerBrowser ElementName (without x:Reference hackery) and: (2) enable ability to sort players by Elo column in UI.
            foreach (var p in players)
            {
                p.player_game_type = gt;
            }

        }
    }
}
