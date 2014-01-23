using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using UQLT.Models;
using System.IO;
using UQLT.Helpers;
using Newtonsoft.Json;
using System.Windows;
using System.ComponentModel.Composition;

namespace UQLT.ViewModels
{
    [Export(typeof(FilterViewModel))]
    public class FilterViewModel
    {
        // list of maps that receive "tag" description for arena_type when building filter string
        static List<String> arenatag = new List<String>();
        // list of maps that receive "map" description for arena_type when building filter string
        static List<String> arenamap = new List<String>();

        SavedFilters sf = new SavedFilters();
        
        //ImportedFilters importedfilters;
        List<ActiveLocation> _active_locations;
        List<InactiveLocation> _inactive_locations;
        List<ServerBrowserLocations> _serverbrowser_locations;
        List<Arena> _arenas;
        List<Difficulty> _difficulty;
        List<GameState> _gamestate;
        List<GameType> _game_types;
        List<GameInfo> _game_info;
        List<String> _game_visibility = new List<String>();
        int _gametype_index;
        int _gamearena_index;
        int _gamelocation_index;
        int _gamestate_index;
        int _gamevisibility_index;
        int _gamepremium_index;
        bool _gamepremium_bool;

        public FilterViewModel()
        {
            // load hard-coded fail-safe filters if downloaded list doesn't exist
            if (!downloadedFilterListExists())
            {
                FailsafeFilterHelper failsafe = new FailsafeFilterHelper();
                failsafe.getFilterBackup();
                LoadFilterList();
                if (savedUserFiltersExist()) { ApplySavedUserFilters(); }
                else { SetStandardDefaultFilters(); }
            }
            else
            {
                LoadFilterList();
                if (savedUserFiltersExist())
                { ApplySavedUserFilters(); }
                else { SetStandardDefaultFilters(); }
            }

            _game_visibility.Add("Public games");
            _game_visibility.Add("Private games");

        }

        private bool savedUserFiltersExist()
        {
            return (File.Exists(UQLTFilepaths.saveduserfilterpath)) ? true : false;
        }

        private bool downloadedFilterListExists()
        {
            return (File.Exists(UQLTFilepaths.currentfilterpath)) ? true : false;
        }
        
        public List<ActiveLocation> active_locations
        {
            get
            {
                //_active_locations = new List<ActiveLocation>(importedfilters.active_locations);
                return _active_locations;
            }
            set
            {
                _active_locations = value;
            }
        }

        public List<InactiveLocation> inactive_locations
        {
            get
            {
                //_inactive_locations = new List<InactiveLocation>(importedfilters.inactive_locations);
                return _inactive_locations;
            }
            set
            {
                _inactive_locations = value;
            }
        }

        public List<ServerBrowserLocations> serverbrowser_locations
        {
            get
            {
               // _serverbrowser_locations = new List<ServerBrowserLocations>(importedfilters.serverbrowser_locations);
                return _serverbrowser_locations;
            }
            set
            {
                _serverbrowser_locations = value;
            }
        }

        public List<Arena> arenas
        {
            get
            {
                //_arenas = new List<Arena>(importedfilters.arenas);
                return _arenas;
            }
            set
            {
                _arenas = value;
            }
        }

        public List<Difficulty> difficulty
        {
            get
            {
                //_difficulty = new List<Difficulty>(importedfilters.difficulty);
                return _difficulty;
            }
            set
            {
                _difficulty = value;
            }
        }
        
        public List<GameState> gamestate
        {
            get
            {
                //_gamestate = new List<GameState>(importedfilters.gamestate);
                return _gamestate;
            }
            set
            {
                _gamestate = value;
            }
        }

        public List<GameType> game_types
        {
            get
            {
                //_game_types = new List<GameType>(importedfilters.game_types);
                return _game_types;
            }
            set
            {
                _game_types = value;
            }
        }

        public List<GameInfo> game_info
        {
            get
            {
                //_game_info = new List<GameInfo>(importedfilters.game_info);
                return _game_info;
            }
            set
            {
                _game_info = value;
            }
        }

