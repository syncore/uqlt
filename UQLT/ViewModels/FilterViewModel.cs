using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using UQLT.Models;
using System.IO;
using UQLT.Helpers;
using UQLT.Events;
using Newtonsoft.Json;
using System.Windows;
using System.ComponentModel.Composition;

namespace UQLT.ViewModels
{
    [Export(typeof(FilterViewModel))]
    public class FilterViewModel : PropertyChangedBase, IHandle<FilterVisibilityEvent>
    {
        // list of maps that receive "tag" description for arena_type when building filter string
        static List<String> arenatag = new List<String>();
        // list of maps that receive "map" description for arena_type when building filter string
        static List<String> arenamap = new List<String>();

        SavedFilters sf = new SavedFilters();

        [ImportingConstructor]
        public FilterViewModel(IEventAggregator events)
        {
            events.Subscribe(this);

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
            // game visibility types don't depend on saved filter file
            _game_visibility.Add("Public games");
            _game_visibility.Add("Private games");

        }

        private bool savedUserFiltersExist()
        {
            return (File.Exists(UQLTGlobals.saveduserfilterpath)) ? true : false;
        }

        private bool downloadedFilterListExists()
        {
            return (File.Exists(UQLTGlobals.currentfilterpath)) ? true : false;
        }

        private bool _isVisible = true;
        public bool isVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; NotifyOfPropertyChange(() => isVisible); }
        }

        private List<ActiveLocation> _active_locations;
        public List<ActiveLocation> active_locations
        {
            get { return _active_locations; }
            set { _active_locations = value; NotifyOfPropertyChange(() => active_locations); }
        }

        private List<InactiveLocation> _inactive_locations;
        public List<InactiveLocation> inactive_locations
        {
            get { return _inactive_locations; }
            set { _inactive_locations = value; NotifyOfPropertyChange(() => inactive_locations); }
        }

        private List<ServerBrowserLocations> _serverbrowser_locations;
        public List<ServerBrowserLocations> serverbrowser_locations
        {
            get { return _serverbrowser_locations; }
            set { _serverbrowser_locations = value; NotifyOfPropertyChange(() => serverbrowser_locations); }
        }

        private List<Arena> _arenas;
        public List<Arena> arenas
        {
            get { return _arenas; }
            set { _arenas = value; NotifyOfPropertyChange(() => arenas); }
        }

        private List<Difficulty> _difficulty;
        public List<Difficulty> difficulty
        {
            get { return _difficulty; }
            set { _difficulty = value; NotifyOfPropertyChange(() => difficulty); }
        }

        private List<GameState> _gamestate;
        public List<GameState> gamestate
        {
            get { return _gamestate; }
            set { _gamestate = value; NotifyOfPropertyChange(() => gamestate); }
        }

        private List<GameType> _game_types;
        public List<GameType> game_types
        {
            get { return _game_types; }
            set { _game_types = value; NotifyOfPropertyChange(() => game_types); }
        }

        private List<GameInfo> _game_info;
        public List<GameInfo> game_info
        {
            get { return _game_info; }
            set { _game_info = value; NotifyOfPropertyChange(() => game_info); }
        }

        private List<String> _game_visibility = new List<String>();
        public List<String> game_visibility
        {
            get { return _game_visibility; }
        }

        private int _gametype_index;
        public int gametype_index
        {
            get { return _gametype_index; }
            set { _gametype_index = value; NotifyOfPropertyChange(() => gametype_index); }
        }

        private int _gamearena_index;
        public int gamearena_index
        {
            get { return _gamearena_index; }
            set { _gamearena_index = value; NotifyOfPropertyChange(() => gamearena_index); }
        }

        private int _gamelocation_index;
        public int gamelocation_index
        {
            get { return _gamelocation_index; }
            set { _gamelocation_index = value; NotifyOfPropertyChange(() => gamelocation_index); }
        }

        private int _gamestate_index;
        public int gamestate_index
        {
            get { return _gamestate_index; }
            set { _gamestate_index = value; NotifyOfPropertyChange(() => gamestate_index); }
        }

        private int _gamevisibility_index;
        public int gamevisibility_index
        {
            get { return _gamevisibility_index; }
            set { _gamevisibility_index = value; NotifyOfPropertyChange(() => gamevisibility_index); }
        }

        private int _gamepremium_index;
        public int gamepremium_index
        {
            get { return _gamepremium_index; }
            set { _gamepremium_index = value; NotifyOfPropertyChange(() => gamepremium_index); }
        }

        private bool _gamepremium_bool;
        public bool gamepremium_bool
        {
            get { return _gamepremium_bool; }
            set { _gamepremium_bool = value; NotifyOfPropertyChange(() => gamepremium_bool); }
        }

        public void Handle(FilterVisibilityEvent message)
        {
            isVisible = message.FilterViewVisibility;
            //Console.WriteLine("Visibility: " + message.FilterViewVisibility);
        }

        // load the most current filter list (downloaded from application's web server)
        private async void LoadFilterList()
        {
            try
            {
                // read filter json from filedir\data\currentfilters.json
                using (StreamReader sr = new StreamReader(UQLTGlobals.currentfilterpath))
                {
                    var x = await sr.ReadToEndAsync();
                    var filters = JsonConvert.DeserializeObject<ImportedFilters>(x);

                    // set our viewmodel's properties to those in the model
                    game_types = filters.game_types;
                    arenas = filters.arenas;
                    active_locations = filters.active_locations;
                    gamestate = filters.gamestate;

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
                using (StreamReader sr = new StreamReader(UQLTGlobals.saveduserfilterpath))
                {
                    String savedblob = sr.ReadToEnd();
                    var savedFilterJson = JsonConvert.DeserializeObject<SavedFilters>(savedblob);
                    sf.gametype_index = savedFilterJson.gametype_index;
                    sf.gamearena_index = savedFilterJson.gamearena_index;
                    sf.gamelocation_index = savedFilterJson.gamelocation_index;
                    sf.gamestate_index = savedFilterJson.gamestate_index;
                    sf.gamevisibility_index = savedFilterJson.gamevisibility_index;
                    sf.gamepremium_index = savedFilterJson.gamepremium_index;


                    // set our viewmodel's index properties to appropriate properties from saved filter file
                    gametype_index = sf.gametype_index;
                    gamearena_index = sf.gamearena_index;
                    gamelocation_index = sf.gamelocation_index;
                    gamestate_index = sf.gamestate_index;

                    if (sf.gamevisibility_index == 0) { gamevisibility_index = 0; }
                    else { gamevisibility_index = 1; }
                    if (sf.gamepremium_index == 0) { gamepremium_bool = false; }
                    else { gamepremium_bool = true; }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error applying saved filters: " + ex);
                ClearSavedUserFilters();
            }
        }

        // clear the user's saved default filters when "Clear Filters" button is clicked
        public void ClearSavedUserFilters()
        {
            if (savedUserFiltersExist())
                File.Delete(UQLTGlobals.saveduserfilterpath);
            SetStandardDefaultFilters();
            Console.WriteLine("Saved filters cleared!");
        }

        public void SaveNewUserFilters(int sgametype_index, int sgamearena_index, int sgamelocation_index, int sgamestate_index, int sgamevisibility_index, bool sgamepremium_bool)
        {
            int sgamepremium_index = 0;
            if (sgamepremium_bool == true) { sgamepremium_index = 1; }
            else if (sgamepremium_bool == false) { sgamepremium_index = 0; }

            // Save this filter information as the new default
            sf.gametype_index = sgametype_index;
            sf.gamearena_index = sgamearena_index;
            sf.gamelocation_index = sgamelocation_index;
            sf.gamestate_index = sgamestate_index;
            sf.gamevisibility_index = sgamevisibility_index;
            sf.gamepremium_index = sgamepremium_index;

            // write to disk
            String savedfilterjson = JsonConvert.SerializeObject(sf);
            using (FileStream fs = File.Create(UQLTGlobals.saveduserfilterpath))
            using (TextWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine(savedfilterjson);
            }

            // make filter url based on user selections
            //makeFilterJson(selectedgametype, selectedgamearena, selectedgamestate, selectedgamelocation, selectedgamevisibility, selectedgamepremium);

        }

        // take the output from the filter menu, process it, and return a quakelive.com url that includes base64 encoded filter json
        public String MakeFilterUrl(String gametype, String arena, String state, Object location, Object priv, bool ispremium)
        {
            String encodedFilterUrl = null;
            String jsonFilterString = null;
            List<String> players = new List<String>(); // always empty for purposes of filter encoding

            // arena_type is determined from arena. ig, game_types array, and ranked are determined from gametype
            String arena_type = null;
            Object ig = null;
            List<int> gtarr = null;
            Object ranked = null;
            int premium = 0;

            if (ispremium == true) { premium = 1; }
            else { premium = 0; }

            // arena_type determination from arena:
            if (arena.Equals("any")) { arena_type = ""; }

            if (arenamap.Contains(arena)) { arena_type = "map"; }
            else if (arenatag.Contains(arena)) { arena_type = "tag"; }

            Console.WriteLine("Arena type for " + arena + " is: " + arena_type);

            // ig, game_types array, ranked determination from gametype:
            try
            {
                // read filter json from filedir\data\currentfilters.json
                using (StreamReader sr = new StreamReader(UQLTGlobals.currentfilterpath))
                {
                    var x = sr.ReadToEnd();
                    var gi = JsonConvert.DeserializeObject<ImportedFilters>(x);

                    // get appropriate ig, game_types array, and ranked for gametype
                    foreach (var g in gi.game_info)
                    {
                        if (g.type.Equals(gametype))
                        {
                            ig = g.ig;
                            gtarr = g.gtarr;
                            ranked = g.ranked;
                        }
                    }

                }

                // create objects to be converted to json
                QuakeLiveFilters qlf = new QuakeLiveFilters
                {
                    group = "any", //hard-code
                    game_type = gametype,
                    arena = arena,
                    state = state,
                    difficulty = "any", //hard-code
                    location = location,
                    @private = priv,
                    premium_only = premium,
                    ranked = ranked,
                    invitation_only = 0, //hard-code
                };

                QuakeLiveFilterObject qlfo = new QuakeLiveFilterObject
                {
                    filters = qlf,
                    arena_type = arena_type,
                    players = players,
                    game_types = gtarr,
                    ig = ig
                };

                // convert to json
                jsonFilterString = JsonConvert.SerializeObject(qlfo);

                // format url and base64 encode the json
                encodedFilterUrl = "http://www.quakelive.com/browser/list?filter=" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonFilterString)) + "&_="
                    + Math.Truncate((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);

                Console.WriteLine("Arena ({0}) type: {1} | Gametype ({2}) ig: {3} | game_types: {4} | ranked: {5}", arena, arena_type, gametype, ig, string.Join(", ", gtarr), ranked);
                Console.WriteLine(jsonFilterString);
                Console.WriteLine(encodedFilterUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to read filter data. Error: " + ex);
            }
            return encodedFilterUrl;
        }

    }
}
