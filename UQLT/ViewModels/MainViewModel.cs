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
	public class MainViewModel : PropertyChangedBase, IHaveDisplayName
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
	}
}