        public List<String> game_visibility
        {
            get { return _game_visibility; }
        }

        public int gametype_index
        {
            get { return _gametype_index; }
            set { _gametype_index = value; }
        }

        public int gamearena_index
        {
            get { return _gamearena_index; }
            set { _gamearena_index = value; }
        }

        public int gamelocation_index
        {
            get { return _gamelocation_index; }
            set { _gamelocation_index = value; }
        }

        public int gamestate_index
        {
            get { return _gamestate_index; }
            set { _gamestate_index = value; }
        }

        public int gamevisibility_index
        {
            get { return _gamevisibility_index; }
            set { _gamevisibility_index = value; }
        }

        public int gamepremium_index
        {
            get { return _gamepremium_index;  }
            set { _gamepremium_index = value; }
        }

        public bool gamepremium_bool
        {
            get { return _gamepremium_bool; }
            set { _gamepremium_bool = value; }
        }


        // load the most current filter list (downloaded from application's web server)
        private async void LoadFilterList()
        {
            try
            {
                // read filter json from filedir\data\currentfilters.json
                using (StreamReader sr = new StreamReader(UQLTFilepaths.currentfilterpath))
                {
                    Console.WriteLine("**DEBUG: Now reading filters from: " + UQLTFilepaths.currentfilterpath);
                    var x = await sr.ReadToEndAsync();
                    var filters = JsonConvert.DeserializeObject<ImportedFilters>(x);
                    // set combo boxes' itemsources to appropriate filter in code
                    /*
                    cboGameType.ItemsSource = filters.game_types;
                    cboGameArena.ItemsSource = filters.arenas;
                    cboGameLocation.ItemsSource = filters.active_locations;
                    cboGameState.ItemsSource = filters.gamestate;
                    */
                    // game visibility will never change, so it's hard-coded
                    /*
                    cboGameVisibility.Items.Add("Public games");
                    cboGameVisibility.Items.Add("Private games");
                    */

                    game_types = filters.game_types;
                    arenas = filters.arenas;
                    active_locations = filters.active_locations;
                    gamestate = filters.gamestate;

                    Console.WriteLine("**DEBUG: Gametypes: " + string.Join(",", game_types));

                    // add to appropriate list in order to set proper arena tag when building filter
                    foreach (var arena in filters.arenas)
                    {
                        if (arena.arena_type.Equals("map") && (!arenamap.Contains(arena.arena)))
                        {
                            arenamap.Add(arena.arena);
                        }
                        else if (arena.arena_type.Equals("tag") && (!arenatag.Contains(arena.arena)))
                        {
                            arenatag.Add(arena.arena);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to read filter data.");
                Console.WriteLine(ex);
                //TODO: make this download filter from net, if that fails THEN load hard-coded filter
                FailsafeFilterHelper failsafe = new FailsafeFilterHelper();
                failsafe.getFilterBackup();

            }
        }
        
        // set default filters (i.e.: "Any Location", "Any Game State", etc.)
        private void SetStandardDefaultFilters()
        {
            gametype_index = 0;
            gamearena_index = 0;
            gamelocation_index = 0;
            gamestate_index = 0;
            gamevisibility_index = 0;
            gamepremium_bool = false;
        }
        
        private void ApplySavedUserFilters()
        {
            try
            {
                using (StreamReader sr = new StreamReader(UQLTFilepaths.saveduserfilterpath))
                {
                    var x = sr.ReadToEnd();
                    var sfj = JsonConvert.DeserializeObject<SavedFilters>(x);
                    sf.gametype_index = sfj.gametype_index;
                    sf.gamearena_index = sfj.gamearena_index;
                    sf.gamelocation_index = sfj.gamelocation_index;
                    sf.gamestate_index = sfj.gamestate_index;
                    sf.gamevisibility_index = sfj.gamevisibility_index;
                    sf.gamepremium_index = sfj.gamepremium_index;

                    /*
                    cboGameType.SelectedIndex = sf.gametype_index;
                    cboGameArena.SelectedIndex = sf.gamearena_index;
                    cboGameLocation.SelectedIndex = sf.gamelocation_index;
                    cboGameState.SelectedIndex = sf.gamestate_index;
                    */

                    gametype_index = sf.gametype_index;
                    gamearena_index = sf.gamearena_index;
                    gamelocation_index = sf.gamelocation_index;
                    gamestate_index = sf.gamestate_index;

                    if (sf.gamevisibility_index == 0) { gamevisibility_index = 0; }
                    else { gamevisibility_index = 1; }
                    if (sf.gamepremium_index == 0) { gamepremium_bool = false; }
                    else { gamepremium_bool = true; }
                    //Console.WriteLine(sfj.saved_gamearena);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error applying saved filters: " + ex);
                ClearSavedUserFilters(null, null);
            }
        }

        // clear the user's saved default filters when "Clear Filters" button is clicked
        private void ClearSavedUserFilters(object sender, RoutedEventArgs e)
        {
            if (savedUserFiltersExist())
                File.Delete(UQLTFilepaths.saveduserfilterpath);
            SetStandardDefaultFilters();
            Console.WriteLine("Saved filters cleared!");
        }

        private void SaveNewUserFilters(int sgametype_index, int sgamearena_index, int sgamelocation_index, int sgamestate_index, int sgamevisibility_index, bool sgamepremium_bool)
        {
            //Console.WriteLine("sender: " + sender + "e: " + e);
            // Combo Box strings and visibility/prem ints, used for passing to makeFilterJson method
            /*
            selectedgametype = cboGameType.SelectedValue.ToString();
            String selectedgamearena = cboGameArena.SelectedValue.ToString();
            String selectedgamelocation = cboGameLocation.SelectedValue.ToString();
            String selectedgamestate = cboGameState.SelectedValue.ToString();
            int selectedgamevisibility = 0;
            int selectedgamepremium = 0;

            // Combo Box index numbers for making filter default for user
            int sgtypeindex = cboGameType.SelectedIndex;
            int sgarenaindex = cboGameArena.SelectedIndex;
            int sglocationindex = cboGameLocation.SelectedIndex;
            int sgstateindex = cboGameState.SelectedIndex;
            int sgvisibilityindex = cboGameVisibility.SelectedIndex;
            */

            // SelectedIndex 0: "Public games", 1: "Private games"
            //if (cboGameVisibility.SelectedIndex == 0) { selectedgamevisibility = 0; } // private = 0 
            //else if (cboGameVisibility.SelectedIndex == 1) { selectedgamevisibility = 1; } // private = 1

            int sgamepremium_index = 0;


            if (sgamepremium_bool == true) { sgamepremium_index = 1; }
            else if (sgamepremium_bool == false) { sgamepremium_index = 0; }

            //Console.WriteLine("[SELECTED VALUES]: Game type: {0} | Game Arena: {1} | Game Location: {2} | Game Status: {3} | Game Visibility: {4} | Game Premium: {5}",
            //selectedgametype, selectedgamearena, selectedgamelocation, selectedgamestate, selectedgamevisibility, selectedgamepremium);

            // Save this filter information as the new default
            sf.gametype_index = sgametype_index;
            sf.gamearena_index = sgamearena_index;
            sf.gamelocation_index = sgamelocation_index;
            sf.gamestate_index = sgamestate_index;
            sf.gamevisibility_index = sgamevisibility_index;
            sf.gamepremium_index = sgamepremium_index;

            // write to disk
            String savedfilterjson = JsonConvert.SerializeObject(sf);
            using (FileStream fs = File.Create(UQLTFilepaths.saveduserfilterpath))
            using (TextWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine(savedfilterjson);
            }

            // make filter url based on user selections
            //makeFilterJson(selectedgametype, selectedgamearena, selectedgamestate, selectedgamelocation, selectedgamevisibility, selectedgamepremium);

        }

    }
}
