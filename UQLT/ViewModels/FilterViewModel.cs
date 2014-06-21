using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Events;
using UQLT.Helpers;
using UQLT.Models.Filters.Remote;
using UQLT.Models.Filters.User;

namespace UQLT.ViewModels
{
    /// <summary>
    /// Viewmodel for the filter menu
    /// </summary>
    [Export(typeof(FilterViewModel))]
    public class FilterViewModel : PropertyChangedBase, IHandle<FilterVisibilityEvent>
    {
        // A List of maps that receive "map" description for arena_type when building filter string.
        private static readonly List<string> ArenaMap = new List<string>();

        // A list of maps that receive "tag" description for arena_type when building filter string.
        private static readonly List<string> ArenaTag = new List<string>();

        private readonly IEventAggregator _events;

        // Loading filters: game visibility types don't depend on saved filter file
        private readonly List<string> _gameVisibility = new List<string> { "Public games", "Private games", "Invite-only games" };

        private List<Arena> _arenas;
        private List<Difficulty> _difficulty;
        private int _gameArenaIndex;
        private List<GameInfo> _gameInfo;
        private int _gameLocationIndex;
        private bool _gamePremiumBool;
        private int _gamePremiumIndex;
        private List<GameState> _gameState;
        private int _gameStateIndex;
        private int _gameTypeIndex;
        private List<GameType> _gameTypes;
        private int _gameVisibilityIndex;
        private bool _isVisible = true;
        private List<Location> _locations;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterViewModel" /> class.
        /// </summary>
        /// <param name="events">The events that this viewmodel publishes/subscribes to.</param>
        [ImportingConstructor]
        public FilterViewModel(IEventAggregator events)
        {
            _events = events;
            events.Subscribe(this);

            //TODO: implement downloading of filter list functionality
            // Async: suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
            var p = PopulateAndApplyFiltersAsync();
        }

        /// <summary>
        /// Gets or sets the arenas to be used in the view.
        /// </summary>
        /// <value>The arenas.</value>
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

        /// <summary>
        /// Gets or sets the difficulties to be displayed in the view.
        /// </summary>
        /// <value>The difficulties.</value>
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

        /// <summary>
        /// Gets or sets the index of the game arena.
        /// </summary>
        /// <value>The index of the game arena.</value>
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

        /// <summary>
        /// Gets or sets the game information.
        /// </summary>
        /// <value>The game information.</value>
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

        /// <summary>
        /// Gets or sets the index of the game location.
        /// </summary>
        /// <value>The index of the game location.</value>
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

        /// <summary>
        /// Gets or sets a value indicating whether the game is a premium game or not.
        /// </summary>
        /// <value><c>true</c> if game is premium; otherwise, <c>false</c>.</value>
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

        /// <summary>
        /// Gets or sets the index of the game premium.
        /// </summary>
        /// <value>The index of the game premium.</value>
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

        /// <summary>
        /// Gets or sets the gamestates to be displayed in the view.
        /// </summary>
        /// <value>The gamestates.</value>
        public List<GameState> GameState
        {
            get
            {
                return _gameState;
            }

            set
            {
                _gameState = value;
                NotifyOfPropertyChange(() => GameState);
            }
        }

        /// <summary>
        /// Gets or sets the index of the game state.
        /// </summary>
        /// <value>The index of the game state.</value>
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

        /// <summary>
        /// Gets or sets the index of the game type.
        /// </summary>
        /// <value>The index of the game type.</value>
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

        /// <summary>
        /// Gets or sets the game types to be displayed in the view.
        /// </summary>
        /// <value>The game types.</value>
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

        /// <summary>
        /// Gets the game visibilities to be displayed in the view.
        /// </summary>
        /// <value>The game visibility.</value>
        public List<string> GameVisibility
        {
            get
            {
                return _gameVisibility;
            }
        }

        /// <summary>
        /// Gets or sets the index of the game visibility.
        /// </summary>
        /// <value>The index of the game visibility.</value>
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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is visible.
        /// </summary>
        /// <value><c>true</c> if this instance is visible; otherwise, <c>false</c>.</value>
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

