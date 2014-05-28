using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
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

			// Loading filters:
			// game visibility types don't depend on saved filter file
			_gameVisibility.Add("Public games");
			_gameVisibility.Add("Private games");

			//TODO: fix the interaction between downloading & dumping failsafe when downloading of filter list functionality is implemented
			// load hard-coded fail-safe filters if downloaded list doesn't exist
			if (!DownloadedFilterListExists())
			{
				FailsafeFilterHelper failsafe = new FailsafeFilterHelper();
				failsafe.DumpBackupFilters();
				var p = PopulateFilters();

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
				var p = PopulateFilters();

				if (SavedUserFiltersExist())
				{
					ApplySavedUserFilters();
				}
				else
				{
					SetStandardDefaultFilters();
				}
			}

			// Send the message (event) to the MainViewModel to set the text in the statusbar
			//SetFilterStatusText(GameTypeIndex, GameArenaIndex, GameLocationIndex, GameStateIndex, GameVisibilityIndex, GamePremiumBool);

		}

		public void Handle(FilterVisibilityEvent message)
		{
			IsVisible = message.FilterViewVisibility;

			// Debug.WriteLine("Visibility: " + message.FilterViewVisibility);
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
			Debug.WriteLine("Saved filters cleared!");
		}

		// This is called directly from the Filter view itself
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
			var sf = new SavedFilters()
			{
				type_in = selGameTypeIndex,
				arena_in = selGameArenaIndex,
				location_in = selGameLocationIndex,
				state_in = selGameStateIndex,
				visibility_in = selGameVisibilityIndex,
				premium_in = selGamePremiumIndex,
				fltr_enc = MakeEncFilterFromIndexes(selGameTypeIndex, selGameArenaIndex, selGameLocationIndex, selGameStateIndex, selGameVisibilityIndex, selGamePremiumBool)
			};

			// write to disk
			string savedfilterjson = JsonConvert.SerializeObject(sf);
			using(FileStream fs = File.Create(UQLTGlobals.SavedUserFilterPath))
			using(TextWriter writer = new StreamWriter(fs))
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

			Debug.WriteLine("Arena type for " + arena + " is: " + arena_type);

			// ig, GameTypes array, ranked determination from gametype:
			try
			{
				// read filter json from filedir\data\currentfilters.json
				using(StreamReader sr = new StreamReader(UQLTGlobals.CurrentFilterPath))
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
				encodedFilter = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonFilterString)) + "&_=";

				Debug.WriteLine("Arena ({0}) type: {1} | Gametype ({2}) ig: {3} | GameTypes: {4} | ranked: {5}", arena, arena_type, gametype, ig, string.Join(", ", gtarr), ranked);
				Debug.WriteLine(jsonFilterString);
				Debug.WriteLine(encodedFilter);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Unable to read filter data. Error: " + ex);
			}

			// fire event to server browser
			SetServerBrowserUrl(UQLTGlobals.QLDomainListFilter + encodedFilter);
			return encodedFilter;
		}

		// This event will be fired to the Main ViewModel so that the status bar can display the correct filter text. It is called from the view itself.
		// Unfortunately, indexes must be used since the ValuePath in the view is already set
		public void SetFilterStatusText(int gametype_index, int gamearena_index, int gamelocation_index, int gamestate_index, int gamevisibility_index, bool gamepremium)
		{
			string gametype;
			string gamearena;
			string gamelocation;
			string gamestate;
			string gamevisibility;
			string premium;

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


		// This event will be fired to the Server Browser ViewModel notifying it that a new filter URL has been set.
		public void SetServerBrowserUrl(string url)
		{
			// Debug.WriteLine("Attempting to publish event");
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

		private async Task PopulateFilters()
		{
			try
			{
				// read filter json from filedir\data\currentfilters.json
				using(StreamReader sr = new StreamReader(UQLTGlobals.CurrentFilterPath))
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
			}
			catch (Exception ex)
			{
				MessageBox.Show("Unable to read filter data.");
				Debug.WriteLine(ex);

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
		}

		private void ApplySavedUserFilters()
		{
			try
			{
				using(StreamReader sr = new StreamReader(UQLTGlobals.SavedUserFilterPath))
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
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error applying saved filters: " + ex);
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
				using(StreamReader sr = new StreamReader(UQLTGlobals.CurrentFilterPath))
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