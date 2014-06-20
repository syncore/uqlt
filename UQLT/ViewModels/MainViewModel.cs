using System.ComponentModel.Composition;
using System.Diagnostics;
using Caliburn.Micro;
using UQLT.Events;

namespace UQLT.ViewModels
{
    [Export(typeof(MainViewModel))]

    /// <summary>
    /// Viewmodel for the main window. This represents the bulk of the application once the user has
    /// gotten past the login screen by successfully authenticating with QL
    /// </summary>
    public class MainViewModel : PropertyChangedBase, IHaveDisplayName, IHandle<FilterStatusEvent>, IHandle<ServerCountEvent>, IHandle<PlayerCountEvent>
    {
        private readonly IEventAggregator _events;
        private readonly IWindowManager _windowManager;
        private string _arenaStatusTitle;
        private string _displayName = "UQLT v0.1";
        private FilterViewModel _filterViewModel;
        private string _locationStatusTitle;
        private int _playerCountStatusTitle;
        private string _premiumStatusTitle;
        private int _serverCountStatusTitle;

        private string _stateStatusTitle;

        private string _typeStatusTitle;

        private string _visibilityStatusTitle;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        /// <param name="windowManager">The window manager.</param>
        /// <param name="events">The events that this viewmodel publishes and/or subscribes to.</param>
        [ImportingConstructor]
        public MainViewModel(IWindowManager windowManager, IEventAggregator events)
        {
            _windowManager = windowManager;
            _events = events;
            events.Subscribe(this);
            _filterViewModel = new FilterViewModel(_events);
            ServerBrowserViewModel = new ServerBrowserViewModel(_events);
            ChatListViewModel = new ChatListViewModel(windowManager, _events);
            // TODO: _events for any events, i.e.: hiding buddy list
        }

        /// <summary>
        /// Gets or sets the game arena status bar title.
        /// </summary>
        /// <value>The game arena status bar title.</value>
        /// <remarks>This is a property used for the status bar.</remarks>
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

        /// <summary>
        /// Gets or sets the chat ListView model.
        /// </summary>
        /// <value>The chat ListView model.</value>
        public ChatListViewModel ChatListViewModel { get; set; }

        /// <summary>
        /// Gets or Sets the display name for this window.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the filter view model.
        /// </summary>
        /// <value>The filter view model.</value>
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

        /// <summary>
        /// Gets or sets the game location status bar title.
        /// </summary>
        /// <value>The game location status bar title.</value>
        /// <remarks>This is a property used for the status bar.</remarks>
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

        /// <summary>
        /// Gets or sets the player count status bar title.
        /// </summary>
        /// <value>The player count status bar title.</value>
        /// <remarks>This is a property used for the status bar.</remarks>
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

        /// <summary>
        /// Gets or sets the premium status bar title.
        /// </summary>
        /// <value>The premium status bar title.</value>
        /// <remarks>This is a property used for the status bar.</remarks>
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

        /// <summary>
        /// Gets or sets the server browser view model.
        /// </summary>
        /// <value>The server browser view model.</value>
        public ServerBrowserViewModel ServerBrowserViewModel { get; set; }

        /// <summary>
        /// Gets or sets the server count status bar title.
        /// </summary>
        /// <value>The server count status bar title.</value>
        /// <remarks>This is a property used for the status bar.</remarks>
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

        /// <summary>
        /// Gets or sets the game state status bar title.
        /// </summary>
        /// <value>The gane state status bar title.</value>
        /// <remarks>This is a property used for the status bar.</remarks>
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

        /// <summary>
        /// Gets or sets the game type status bar title.
        /// </summary>
        /// <value>The game type status bar title.</value>
        /// <remarks>This is a property used for the status bar.</remarks>
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

        /// <summary>
        /// Gets or sets the game visibility status bar title.
        /// </summary>
        /// <value>The game visibility status bar title.</value>
        /// <remarks>This is a property used for the status bar.</remarks>
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

        /// <summary>
        /// Handles the message (event) sent to this viewmodel from the FilterViewModel to set the
        /// statusbar text.
        /// </summary>
        /// <param name="message">The message (event)</param>
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

        /// <summary>
        /// Handles the message (event) sent to this viewmodel from the ServerBrowser to update the
        /// server count in the statusbar.
        /// </summary>
        /// <param name="message">The message (event).</param>
        public void Handle(ServerCountEvent message)
        {
            ServerCountStatusTitle = message.ServerCount;
            Debug.WriteLine("[EVENT RECEIVED] Incoming server count information from Server Browser: " + message.ServerCount);
        }

        /// <summary>
        /// Handles the message (event) sent to this viewmodel from the ServerBrowser to update the
        /// player count in the statusbar.
        /// </summary>
        /// <param name="message">The message (event).</param>
        public void Handle(PlayerCountEvent message)
        {
            PlayerCountStatusTitle = message.PlayerCount;
            Debug.WriteLine("[EVENT RECEIVED] Incoming server count information from Server Browser: " + message.PlayerCount);
        }

        /// <summary>
        /// Hides the filter menu.
        /// </summary>
        /// <remarks>This is called from the view itself.</remarks>
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