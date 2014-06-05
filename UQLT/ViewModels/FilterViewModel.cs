﻿using System;
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
    [Export(typeof(FilterViewModel))]

    /// <summary>
    /// Viewmodel for the filter menu
    /// </summary>
    public class FilterViewModel : PropertyChangedBase, IHandle<FilterVisibilityEvent>
    {
        // list of maps that receive "tag" description for arena_type when building filter string
        private static List<string> arenaTag = new List<string>();

        // list of maps that receive "map" description for arena_type when building filter string
        private static List<string> arenaMap = new List<string>();

        private readonly IEventAggregator _events;

        private bool _isVisible = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
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

        private List<Location> _locations;

        /// <summary>
        /// Gets or sets the locations to be used in the view.
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
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

        private List<Arena> _arenas;

        /// <summary>
        /// Gets or sets the arenas to be used in the view.
        /// </summary>
        /// <value>
        /// The arenas.
        /// </value>
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

        /// <summary>
        /// Gets or sets the difficulties to be displayed in the view.
        /// </summary>
        /// <value>
        /// The difficulties.
        /// </value>
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

        /// <summary>
        /// Gets or sets the gamestates to be displayed in the view.
        /// </summary>
        /// <value>
        /// The gamestates.
        /// </value>
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

        /// <summary>
        /// Gets or sets the game types to be displayed in the view.
        /// </summary>
        /// <value>
        /// The game types.
        /// </value>
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

        /// <summary>
        /// Gets or sets the game information.
        /// </summary>
        /// <value>
        /// The game information.
        /// </value>
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

        /// <summary>
        /// Gets the game visibilities to be displayed in the view.
        /// </summary>
        /// <value>
        /// The game visibility.
        /// </value>
        public List<string> GameVisibility
        {
            get
            {
                return _gameVisibility;
            }
        }

        private int _gameTypeIndex;

        /// <summary>
        /// Gets or sets the index of the game type.
        /// </summary>
        /// <value>
        /// The index of the game type.
        /// </value>
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

        /// <summary>
        /// Gets or sets the index of the game arena.
        /// </summary>
        /// <value>
        /// The index of the game arena.
        /// </value>
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

        /// <summary>
        /// Gets or sets the index of the game location.
        /// </summary>
        /// <value>
        /// The index of the game location.
        /// </value>
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

        /// <summary>
        /// Gets or sets the index of the game state.
        /// </summary>
        /// <value>
        /// The index of the game state.
        /// </value>
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

        /// <summary>
        /// Gets or sets the index of the game visibility.
        /// </summary>
        /// <value>
        /// The index of the game visibility.
        /// </value>
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

        /// <summary>
        /// Gets or sets the index of the game premium.
        /// </summary>
        /// <value>
        /// The index of the game premium.
        /// </value>
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

        /// <summary>
        /// Gets or sets a value indicating whether the game is a premium game or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if game is premium; otherwise, <c>false</c>.
        /// </value>
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
        /// Initializes a new instance of the <see cref="FilterViewModel"/> class.
        /// </summary>
        /// <param name="events">The events that this viewmodel publishes/subscribes to.</param>
        [ImportingConstructor]
        public FilterViewModel(IEventAggregator events)
        {
            _events = events;
            events.Subscribe(this);

            // Loading filters:
            // game visibility types don't depend on saved filter file
            _gameVisibility.Add("Public games");
            _gameVisibility.Add("Private games");

            //TODO: implement downloading of filter list functionality
            var p = PopulateAndApplyFiltersAsync();
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
        /// Clears the saved user filters.
        /// </summary>
        public void ClearSavedUserFilters()
        {
            if (SavedUserFiltersExist())
            {
                File.Delete(UQLTGlobals.SavedUserFilterPath);
            }

            SetStandardDefaultFilters();
            SetServerBrowserUrl(UQLTGlobals.QLDefaultFilter);
            Debug.WriteLine("Saved filters cleared!");
        }

        /// <summary>
        /// Saves the new user filters. This makes the current selections in the view's comboboxes (or from the filter file, depending on the caller) the new default filters.
        /// </summary>
        /// <param name="selGameTypeIndex">Index of the selected game type.</param>
        /// <param name="selGameArenaIndex">Index of the selected game arena.</param>
        /// <param name="selGameLocationIndex">Index of the selected game location.</param>
        /// <param name="selGameStateIndex">Index of the selected game state.</param>
        /// <param name="selGameVisibilityIndex">Index of the selected game visibility.</param>
        /// <param name="selGamePremiumBool">if set to <c>true</c> [selected game premium bool].</param>
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

            // Filter to object for json file
            var sf = new SavedFilters()
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
            using (FileStream fs = File.Create(UQLTGlobals.SavedUserFilterPath))
            using (TextWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine(savedfilterjson);
            }
        }

        /// <summary>
        /// Makes the encoded filter. This takes the output from the filter menu, processes it, and returns base64 encoded filter json.
        /// </summary>
        /// <param name="gametype">The gametype.</param>
        /// <param name="arena">The arena.</param>
        /// <param name="state">The state.</param>
        /// <param name="location">The location.</param>
        /// <param name="priv">Whether the game is private</param>
        /// <param name="ispremium">if set to <c>true</c> then the game is premium, if <c>false</c> then it is not premium.</param>
        /// <returns>Base64 encoded json filter data.</returns>
        public string MakeEncodedFilter(string gametype, string arena, string state, object location, object priv, bool ispremium)
        {
            string encodedFilter = null;
            string jsonFilterString = null;
            // players is always empty for purposes of filter encoding
            List<string> players = new List<string>();

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

            Debug.WriteLine("Arena type for " + arena + " is: " + arena_type);

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
            SetServerBrowserUrl(UQLTGlobals.QLDomainListFilter + encodedFilter);
            return encodedFilter;
        }

        /// <summary>
        /// Sets the filter status text. This will fire a message (event) to the MainViewModel so that the status bar can display the correct filter text.
        /// </summary>
        /// <param name="gametype_index">The currently selected gametype index.</param>
        /// <param name="gamearena_index">The currently selected game arena index.</param>
        /// <param name="gamelocation_index">The currently selected game location index.</param>
        /// <param name="gamestate_index">The currently selected game state index.</param>
        /// <param name="gamevisibility_index">The currently selected game visibility index.</param>
        /// <param name="gamepremium">if set to <c>true</c> then it's a premium game, if not then <c>false</c>.</param>
        /// <remarks>
        /// It is called from the view itself.
        /// Unfortunately, indexes must be used since the ValuePath in the view is already set.
        /// </remarks>
        public void SetFilterStatusText(int gametype_index, int gamearena_index, int gamelocation_index, int gamestate_index, int gamevisibility_index, bool gamepremium)
        {
            string gametype, gamearena, gamelocation, gamestate, gamevisibility, premium;

            gametype = GameTypes[gametype_index].display_name;
            gamearena = Arenas[gamearena_index].display_name;
            gamelocation = Locations[gamelocation_index].display_name;
            gamestate = GameState[gamestate_index].display_name;
            gamevisibility = GameVisibility[gamevisibility_index];

            if (gamepremium)
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
        /// Sets the server browser URL. This fires a message (event) to the Server Browser ViewModel notifying it that a new filter URL has been set.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void SetServerBrowserUrl(string url)
        {
            _events.Publish(new ServerRequestEvent(url));
        }

        /// <summary>
        /// Checks whether or not there are saved filter settings for the user on the disk.
        /// </summary>
        /// <returns><c>true</c> if saved filter settings exist, otherwise <c>false</c></returns>
        private bool SavedUserFiltersExist()
        {
            // TODO: Implement multiple user account support.
            return File.Exists(UQLTGlobals.SavedUserFilterPath) ? true : false;
        }

        /// <summary>
        /// Checks whether or not the most up to date filter list downloaded from the UQLT webserver exists on the disk.
        /// </summary>
        /// <returns><c>true</c> if the downloaded filter list exist, otherwise <c>false</c></returns>
        private bool DownloadedFilterListExists()
        {
            return File.Exists(UQLTGlobals.CurrentFilterPath) ? true : false;
        }

        /// <summary>
        /// Asynchronously populates the filter collections on this viewmodel using the current downloaded filter file and applies the filters.
        /// </summary>
        /// <returns>
        /// Nothing.
        /// </returns>
        private async Task PopulateAndApplyFiltersAsync()
        {
            // Make sure there is a current filter file on the hard disk.
            if (DownloadedFilterListExists())
            {
                try
                {
                    // Read filter json from filedir\data\currentfilters.json
                    using (StreamReader sr = new StreamReader(UQLTGlobals.CurrentFilterPath))
                    {
                        var x = await sr.ReadToEndAsync();
                        var filters = JsonConvert.DeserializeObject<ImportedFilters>(x);

                        // set our viewmodel's properties to those in the model
                        GameTypes = filters.game_types;
                        Arenas = filters.arenas;
                        Locations = filters.locations.Where(location => location.active).ToList();
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

                    // Apply
                    ApplySavedUserFilters();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to read filter data.");
                    Debug.WriteLine(ex);

                    // TODO: make this download filter from net, if that fails THEN load hard-coded filter
                    FailsafeFilterHelper failsafe = new FailsafeFilterHelper();
                    failsafe.DumpBackupFilters();
                    SetStandardDefaultFilters();
                }
            }
            // Current filter list does not exist. Dump it to disk from failsafe then set some default filters.
            // TODO: implement functionality that will download or mark the client to download filter list from webserver on next run.
            else
            {
                FailsafeFilterHelper failsafe = new FailsafeFilterHelper();
                failsafe.DumpBackupFilters();
                SetStandardDefaultFilters();
            }
        }

        /// <summary>
        /// Sets the standard filters (i.e.: "Any Location", "Any Game State", etc.) and save them as the new default.
        /// </summary>
        private void SetStandardDefaultFilters()
        {
            GameTypeIndex = 0;
            GameArenaIndex = 0;
            GameLocationIndex = 1;
            GameStateIndex = 0;
            GameVisibilityIndex = 0;
            GamePremiumBool = false;

            // Save these as the new defaults.
            SaveNewUserFilters(GameTypeIndex, GameArenaIndex, GameLocationIndex, GameStateIndex, GameVisibilityIndex, GamePremiumBool);

            // Send the message (event) to the MainViewModel to set the text in the statusbar.
            SetFilterStatusText(GameTypeIndex, GameArenaIndex, GameLocationIndex, GameStateIndex, GameVisibilityIndex, GamePremiumBool);
        }

        /// <summary>
        /// Applies the saved user filters, if they exist. Otherwise sets standard default filters.
        /// </summary>
        private void ApplySavedUserFilters()
        {
            // Make sure there are actually saved user filters on the disk.
            if (SavedUserFiltersExist())
            {
                try
                {
                    using (StreamReader sr = new StreamReader(UQLTGlobals.SavedUserFilterPath))
                    {
                        string savedblob = sr.ReadToEnd();
                        SavedFilters savedFilterJson = JsonConvert.DeserializeObject<SavedFilters>(savedblob);

                        // set our viewmodel's index properties to appropriate properties from saved filter file
                        GameTypeIndex = savedFilterJson.type_in;
                        GameArenaIndex = savedFilterJson.arena_in;
                        GameLocationIndex = savedFilterJson.location_in;
                        GameStateIndex = savedFilterJson.state_in;

                        if (savedFilterJson.visibility_in == 0)
                        {
                            GameVisibilityIndex = 0;
                        }
                        else
                        {
                            GameVisibilityIndex = 1;
                        }

                        if (savedFilterJson.premium_in == 0)
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
            // Saved user filters do not exist on disk, set the standard defaults
            else
            {
                SetStandardDefaultFilters();
            }
        }

        /// <summary>
        /// Receives indexes, compares them to downloaded filter list then passes the information to <see cref="MakeEncodedFilter"> to actually make the encoded filter.
        /// </summary>
        /// <param name="gtIndex">The gametype index.</param>
        /// <param name="arIndex">The game arena index.</param>
        /// <param name="locIndex">The game location index.</param>
        /// <param name="statIndex">The game state index.</param>
        /// <param name="visIndex">The game visibility index.</param>
        /// <param name="premBool">if set to <c>true</c> then the game is premium, otherwise <c>false</c>.</param>
        /// <returns></returns>
        private string ReceiveIndexesForEncoding(int gtIndex, int arIndex, int locIndex, int statIndex, int visIndex, bool premBool)
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

                    foreach (var gametype in importedFilterJson.game_types)
                    {
                        if (importedFilterJson.game_types[gtIndex].ToString().Equals(gametype.display_name))
                        {
                            Debug.WriteLine("Gametype: " + gametype.game_type);
                            gt = gametype.game_type;
                        }
                    }

                    foreach (var arena in importedFilterJson.arenas)
                    {
                        if (importedFilterJson.arenas[arIndex].ToString().Equals(arena.display_name))
                        {
                            Debug.WriteLine("Arena Type: " + arena.arena_type);
                            Debug.WriteLine("Arena: " + arena.arena);
                            ar = arena.arena;
                        }
                    }

                    foreach (var location in importedFilterJson.locations)
                    {
                        if (importedFilterJson.locations[locIndex].ToString().Equals(location.display_name))
                        {
                            Debug.WriteLine("Location: " + location.location_id);
                            loc = location.location_id;
                        }
                    }

                    foreach (var gamestate in importedFilterJson.gamestate)
                    {
                        if (importedFilterJson.gamestate[statIndex].ToString().Equals(gamestate.display_name))
                        {
                            Debug.WriteLine("Gamestate: " + gamestate.state);
                            stat = gamestate.state;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error creating url from filters: " + ex);
            }

            return MakeEncodedFilter(gt, ar, stat, loc, visIndex, premBool);
        }
    }
}