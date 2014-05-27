using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT;
using UQLT.Events;
using UQLT.Interfaces;
using UQLT.Models;
using UQLT.Models.Filters.Remote;
using UQLT.Models.QLRanks;
using UQLT.Models.QuakeLiveAPI;
using UQLT.Models.Filters.User;
using UQLT.Core.ServerBrowser;
using System.Net;
using System.Windows;
using UQLT.Models.Configuration;

namespace UQLT.ViewModels
{

	[Export(typeof(ServerBrowserViewModel))]

	/// <summary>
	/// Viewmodel for the Server Browser view
	/// </summary>
	public class ServerBrowserViewModel : PropertyChangedBase, IHandle<ServerRequestEvent>, IUQLTConfiguration
	{
		private ServerBrowser SB;

		private ObservableCollection<ServerDetailsViewModel> _servers;

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

		private int _numberOfServersToUpdate;

		public int NumberOfServersToUpdate
		{
			get
			{
				return _numberOfServersToUpdate;
			}
			set
			{
				_numberOfServersToUpdate = value;
				NotifyOfPropertyChange(() => NumberOfServersToUpdate);
			}
		}

		private int _numberOfPlayersToUpdate;

		public int NumberOfPlayersToUpdate
		{
			get
			{
				return _numberOfPlayersToUpdate;
			}
			set
			{
				_numberOfPlayersToUpdate = value;
				NotifyOfPropertyChange(() => NumberOfPlayersToUpdate);
			}
		}

		private List<ServerBrowserRefreshItem> _autoRefreshItems;

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

		[ImportingConstructor]
		public ServerBrowserViewModel(IEventAggregator events)
		{
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
			SB = new ServerBrowser(this);

		}

		// Sort the server browser based on specified criteria
		private void DoServerBrowserAutoSort(string property)
		{
			var view = CollectionViewSource.GetDefaultView(Servers);
			var sortDescription = new SortDescription(property, ListSortDirection.Ascending);
			view.SortDescriptions.Add(sortDescription);
		}

		// This is fired whenever we receive a new default filter, either through the "make new default" button or "reset filters" button from the FilterViewModel

		public void Handle(ServerRequestEvent message)
		{
			FilterURL = message.ServerRequestURL;
			var l = SB.LoadServerListAsync(FilterURL);
			Debug.WriteLine("[EVENT RECEIVED] Filter URL Change: " + message.ServerRequestURL);
		}

		// This method is called from the view itself.
		public void SetRefreshTime(int seconds)
		{
			Debug.WriteLine("Setting auto-server refresh time to " + seconds + " seconds.");
			seconds = AutoRefreshSeconds;
			SaveConfig();
		}

		// This method is called from the view itself.
		public void StartServerRefreshTimer()
		{
			SB.StartServerRefreshTimer();
		}

		// This method is called from the view itself.
		public void StopServerRefreshTimer()
		{
			SB.StopServerRefreshTimer();
		}

		// Does the configuration file exist?
		public bool ConfigExists()
		{
			return File.Exists(UQLTGlobals.ConfigPath) ? true : false;
		}

		// Load any user configuration settings related to the Server Browser
		public void LoadConfig()
		{

			if (!ConfigExists())
			{
				LoadDefaultConfig();
			}

			try
			{
				using(StreamReader sr = new StreamReader(UQLTGlobals.ConfigPath))
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

		// Load the default configuration options in the event that the config file is not found
		public void LoadDefaultConfig()
		{
			// Set the default VM properties (auto-refresh enabled, refresh every 60 seconds)
			IsAutoRefreshEnabled = true;
			AutoRefreshIndex = 0;
			AutoRefreshSeconds = 30;
			SaveConfig();
		}

		// Save the configuration settings related to the Server Browser. This method is called from the view itself.
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
			using(FileStream fs = File.Create(UQLTGlobals.ConfigPath))
			using(TextWriter writer = new StreamWriter(fs))
			{
				writer.WriteLine(savedConfigJson);
			}
			Debug.WriteLine("**New server browser auto-refresh configuration saved");
		}

	}
}