using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows.Data;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Core.ServerBrowser;
using UQLT.Events;
using UQLT.Interfaces;
using UQLT.Models.Configuration;

namespace UQLT.ViewModels
{
	[Export(typeof(ServerBrowserViewModel))]

	/// <summary>
	/// Viewmodel for the Server Browser view
	/// </summary>
	public class ServerBrowserViewModel : PropertyChangedBase, IHandle<ServerRequestEvent>, IUQLTConfiguration
	{
		private readonly IEventAggregator _events;

		private ServerBrowser SB;

		private ObservableCollection<ServerDetailsViewModel> _servers;

		/// <summary>
		/// Gets or sets the servers that this viewmodel will display in the view.
		/// </summary>
		/// <value>
		/// The servers that this viewmodel will display in the view.
		/// </value>
		public ObservableCollection<ServerDetailsViewModel> Servers
		{
			get
			{
				return _servers;
			}

			set
			{
				_servers = value;
				NotifyOfPropertyChange(() => Servers);
			}
		}

		private ServerDetailsViewModel _selectedServer;

		/// <summary>
		/// Gets or sets the selected server.
		/// </summary>
		/// <value>
		/// The selected server.
		/// </value>
		public ServerDetailsViewModel SelectedServer
		{
			get
			{
				return _selectedServer;
			}

			set
			{
				_selectedServer = value;
				NotifyOfPropertyChange(() => SelectedServer);
			}
		}

		private string _filterURL;