        /// <summary>
        /// Gets or sets the locations to be used in the view.
        /// </summary>
        /// <value>The locations.</value>
        public List<Location> Locations
        {
            get
            {
                return _locations;
            }

            set
            {
                _locations = value;
                NotifyOfPropertyChange(() => Locations);
            }
        }

        /// <summary>
        /// Clears the saved user filters.
        /// </summary>
        public void ClearSavedUserFilters()
        {
            if (SavedUserFiltersExist())
            {
                File.Delete(UQltGlobals.SavedUserFilterPath);
            }

            ApplyDefaultFilters();
            SetServerBrowserUrl(UQltGlobals.QlDefaultFilter);
            Debug.WriteLine("Saved filters cleared!");
        }

        /// <summary>
        /// Handles the specified message (event).
        /// </summary>
        /// <param name="message">The message (event)</param>
        public void Handle(FilterVisibilityEvent message)
        {
            IsVisible = message.FilterViewVisibility;
            // Debug.WriteLine("Visibility: " + message.FilterViewVisibility);
        }

        /// <summary>
        /// Makes the encoded filter. This takes the output from the filter menu, processes it, and
        /// returns base64 encoded filter json.
        /// </summary>
        /// <param name="gametype">The gametype.</param>
        /// <param name="arena">The arena.</param>
        /// <param name="state">The state.</param>
        /// <param name="location">The location.</param>
        /// <param name="priv">Whether the game is private</param>
        /// <param name="ispremium">
        /// if set to <c>true</c> then the game is premium, if <c>false</c> then it is not premium.
        /// </param>
        /// <returns>Base64 encoded json filter data.</returns>
        /// <remarks><c>object priv</c> is actually the visibility index</remarks>
        public string MakeEncodedFilter(string gametype, string arena, string state, object location, object priv, bool ispremium)
        {
            string encodedFilter = null;
            string jsonFilterString = null;
            // players is always empty for purposes of filter encoding
            var players = new List<string>();

            // arena_type is determined from arena. ig, GameTypes array, and ranked are determined
            // from gametype
            string arena_type = null;
            object ig = null;
            List<int> gtarr = null;
            object ranked = null;
            int premium = 0;
            int invitation = 0;

            if (ispremium)
            {
                premium = 1;
            }
            else
            {
                premium = 0;
            }

            // invitation-only determination from priv (visibility index)
            if ((int)priv == 2)
            {
                invitation = 1;
            }

            // arena_type determination from arena:
            if (arena.Equals("any"))
            {
                arena_type = string.Empty;
            }

            if (ArenaMap.Contains(arena))
            {
                arena_type = "map";
            }
            else if (ArenaTag.Contains(arena))
            {
                arena_type = "tag";
            }

            Debug.WriteLine("Arena type for " + arena + " is: " + arena_type);

            // ig, GameTypes array, ranked determination from gametype:
            try
            {
                // read filter json from filedir\data\currentfilters.json
                using (var sr = new StreamReader(UQltGlobals.CurrentFilterPath))
                {
                    var json = sr.ReadToEnd();
                    var gi = JsonConvert.DeserializeObject<ImportedFilters>(json);

                    // get appropriate ig, GameTypes array, and ranked for gametype
                    foreach (var g in gi.game_info.Where(g => g.type.Equals(gametype)))
                    {
                        ig = g.ig;
                        gtarr = g.gtarr;
                        ranked = g.ranked;
                    }
                }

                // create objects to be converted to json
                var fbd = new FilterBuilderDetails
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
                    invitation_only = invitation,
                };

                var fbo = new FilterBuilderObject
                {
                    filters = fbd,
                    arena_type = arena_type,
                    players = players,
                    game_types = gtarr,
                    ig = ig
                };

                // Convert to json
                jsonFilterString = JsonConvert.SerializeObject(fbo);

                // base64 encode the json
                encodedFilter = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonFilterString)) + "&_=";

