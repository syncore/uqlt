using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UQLT;
using UQLT.Events;
using UQLT.Helpers;
using UQLT.Models.Filters;
using UQLT.Models.Filters.Remote;
using UQLT.Models.Filters.User;

namespace UQLT.ViewModels
{
    [Export(typeof(FilterViewModel))]
    public class FilterViewModel : PropertyChangedBase, IHandle<FilterVisibilityEvent>
    {
        // list of maps that receive "tag" description for arena_type when building filter string
        private static List<string> arenaTag = new List<string>();

        // list of maps that receive "map" description for arena_type when building filter string
        private static List<string> arenaMap = new List<string>();

        private readonly IEventAggregator _events;
        private SavedFilters sf = new SavedFilters();
        
        private bool _isVisible = true;

        public bool IsVisible
        {
            get 
            { 
                return _isVisible;
            }
            
            set
            { 
                _isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        private List<ActiveLocation> _activeLocations;

        public List<ActiveLocation> ActiveLocations
        {
            get 
            { 
                return _activeLocations;
            }
            
            set
            { 
                _activeLocations = value; 
                NotifyOfPropertyChange(() => ActiveLocations);
            }
        }

        private List<InactiveLocation> _inactiveLocations;

        public List<InactiveLocation> InactiveLocations
        {
            get 
            { 
                return _inactiveLocations;
            }
            
            set
            { 
                _inactiveLocations = value;
                NotifyOfPropertyChange(() => InactiveLocations);
            }
        }

        private List<ServerBrowserLocations> _serverBrowserLocations;

        public List<ServerBrowserLocations> ServerBrowserLocations
        {
            get 
            { 
                return _serverBrowserLocations;
            }
            
            set
            { 
                _serverBrowserLocations = value;
                NotifyOfPropertyChange(() => ServerBrowserLocations);
            }
        }

        private List<Arena> _arenas;

        public List<Arena> Arenas
        {
            get 
            { 
                return _arenas; 
            }
            
            set
            { 
                _arenas = value;
                NotifyOfPropertyChange(() => Arenas);
            }
        }

        private List<Difficulty> _difficulty;

        public List<Difficulty> Difficulty
        {
            get 
            { 
                return _difficulty;
            }
            
            set
            { 
                _difficulty = value;
                NotifyOfPropertyChange(() => Difficulty);
            }
        }

        private List<GameState> _gamestate;

        public List<GameState> GameState
        {
            get 
            { 
                return _gamestate;
            }
            
            set
            { 
                _gamestate = value;
                NotifyOfPropertyChange(() => GameState);
            }
        }

        private List<GameType> _gameTypes;

        public List<GameType> GameTypes
        {
            get 
            { 
                return _gameTypes;
            }
            
            set
            { 
                _gameTypes = value;
                NotifyOfPropertyChange(() => GameTypes);
            }
        }

        private List<GameInfo> _gameInfo;

        public List<GameInfo> GameInfo
        {
            get 
            { 
                return _gameInfo;
            }
            
            set
            {
                _gameInfo = value;
                NotifyOfPropertyChange(() => GameInfo);
            }
        }

        private List<string> _gameVisibility = new List<string>();

        public List<string> GameVisibility
        {
            get 
            { 
                return _gameVisibility;
            }
        }

        private int _gameTypeIndex;

        public int GameTypeIndex
        {
            get 
            { 
                return _gameTypeIndex;
            }
            
            set
            { 
                _gameTypeIndex = value;
                NotifyOfPropertyChange(() => GameTypeIndex);
            }
        }

        private int _gameArenaIndex;

        public int GameArenaIndex
        {
            get 
            { 
                return _gameArenaIndex;
            }
            
            set
            { 
                _gameArenaIndex = value;
                NotifyOfPropertyChange(() => GameArenaIndex);
            }
        }

        private int _gameLocationIndex;

        public int GameLocationIndex
        {
            get 
            { 
                return _gameLocationIndex;
            }
            
            set
            { 
                _gameLocationIndex = value;
                NotifyOfPropertyChange(() => GameLocationIndex);
            }
        }

        private int _gameStateIndex;

        public int GameStateIndex
        {
            get 
            { 
                return _gameStateIndex;
            }
            
            set
            { 
                _gameStateIndex = value;
                NotifyOfPropertyChange(() => GameStateIndex);
            }
        }

        private int _gameVisibilityIndex;

        public int GameVisibilityIndex
        {
            get 
            { 
                return _gameVisibilityIndex;
            }
            
            set 
            { 
                _gameVisibilityIndex = value;
                NotifyOfPropertyChange(() => GameVisibilityIndex);
            }
        }

        private int _gamePremiumIndex;

        public int GamePremiumIndex
        {
            get 
            { 
                return _gamePremiumIndex;
            }
            
            set
            { 
                _gamePremiumIndex = value;
                NotifyOfPropertyChange(() => GamePremiumIndex);
            }
        }

        private bool _gamePremiumBool;

        public bool GamePremiumBool
        {
            get 
            { 
                return _gamePremiumBool;
            }
            
            set
            { 
                _gamePremiumBool = value;
                NotifyOfPropertyChange(() => GamePremiumBool);
            }
        }
        [ImportingConstructor]
        public FilterViewModel(IEventAggregator events)
        {
            _events = events;
            events.Subscribe(this);

            // load hard-coded fail-safe filters if downloaded list doesn't exist
            if (!DownloadedFilterListExists())
            {
                FailsafeFilterHelper failsafe = new FailsafeFilterHelper();
                failsafe.DumpBackupFilters();
                PopulateFilters();

                if (SavedUserFiltersExist())
                {
                    ApplySavedUserFilters();
                }
                else
                {
                    SetStandardDefaultFilters();
                }
            }
            else
            {
                PopulateFilters();

                if (SavedUserFiltersExist())
                {
                    ApplySavedUserFilters();
                }
                else
                {
                    SetStandardDefaultFilters();
                }
            }

            // game visibility types don't depend on saved filter file
            _gameVisibility.Add("Public games");
            _gameVisibility.Add("Private games");
        }

        public void Handle(FilterVisibilityEvent message)
        {
            IsVisible = message.FilterViewVisibility;
            
            // Console.WriteLine("Visibility: " + message.FilterViewVisibility);
        }

        // clear the user's saved default filters when "Clear Filters" button is clicked
        public void ClearSavedUserFilters()
        {
            if (SavedUserFiltersExist())
            {
                File.Delete(UQLTGlobals.SavedUserFilterPath);
            }
            
            SetStandardDefaultFilters();
            SetServerBrowserUrl(UQLTGlobals.QLDefaultFilter);
            Console.WriteLine("Saved filters cleared!");
        }

        public void SaveNewUserFilters(int selGameTypeIndex, int selGameArenaIndex, int selGameLocationIndex, int selGameStateIndex, int selGameVisibilityIndex, bool selGamePremiumBool)
        {
            int selGamePremiumIndex = 0;
            if (selGamePremiumBool == true)
            {
                selGamePremiumIndex = 1;
            }
            else if (selGamePremiumBool == false)
            {
                selGamePremiumIndex = 0;
            }

            // Save this filter information as the new default
            sf.type_in = selGameTypeIndex;
            sf.arena_in = selGameArenaIndex;
            sf.location_in = selGameLocationIndex;
            sf.state_in = selGameStateIndex;
            sf.visibility_in = selGameVisibilityIndex;
            sf.premium_in = selGamePremiumIndex;
            sf.fltr_enc = MakeEncFilterFromIndexes(selGameTypeIndex, selGameArenaIndex, selGameLocationIndex, selGameStateIndex, selGameVisibilityIndex, selGamePremiumBool);

            // write to disk
            string savedfilterjson = JsonConvert.SerializeObject(sf);
            using (FileStream fs = File.Create(UQLTGlobals.SavedUserFilterPath))
            using (TextWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine(savedfilterjson);
            }

            // make filter url based on user selections
            // makeFilterJson(selectedgametype, selectedgamearena, selectedgamestate, selectedgamelocation, selectedgamevisibility, selectedgamepremium);
        }

        // take the output from the filter menu, process it, and return a quakelive.com url that includes base64 encoded filter json
        public string MakeEncodedFilter(string gametype, string arena, string state, object location, object priv, bool ispremium)
        {
            string encodedFilter = null;
            string jsonFilterString = null;
            List<string> players = new List<string>(); // always empty for purposes of filter encoding

            // arena_type is determined from arena. ig, GameTypes array, and ranked are determined from gametype
            string arena_type = null;
            object ig = null;
            List<int> gtarr = null;
            object ranked = null;
            int premium = 0;

            if (ispremium == true)
            { 
                premium = 1;
            }
            else
            {
                premium = 0;
            }

            // arena_type determination from arena:
            if (arena.Equals("any"))
            { 
                arena_type = string.Empty;
            }

            if (arenaMap.Contains(arena))
            { 
                arena_type = "map";
            }
            else if (arenaTag.Contains(arena)) 
            { 
                arena_type = "tag";
            }

            Console.WriteLine("Arena type for " + arena + " is: " + arena_type);

            // ig, GameTypes array, ranked determination from gametype:
            try
            {
                // read filter json from filedir\data\currentfilters.json
                using (StreamReader sr = new StreamReader(UQLTGlobals.CurrentFilterPath))
                {
                    var x = sr.ReadToEnd();
                    var gi = JsonConvert.DeserializeObject<ImportedFilters>(x);

                    // get appropriate ig, GameTypes array, and ranked for gametype
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
                FilterBuilderDetails fbd = new FilterBuilderDetails
                {
                    group = "any", // hard-code
                    game_type = gametype,
                    arena = arena,
                    state = state,
                    difficulty = "any", // hard-code
                    location = location,
                    @private = priv,
                    premium_only = premium,
                    ranked = ranked,
                    invitation_only = 0, // hard-code
                };

                FilterBuilderObject fbo = new FilterBuilderObject
                {
                    filters = fbd,
                    arena_type = arena_type,
                    players = players,
                    game_types = gtarr,
                    ig = ig
                };

                // convert to json
                jsonFilterString = JsonConvert.SerializeObject(fbo);

                // base64 encode the json
                encodedFilter = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonFilterString))+"&_=";

                Console.WriteLine("Arena ({0}) type: {1} | Gametype ({2}) ig: {3} | GameTypes: {4} | ranked: {5}", arena, arena_type, gametype, ig, string.Join(", ", gtarr), ranked);
                Console.WriteLine(jsonFilterString);
                Console.WriteLine(encodedFilter);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to read filter data. Error: " + ex);
            }
            
            // fire event to server browser
            SetServerBrowserUrl(UQLTGlobals.QLDomainListFilter+encodedFilter);
            return encodedFilter;
        }

        public void SetServerBrowserUrl(string url)
        {
            // Console.WriteLine("Attempting to publish event");
            _events.Publish(new ServerRequestEvent(url));
        }

        private bool SavedUserFiltersExist()
        {
            return File.Exists(UQLTGlobals.SavedUserFilterPath) ? true : false;
        }

        private bool DownloadedFilterListExists()
        {
            return File.Exists(UQLTGlobals.CurrentFilterPath) ? true : false;
        }
        
        // load the most current filter list (downloaded from application's web server)
        private async void PopulateFilters()
        {
            try
            {
                // read filter json from filedir\data\currentfilters.json
                using (StreamReader sr = new StreamReader(UQLTGlobals.CurrentFilterPath))
                {
                    var x = await sr.ReadToEndAsync();
                    var filters = JsonConvert.DeserializeObject<ImportedFilters>(x);

                    // set our viewmodel's properties to those in the model
                    GameTypes = filters.game_types;
                    Arenas = filters.arenas;
                    ActiveLocations = filters.active_locations;
                    GameState = filters.gamestate;

                    // add to appropriate list in order to set proper arena tag when building filter
                    foreach (var arena in filters.arenas)
                    {
                        if (arena.arena_type.Equals("map") && (!arenaMap.Contains(arena.arena)))
                        {
                            arenaMap.Add(arena.arena);
                        }
                        else if (arena.arena_type.Equals("tag") && (!arenaTag.Contains(arena.arena)))
                        {
                            arenaTag.Add(arena.arena);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to read filter data.");
                Console.WriteLine(ex);

                // TODO: make this download filter from net, if that fails THEN load hard-coded filter
                FailsafeFilterHelper failsafe = new FailsafeFilterHelper();
                failsafe.DumpBackupFilters();
            }
        }

        // set default filters (i.e.: "Any Location", "Any Game State", etc.)
        private void SetStandardDefaultFilters()
        {
            GameTypeIndex = 0;
            GameArenaIndex = 0;
            GameLocationIndex = 1;
            GameStateIndex = 0;
            GameVisibilityIndex = 0;
            GamePremiumBool = false;

            // default url here
        }

        private void ApplySavedUserFilters()
        {
            try
            {
                using (StreamReader sr = new StreamReader(UQLTGlobals.SavedUserFilterPath))
                {
                    string savedblob = sr.ReadToEnd();
                    SavedFilters savedFilterJson = JsonConvert.DeserializeObject<SavedFilters>(savedblob);
                    sf.type_in = savedFilterJson.type_in;
                    sf.arena_in = savedFilterJson.arena_in;
                    sf.location_in = savedFilterJson.location_in;
                    sf.state_in = savedFilterJson.state_in;
                    sf.visibility_in = savedFilterJson.visibility_in;
                    sf.premium_in = savedFilterJson.premium_in;

                    // set our viewmodel's index properties to appropriate properties from saved filter file
                    GameTypeIndex = sf.type_in;
                    GameArenaIndex = sf.arena_in;
                    GameLocationIndex = sf.location_in;
                    GameStateIndex = sf.state_in;

                    if (sf.visibility_in == 0)
                    { 
                        GameVisibilityIndex = 0;
                    }
                    else
                    { 
                        GameVisibilityIndex = 1;
                    }

                    if (sf.premium_in == 0)
                    {
                        GamePremiumBool = false;
                    }
                    else
                    { 
                        GamePremiumBool = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error applying saved filters: " + ex);
                ClearSavedUserFilters();
            }
        }

        private string MakeEncFilterFromIndexes(int gtIndex, int arIndex, int locIndex, int statIndex, int visIndex, bool premBool)
        {
            string gt = null;
            string ar = null;
            string stat = null;
            object loc = null;

            try
            {
                using (StreamReader sr = new StreamReader(UQLTGlobals.CurrentFilterPath))
                {
                    string currentblob = sr.ReadToEnd();
                    ImportedFilters importedFilterJson = JsonConvert.DeserializeObject<ImportedFilters>(currentblob);
                    /*
                    for (int i = 0; i < importedFilterJson.GameTypes.Count; i++)
                    {
                        if (i == gt_index)
                        {
                            Console.WriteLine("FOUND: " + gt_index);
                            Console.WriteLine(importedFilterJson.GameTypes.ElementAt(gt_index));
                        }
                    }
                    */

                    foreach (var gametype in importedFilterJson.game_types)
                    {
                        if (importedFilterJson.game_types.ElementAt(gtIndex).ToString().Equals(gametype.display_name))
                        {
                            Console.WriteLine("Gametype: " + gametype.game_type);
                            gt = gametype.game_type;
                        }
                    }

                    foreach (var arena in importedFilterJson.arenas)
                    {
                        if (importedFilterJson.arenas.ElementAt(arIndex).ToString().Equals(arena.display_name))
                        {
                            Console.WriteLine("Arena Type: " + arena.arena_type);
                            Console.WriteLine("Arena: " + arena.arena);
                            ar = arena.arena;
                        }
                    }

                    foreach (var location in importedFilterJson.active_locations)
                    {
                        if (importedFilterJson.active_locations.ElementAt(locIndex).ToString().Equals(location.display_name))
                        {
                            Console.WriteLine("Location: " + location.location_id);
                            loc = location.location_id;
                        }
                    }

                    foreach (var gamestate in importedFilterJson.gamestate)
                    {
                        if (importedFilterJson.gamestate.ElementAt(statIndex).ToString().Equals(gamestate.display_name))
                        {
                            Console.WriteLine("Gamestate: " + gamestate.state);
                            stat = gamestate.state;
                        }
                    }

                    // just pass vis_ind to MakeFilterUrl no need to loop
                    // just pass prem_bool to MakeFilterUrl no need to convert
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating url from filters: " + ex);
            }

            return MakeEncodedFilter(gt, ar, stat, loc, visIndex, premBool);
        }
    }
}
