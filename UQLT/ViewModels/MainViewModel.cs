using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using UQLT.Events;
using UQLT.Helpers;

namespace UQLT.ViewModels
{

	[Export(typeof(MainViewModel))]

	/// <summary>
	/// Viewmodel for the main window. This represents the bulk of the application once the user has gotten past the login screen by successfully authenticating with QL
	/// </summary>
	public class MainViewModel : PropertyChangedBase, IHaveDisplayName, IHandle<FilterStatusEvent>, IHandle<ServerCountEvent>, IHandle<PlayerCountEvent>
	{
		private readonly IEventAggregator _events;
		private readonly IWindowManager _windowManager;
		private string _displayName = "UQLT v0.1";
		private FilterViewModel _filterViewModel;
		private ServerBrowserViewModel _serverBrowserViewModel;
		private ChatListViewModel _chatListViewModel;

		[ImportingConstructor]
		public MainViewModel(IWindowManager windowManager, IEventAggregator events)
		{
			_windowManager = windowManager;
			_events = events;
			events.Subscribe(this);
			_filterViewModel = new FilterViewModel(_events);
			_serverBrowserViewModel = new ServerBrowserViewModel(_events);
			_chatListViewModel = new ChatListViewModel(); // TODO: _events for any events, i.e.: hiding buddy list
		}

		public string DisplayName
		{
			get
			{
				return _displayName;
			}
			set
			{
				_displayName = value;
			}
		}

		public FilterViewModel FilterViewModel
		{
			get
			{
				return _filterViewModel;
			}
			set
			{
				_filterViewModel = value;
			}
		}

		public ServerBrowserViewModel ServerBrowserViewModel
		{
			get
			{
				return _serverBrowserViewModel;
			}
			set
			{
				_serverBrowserViewModel = value;
			}
		}

		public ChatListViewModel ChatListViewModel
		{
			get
			{
				return _chatListViewModel;
			}
			set
			{
				_chatListViewModel = value;
			}
		}

		// Status bar properties:

		private int _serverCountStatusTitle;
		public int ServerCountStatusTitle
		{
			get
			{
				return _serverCountStatusTitle;
			}
			set
			{
				_serverCountStatusTitle = value;
				NotifyOfPropertyChange(() => ServerCountStatusTitle);
			}

		}
		private int _playerCountStatusTitle;
		public int PlayerCountStatusTitle
		{
			get
			{
				return _playerCountStatusTitle;
			}
			set
			{
				_playerCountStatusTitle = value;
				NotifyOfPropertyChange(() => PlayerCountStatusTitle);
			}

		}
		private string _premiumStatusTitle;
		public string PremiumStatusTitle
		{
			get
			{
				return _premiumStatusTitle;
			}
			set
			{
				_premiumStatusTitle = value;
				NotifyOfPropertyChange(() => PremiumStatusTitle);
			}

		}
		private string _visibilityStatusTitle;
		public string VisibilityStatusTitle
		{
			get
			{
				return _visibilityStatusTitle;
			}
			set
			{
				_visibilityStatusTitle = value;
				NotifyOfPropertyChange(() => VisibilityStatusTitle);
			}

		}
		private string _typeStatusTitle;
		public string TypeStatusTitle
		{
			get
			{
				return _typeStatusTitle;
			}
			set
			{
				_typeStatusTitle = value;
				NotifyOfPropertyChange(() => TypeStatusTitle);
			}

		}
		private string _stateStatusTitle;
		public string StateStatusTitle
		{
			get
			{
				return _stateStatusTitle;
			}
			set
			{
				_stateStatusTitle = value;
				NotifyOfPropertyChange(() => StateStatusTitle);
			}

		}
		private string _locationStatusTitle;
		public string LocationStatusTitle
		{
			get
			{
				return _locationStatusTitle;
			}
			set
			{
				_locationStatusTitle = value;
				NotifyOfPropertyChange(() => LocationStatusTitle);
			}

		}
		private string _arenaStatusTitle;
		public string ArenaStatusTitle
		{
			get
			{
				return _arenaStatusTitle;
			}
			set
			{
				_arenaStatusTitle = value;
				NotifyOfPropertyChange(() => ArenaStatusTitle);
			}
		}

		// This method is called from the view itself.
		public void HideFilters()
		{
			// Debug.WriteLine("Attempting to publish event");
			if (_filterViewModel.IsVisible)
			{
				_events.Publish(new FilterVisibilityEvent(false));
			}
			else
			{
				_events.Publish(new FilterVisibilityEvent(true));
			}
		}

		// This ViewModel has received a message (event) from the FilterViewModel to set the statusbar text
		public void Handle(FilterStatusEvent message)
		{
			TypeStatusTitle = message.Gametype;
			ArenaStatusTitle = message.Arena;
			LocationStatusTitle = message.Location;
			StateStatusTitle = message.Gamestate;
			VisibilityStatusTitle = message.GameVisibility;
			PremiumStatusTitle = message.Premium;

			Debug.WriteLine("[EVENT RECEIVED] Incoming filter information from Filter ViewModel: {0} {1} {2} {3} {4} {5}", message.Gametype, message.Arena, message.Location,
			                message.Gamestate, message.GameVisibility, message.Premium);
		}

		// This ViewModel has received a message (event) from the ServerBrowser to update the server count in the statusbar
		public void Handle(ServerCountEvent message)
		{
			ServerCountStatusTitle = message.ServerCount;
			Debug.WriteLine("[EVENT RECEIVED] Incoming server count information from Server Browser: " + message.ServerCount);
		}

		// This ViewModel has received a message (event) from the ServerBrowser to update the player count in the statusbar
		public void Handle(PlayerCountEvent message)
		{
			PlayerCountStatusTitle = message.PlayerCount;
			Debug.WriteLine("[EVENT RECEIVED] Incoming server count information from Server Browser: " + message.PlayerCount);
		}
	}
}