                Debug.WriteLine("Arena ({0}) type: {1} | Gametype ({2}) ig: {3} | GameTypes: {4} | ranked: {5}", arena, arena_type, gametype, ig, string.Join(", ", gtarr), ranked);
                Debug.WriteLine(jsonFilterString);
                Debug.WriteLine(encodedFilter);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to read filter data. Error: " + ex);
            }

            // Fire event to server browser
            SetServerBrowserUrl(UQltGlobals.QlDomainListFilter + encodedFilter);
            return encodedFilter;
        }

        /// <summary>
        /// Saves the new user filters. This makes the current selections in the view's comboboxes
        /// (or from the filter file, depending on the caller) the new default filters.
        /// </summary>
        /// <param name="selGameTypeIndex">Index of the selected game type.</param>
        /// <param name="selGameArenaIndex">Index of the selected game arena.</param>
        /// <param name="selGameLocationIndex">Index of the selected game location.</param>
        /// <param name="selGameStateIndex">Index of the selected game state.</param>
        /// <param name="selGameVisibilityIndex">Index of the selected game visibility.</param>
        /// <param name="selGamePremiumBool">if set to <c>true</c> [selected game premium bool].</param>
        public void SaveNewUserFilters(int selGameTypeIndex, int selGameArenaIndex, int selGameLocationIndex, int selGameStateIndex, int selGameVisibilityIndex, bool selGamePremiumBool)
        {
            int selGamePremiumIndex = selGamePremiumBool ? 1 : 0;

            // Filter to object for json file
            var sf = new SavedFilters
            {
                type_in = selGameTypeIndex,
                arena_in = selGameArenaIndex,
                location_in = selGameLocationIndex,
                state_in = selGameStateIndex,
                visibility_in = selGameVisibilityIndex,
                premium_in = selGamePremiumIndex,
                fltr_enc = ReceiveIndexesForEncoding(selGameTypeIndex, selGameArenaIndex, selGameLocationIndex, selGameStateIndex, selGameVisibilityIndex, selGamePremiumBool)
            };

            // Write json file to disk
            string savedfilterjson = JsonConvert.SerializeObject(sf);
            using (FileStream fs = File.Create(UQltGlobals.SavedUserFilterPath))
            using (TextWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine(savedfilterjson);
            }
        }

        /// <summary>
        /// Sets the filter status text. This will fire a message (event) to the MainViewModel so
        /// that the status bar can display the correct filter text.
        /// </summary>
        /// <param name="gameTypeIndex">The currently selected gametype index.</param>
        /// <param name="gameArenaIndex">The currently selected game arena index.</param>
        /// <param name="gameLocationIndex">The currently selected game location index.</param>
        /// <param name="gameStateIndex">The currently selected game state index.</param>
        /// <param name="gameVisibilityIndex">The currently selected game visibility index.</param>
        /// <param name="gamePremium">
        /// if set to <c>true</c> then it's a premium game, if not then <c>false</c>.
        /// </param>
        /// <remarks>
        /// It is called from the view itself. Unfortunately, indexes must be used since the
        /// ValuePath in the view is already set.
        /// </remarks>
        public void SetFilterStatusText(int gameTypeIndex, int gameArenaIndex, int gameLocationIndex, int gameStateIndex, int gameVisibilityIndex, bool gamePremium)
        {
            string gametype, gamearena, gamelocation, gamestate, gamevisibility, premium;

            gametype = GameTypes[gameTypeIndex].display_name;
            gamearena = Arenas[gameArenaIndex].display_name;
            gamelocation = Locations[gameLocationIndex].display_name;
            gamestate = GameState[gameStateIndex].display_name;
            gamevisibility = GameVisibility[gameVisibilityIndex];

            if (gamePremium)
            {
                premium = "Premium";
            }
            else
            {
                premium = "Non-premium";
            }

            Debug.WriteLine("Sending filter status text information to Main VM: {0} {1} {2} {3} {4} {5}", gametype, gamearena, gamelocation, gamestate, gamevisibility, premium);
            _events.Publish(new FilterStatusEvent(gametype, gamearena, gamelocation, gamestate, gamevisibility, premium));
        }

        /// <summary>
        /// Sets the server browser URL. This fires a message (event) to the Server Browser
        /// ViewModel notifying it that a new filter URL has been set.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void SetServerBrowserUrl(string url)
        {
            _events.Publish(new ServerRequestEvent(url));
        }

        /// <summary>
        /// Sets the standard filters (i.e.: "Any Location", "Any Game State", etc.) and save them
        /// as the new default.
        /// </summary>
        private void ApplyDefaultFilters()
        {
            GameTypeIndex = 0;
            GameArenaIndex = 0;
            GameLocationIndex = 0;
            GameStateIndex = 0;
            GameVisibilityIndex = 0;
            GamePremiumBool = false;
            SetFilterStatusText(GameTypeIndex, GameArenaIndex, GameLocationIndex, GameStateIndex, GameVisibilityIndex, GamePremiumBool);
        }

        /// <summary>
        /// Applies the saved user filters, if they exist. Otherwise sets standard default filters.
        /// </summary>
        private void ApplySavedUserFilters()
        {
            try
            {
                using (var sr = new StreamReader(UQltGlobals.SavedUserFilterPath))
                {
                    string savedblob = sr.ReadToEnd();
                    var json = JsonConvert.DeserializeObject<SavedFilters>(savedblob);

                    // set our viewmodel's index properties to appropriate properties from saved
                    // filter file
                    GameTypeIndex = json.type_in;
                    GameArenaIndex = json.arena_in;
                    GameLocationIndex = json.location_in;
                    GameStateIndex = json.state_in;

                    switch (json.visibility_in)
                    {
                        case 0:
                        default:
                            GameVisibilityIndex = 0;
                            break;

                        case 1:
                            GameVisibilityIndex = 1;
                            break;

                        case 2:
                            GameVisibilityIndex = 2;
                            break;
                    }

                    if (json.premium_in == 0)
                    {
                        GamePremiumBool = false;
                    }
                    else
                    {
                        GamePremiumBool = true;
                    }

                    // Send the message (event) to the MainViewModel to set the text in the statusbar
                    SetFilterStatusText(GameTypeIndex, GameArenaIndex, GameLocationIndex, GameStateIndex, GameVisibilityIndex, GamePremiumBool);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error applying saved filters: " + ex);
                // Error. clear filters, set defaults
                ClearSavedUserFilters();
            }
        }

        /// <summary>
        /// Checks whether or not the most up to date filter list downloaded from the UQLT webserver
        /// exists on the disk.
        /// </summary>
        /// <returns><c>true</c> if the downloaded filter list exist, otherwise <c>false</c></returns>
        private bool DownloadedFilterListExists()
        {
            return File.Exists(UQltGlobals.CurrentFilterPath);
        }

        /// <summary>
        /// Asynchronously populates the filter collections on this viewmodel using the current
        /// downloaded filter file and applies the filters.
        /// </summary>
        /// <returns>Nothing.</returns>
        private async Task PopulateAndApplyFiltersAsync()
        {
            // Make sure there is a current filter file on the hard disk.
            if (DownloadedFilterListExists())
            {
                try
                {
                    // Read filter json from filedir\data\currentfilters.json
                    using (var sr = new StreamReader(UQltGlobals.CurrentFilterPath))
                    {
                        var json = await sr.ReadToEndAsync();
                        var filters = JsonConvert.DeserializeObject<ImportedFilters>(json);

                        // set our viewmodel's properties to those in the model
                        GameTypes = filters.game_types;
                        Arenas = filters.arenas;
                        Locations = filters.locations.Where(location => location.active).ToList();
                        GameState = filters.gamestate;

                        // add to appropriate list in order to set proper arena tag when building filter
                        foreach (var arena in filters.arenas)
                        {
                            if (arena.arena_type.Equals("map") && (!ArenaMap.Contains(arena.arena)))
                            {
                                ArenaMap.Add(arena.arena);
                            }
                            else if (arena.arena_type.Equals("tag") && (!ArenaTag.Contains(arena.arena)))
                            {
                                ArenaTag.Add(arena.arena);
                            }
                        }
                    }

                    // Apply
                    if (SavedUserFiltersExist())
                    {
                        ApplySavedUserFilters();
                    }
                    else
                    {
                        ApplyDefaultFilters();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to read filter data.");
                    Debug.WriteLine(ex);

                    // TODO: make this download filter from net, if that fails THEN load hard-coded filter
                    var failsafe = new FailsafeFilterHelper();
                    failsafe.DumpBackupFilters();
                    ApplyDefaultFilters();
                }
            }
            // Current filter list does not exist. Dump it to disk from failsafe then set some
            // default filters.
            // TODO: implement functionality that will download or mark the client to download
            //       filter list from webserver on next run.
            else
            {
                var failsafe = new FailsafeFilterHelper();
                failsafe.DumpBackupFilters();
                ApplyDefaultFilters();
            }
        }

        /// <summary>
        /// Receives indexes, compares them to downloaded filter list then passes the information to
        /// <see cref="MakeEncodedFilter" /> to actually make the encoded filter.
        /// </summary>
        /// <param name="gameTypeIndex">The gametype index.</param>
        /// <param name="gameArenaIndex">The game arena index.</param>
        /// <param name="gameLocationIndex">The game location index.</param>
        /// <param name="gameStateIndex">The game state index.</param>
        /// <param name="gameVisibilityIndex">The game visibility index.</param>
        /// <param name="gamePremiumBool">
        /// if set to <c>true</c> then the game is premium, otherwise <c>false</c>.
        /// </param>

        private string ReceiveIndexesForEncoding(int gameTypeIndex, int gameArenaIndex, int gameLocationIndex, int gameStateIndex, int gameVisibilityIndex, bool gamePremiumBool)
        {
            string gameType = null;
            string gameArena = null;
            string gameState = null;
            object gameLocation = null;

            try
            {
                using (var sr = new StreamReader(UQltGlobals.CurrentFilterPath))
                {
                    string blob = sr.ReadToEnd();
                    var json = JsonConvert.DeserializeObject<ImportedFilters>(blob);

                    foreach (var gametype in json.game_types.Where(gametype => json.game_types[gameTypeIndex].ToString().Equals(gametype.display_name)))
                    {
                        Debug.WriteLine("Gametype: " + gametype.game_type);
                        gameType = gametype.game_type;
                    }

                    foreach (var arena in json.arenas.Where(arena => json.arenas[gameArenaIndex].ToString().Equals(arena.display_name)))
                    {
                        Debug.WriteLine("Arena Type: " + arena.arena_type);
                        Debug.WriteLine("Arena: " + arena.arena);
                        gameArena = arena.arena;
                    }

                    foreach (var location in json.locations.Where(location => json.locations[gameLocationIndex].ToString().Equals(location.display_name)))
                    {
                        Debug.WriteLine("Location: " + location.location_id);
                        gameLocation = location.location_id;
                    }

                    foreach (var gamestate in json.gamestate.Where(gamestate => json.gamestate[gameStateIndex].ToString().Equals(gamestate.display_name)))
                    {
                        Debug.WriteLine("Gamestate: " + gamestate.state);
                        gameState = gamestate.state;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error creating url from filters: " + ex);
            }

            return MakeEncodedFilter(gameType, gameArena, gameState, gameLocation, gameVisibilityIndex, gamePremiumBool);
        }

        /// <summary>
        /// Checks whether or not there are saved filter settings for the user on the disk.
        /// </summary>
        /// <returns><c>true</c> if saved filter settings exist, otherwise <c>false</c></returns>
        private bool SavedUserFiltersExist()
        {
            // TODO: Implement multiple user account support.
            return File.Exists(UQltGlobals.SavedUserFilterPath);
        }
    }
}