		/// <summary>
		/// Gets or sets the Quake Live filter URL.
		/// </summary>
		/// <value>
		/// The Quake Live filter URL.
		/// </value>
		public string FilterURL
		{
			get
			{
				return _filterURL;
			}

			set
			{
				_filterURL = value + Math.Truncate((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
				NotifyOfPropertyChange(() => FilterURL);
			}
		}

		private bool _isUpdatingServers;

		/// <summary>
		/// Gets or sets a value indicating whether this viewmodel is currently updating the server list.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is updating servers; otherwise, <c>false</c>.
		/// </value>
		public bool IsUpdatingServers
		{
			get
			{
				return _isUpdatingServers;
			}
			set
			{
				_isUpdatingServers = value;
				NotifyOfPropertyChange(() => IsUpdatingServers);
			}
		}

		private List<ServerBrowserRefreshItem> _autoRefreshItems;

		/// <summary>
		/// Gets or sets the list of time intervals that the user can select for automatic server refreshing.
		/// </summary>
		/// <value>
		/// The list of time intervals.
		/// </value>
		public List<ServerBrowserRefreshItem> AutoRefreshItems
		{
			get
			{
				return _autoRefreshItems;
			}
			set
			{
				_autoRefreshItems = value;
			}
		}

		private int _autoRefreshIndex;

		/// <summary>
		/// Gets or sets the index of the automatic refresh value.
		/// </summary>
		/// <value>
		/// The index of the automatic refresh value.
		/// </value>
		public int AutoRefreshIndex
		{
			get
			{
				return _autoRefreshIndex;
			}
			set
			{
				_autoRefreshIndex = value;
				NotifyOfPropertyChange(() => AutoRefreshIndex);
			}
		}

		private int _autoRefreshSeconds;

		/// <summary>
		/// Gets or sets the time, in seconds, for automatic server refreshing
		/// </summary>
		/// <value>
		/// The time in seconds, for automatic server refreshing.
		/// </value>
		public int AutoRefreshSeconds
		{
			get
			{
				return _autoRefreshSeconds;
			}
			set
			{
				_autoRefreshSeconds = value;
				NotifyOfPropertyChange(() => AutoRefreshSeconds);
			}
		}

		private bool _isAutoRefreshEnabled;

		/// <summary>
		/// Gets or sets a value indicating whether this automatic server refreshing is enabled.
		/// </summary>
		/// <value>
		/// <c>true</c> if this automatic refresh is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool IsAutoRefreshEnabled
		{
			get
			{
				return _isAutoRefreshEnabled;
			}
			set
			{
				_isAutoRefreshEnabled = value;
				NotifyOfPropertyChange(() => IsAutoRefreshEnabled);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServerBrowserViewModel"/> class.
		/// </summary>
		/// <param name="events">The events that this viewmodel publishes and/or subscribes to.</param>
		[ImportingConstructor]
		public ServerBrowserViewModel(IEventAggregator events)
		{
			_events = events;
			events.Subscribe(this);
			_servers = new ObservableCollection<ServerDetailsViewModel>();
			_autoRefreshItems = new List<ServerBrowserRefreshItem>()
			{
				new ServerBrowserRefreshItem() {Name = "every 30 seconds", Seconds = 30},
				new ServerBrowserRefreshItem() {Name = "every 1 minute", Seconds = 60},
				new ServerBrowserRefreshItem() {Name = "every 1.5 minutes", Seconds = 90},
				new ServerBrowserRefreshItem() {Name = "every 5 minutes", Seconds = 300},
			};
			DoServerBrowserAutoSort("FullLocationName");
			LoadConfig();
			// Instantiate a new server browser for this viewmodel
			SB = new ServerBrowser(this, events);
		}

		/// <summary>
		/// Does the server browser automatic sort based on specified criteria.
		/// </summary>
		/// <param name="property">The property criteria.</param>
		private void DoServerBrowserAutoSort(string property)
		{
			var view = CollectionViewSource.GetDefaultView(Servers);
			var sortDescription = new SortDescription(property, ListSortDirection.Ascending);
			view.SortDescriptions.Add(sortDescription);
		}

		/// <summary>
		/// Handles the specified message (event) that is received whenever this viewmodel receive notice of a new default filter,
		/// either through the "make new default" button or "reset filters" button from the FilterViewModel.
		/// </summary>
		/// <param name="message">The message (event).</param>
		public void Handle(ServerRequestEvent message)
		{
			FilterURL = message.ServerRequestURL;
			var l = SB.LoadServerListAsync(FilterURL);
			Debug.WriteLine("[EVENT RECEIVED] Filter URL Change: " + message.ServerRequestURL);
		}

		/// <summary>
		/// Sets the refresh time.
		/// </summary>
		/// <param name="seconds">The time, in seconds.</param>
		/// <remarks>This is called from the view itself.</remarks>
		public void SetRefreshTime(int seconds)
		{
			Debug.WriteLine("Setting auto-server refresh time to " + seconds + " seconds.");
			seconds = AutoRefreshSeconds;
			SaveConfig();
		}

		/// <summary>
		/// Starts the server refresh timer.
		/// </summary>
		/// <remarks>This is called from the view itself.</remarks>
		public void StartServerRefreshTimer()
		{
			SB.StartServerRefreshTimer();
		}

		/// <summary>
		/// Stops the server refresh timer.
		/// </summary>
		/// <remarks>This is called from the view itself.</remarks>
		public void StopServerRefreshTimer()
		{
			SB.StopServerRefreshTimer();
		}

		/// <summary>
		/// Checks whether the configuration already exists
		/// </summary>
		/// <returns>
		/// <c>true</c> if configuration exists, otherwise <c>false</c>
		/// </returns>
		public bool ConfigExists()
		{
			return File.Exists(UQLTGlobals.ConfigPath) ? true : false;
		}

		/// <summary>
		/// Loads the configuration.
		/// </summary>
		public void LoadConfig()
		{
			if (!ConfigExists())
			{
				LoadDefaultConfig();
			}

			try
			{
				using (StreamReader sr = new StreamReader(UQLTGlobals.ConfigPath))
				{
					string saved = sr.ReadToEnd();
					Configuration savedConfigJson = JsonConvert.DeserializeObject<Configuration>(saved);

					// Set VM's index properties to appropriate properties from configuration file
					IsAutoRefreshEnabled = savedConfigJson.serverbrowser_options.auto_refresh;
					AutoRefreshIndex = savedConfigJson.serverbrowser_options.auto_refresh_index;
					AutoRefreshSeconds = savedConfigJson.serverbrowser_options.auto_refresh_seconds;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error loading configuration " + ex);
				LoadDefaultConfig();
			}
		}

		/// <summary>
		/// Loads the default configuration.
		/// </summary>
		public void LoadDefaultConfig()
		{
			// Set the default VM properties (auto-refresh enabled, refresh every 60 seconds)
			IsAutoRefreshEnabled = true;
			AutoRefreshIndex = 1;
			AutoRefreshSeconds = 60;
			SaveConfig();
		}

		/// <summary>
		/// Saves the configuration.
		/// </summary>
		public void SaveConfig()
		{
			// Save this configuration as the new default
			var sbo = new ServerBrowserOptions()
			{
				auto_refresh = IsAutoRefreshEnabled,
				auto_refresh_index = AutoRefreshIndex,
				auto_refresh_seconds = AutoRefreshSeconds
			};
			var config = new Configuration()
			{
				serverbrowser_options = sbo
			};

			// write to disk
			string savedConfigJson = JsonConvert.SerializeObject(config);
			using (FileStream fs = File.Create(UQLTGlobals.ConfigPath))
			using (TextWriter writer = new StreamWriter(fs))
			{
				writer.WriteLine(savedConfigJson);
			}
			Debug.WriteLine("**New server browser auto-refresh configuration saved");
		}
	}
}