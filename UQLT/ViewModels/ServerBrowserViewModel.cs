using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows.Data;
using Caliburn.Micro;
using UQLT.Core.ServerBrowser;
using UQLT.Events;
using UQLT.Interfaces;
using UQLT.Models.Configuration;

namespace UQLT.ViewModels
{
	/// <summary>
	/// Viewmodel for the Server Browser view.
	/// </summary>
	[Export(typeof(ServerBrowserViewModel))]
	public class ServerBrowserViewModel : PropertyChangedBase, IHandle<ServerRequestEvent>, IUqltConfiguration
	{
		private readonly ServerBrowser _sb;
		private int _autoRefreshIndex;

		private List<ServerBrowserRefreshItem> _autoRefreshItems = new List<ServerBrowserRefreshItem>
		{
				new ServerBrowserRefreshItem {Name = "every 30 seconds", Seconds = 30},
				new ServerBrowserRefreshItem {Name = "every 1 minute", Seconds = 60},
				new ServerBrowserRefreshItem {Name = "every 1.5 minutes", Seconds = 90},
				new ServerBrowserRefreshItem {Name = "every 5 minutes", Seconds = 300},
		};

		private int _autoRefreshSeconds;
		private string _filterUrl;
		private bool _isAutoRefreshEnabled;
		private bool _isUpdatingServers;
		private ServerDetailsViewModel _selectedServer;
		private ObservableCollection<ServerDetailsViewModel> _servers = new ObservableCollection<ServerDetailsViewModel>();

		/// <summary>
		/// Initializes a new instance of the <see cref="ServerBrowserViewModel" /> class.
		/// </summary>
		/// <param name="events">The events that this viewmodel publishes and/or subscribes to.</param>
		[ImportingConstructor]
		public ServerBrowserViewModel(IEventAggregator events)
		{
			events.Subscribe(this);

			DoServerBrowserAutoSort("FullLocationName");
			LoadConfig();

			// Instantiate a new server browser for this viewmodel
			_sb = new ServerBrowser(this, events);
		}

		/// <summary>
		/// Gets or sets the index of the automatic refresh value.
		/// </summary>
		/// <value>The index of the automatic refresh value.</value>
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

		/// <summary>
		/// Gets or sets the list of time intervals that the user can select for automatic server refreshing.
		/// </summary>
		/// <value>The list of time intervals.</value>
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

		/// <summary>
		/// Gets or sets the time, in seconds, for automatic server refreshing
		/// </summary>
		/// <value>The time in seconds, for automatic server refreshing.</value>
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

		/// <summary>
		/// Gets or sets the Quake Live filter URL.
		/// </summary>
		/// <value>The Quake Live filter URL.</value>
		public string FilterUrl
		{
			get
			{
				return _filterUrl;
			}

			set
			{
				_filterUrl = value + Math.Truncate((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
				NotifyOfPropertyChange(() => FilterUrl);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this automatic server refreshing is enabled.
		/// </summary>
		/// <value><c>true</c> if this automatic refresh is enabled; otherwise, <c>false</c>.</value>
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
		/// Gets or sets a value indicating whether this viewmodel is currently updating the server list.
		/// </summary>
		/// <value><c>true</c> if this instance is updating servers; otherwise, <c>false</c>.</value>
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

		/// <summary>
		/// Gets or sets the selected server.
		/// </summary>
		/// <value>The selected server.</value>
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

		/// <summary>
		/// Gets or sets the servers that this viewmodel will display in the view.
		/// </summary>
		/// <value>The servers that this viewmodel will display in the view.</value>
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

		/// <summary>
		/// Checks whether the configuration already exists
		/// </summary>
		/// <returns><c>true</c> if configuration exists, otherwise <c>false</c></returns>
		public bool ConfigExists()
		{
			return File.Exists(UQltGlobals.ConfigPath);
		}

		/// <summary>
		/// Handles the specified message (event) that is received whenever this viewmodel receive
		/// notice of a new default filter, either through the "make new default" button or "reset
		/// filters" button from the FilterViewModel.
		/// </summary>
		/// <param name="message">The message (event).</param>
		public void Handle(ServerRequestEvent message)
		{
			FilterUrl = message.ServerRequestUrl;
			// Async: suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
			var l = _sb.LoadServerListAsync(FilterUrl);
			Debug.WriteLine("[EVENT RECEIVED] Filter URL Change: " + message.ServerRequestUrl);
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

			var cfghandler = new ConfigurationHandler();
			cfghandler.ReadConfig();

			IsAutoRefreshEnabled = cfghandler.SbOptAutoRefresh;
			AutoRefreshIndex = cfghandler.SbOptAutoRefreshIndex;
			AutoRefreshSeconds = cfghandler.SbOptAutoRefreshSeconds;
		}

		/// <summary>
		/// Loads the default configuration.
		/// </summary>
		public void LoadDefaultConfig()
		{
			var cfghandler = new ConfigurationHandler();
			cfghandler.RestoreDefaultConfig();
		}

		/// <summary>
		/// Saves the configuration.
		/// </summary>
		public void SaveConfig()
		{
			var cfghandler = new ConfigurationHandler();
			cfghandler.ReadConfig();

			cfghandler.SbOptAutoRefresh = IsAutoRefreshEnabled;
			cfghandler.SbOptAutoRefreshIndex = AutoRefreshIndex;
			cfghandler.SbOptAutoRefreshSeconds = AutoRefreshSeconds;

			cfghandler.WriteConfig();
		}

		/// <summary>
		/// Sets the refresh time.
		/// </summary>
		/// <param name="seconds">The time, in seconds.</param>
		/// <remarks>This is called from the view itself.</remarks>
		public void SetRefreshTime(int seconds)
		{
			Debug.WriteLine("Setting auto-server refresh time to " + seconds + " seconds.");
			AutoRefreshSeconds = seconds;
			SaveConfig();
		}

		/// <summary>
		/// Starts the server refresh timer.
		/// </summary>
		/// <remarks>This is called from the view itself.</remarks>
		public void StartServerRefreshTimer()
		{
			_sb.StartServerRefreshTimer();
		}

		/// <summary>
		/// Stops the server refresh timer.
		/// </summary>
		/// <remarks>This is called from the view itself.</remarks>
		public void StopServerRefreshTimer()
		{
			_sb.StopServerRefreshTimer();
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
	}
}