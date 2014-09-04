using System;
using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using UQLT.Interfaces;
using UQLT.ViewModels.DemoPlayer;

namespace UQLT.ViewModels
{
    /// <summary>
    /// The viewmodel for the LoginView, which serves as the starting point that the user sees when
    /// launching the application.
    /// </summary>
    [Export(typeof(LoginViewModel))]
    public class LoginViewModel : PropertyChangedBase, IHaveDisplayName, IViewAware
    {
        private readonly IEventAggregator _events;
        private readonly IWindowManager _windowManager;
        private readonly IMsgBoxService _msgBoxService;
        private Window _dialogWindow;
        private string _displayName = "Login to Quake Live";

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel" /> class.
        /// </summary>
        /// <param name="windowManager">The window manager.</param>
        /// <param name="events">The events that this viewmodel publishes and/or subscribes to.</param>
        /// <param name="msgBoxService">The MessageBox service.</param>
        [ImportingConstructor]
        public LoginViewModel(IWindowManager windowManager, IEventAggregator events, IMsgBoxService msgBoxService)
        {
            _windowManager = windowManager;
            _events = events;
            _msgBoxService = msgBoxService;
        }

        /// <summary>
        /// Raised when a view is attached.
        /// </summary>
        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

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
        /// Attaches a view to this instance.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The context in which the view appears.</param>
        public void AttachView(object view, object context = null)
        {
            _dialogWindow = view as Window;
            if (ViewAttached != null)
            {
                ViewAttached(this, new ViewAttachedEventArgs
                {
                    Context = context,
                    View = view
                });
            }
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <remarks>For different ways to implement window closing, see: http://stackoverflow.com/questions/10090584/how-to-close-dialog-window-from-viewmodel-caliburnwpf</remarks>
        public void CloseWin()
        {
            _dialogWindow.Close();
        }

        /// <summary>
        /// Does the login.
        /// </summary>
        public void DoLogin()
        {
            // TODO: have some login logic here.. if successful then show main window and close this
            //       current window
            _windowManager.ShowWindow(new MainViewModel(_windowManager, _events, _msgBoxService));
            CloseWin();
        }

        /// <summary>
        /// Gets a view previously attached to this instance.
        /// </summary>
        /// <param name="context">The context denoting which view to retrieve.</param>
        /// <returns>The view.</returns>
        public object GetView(object context = null)
        {
            return _dialogWindow;
        }

        /// <summary>
        /// Launches the demo player.
        /// </summary>
        public void LaunchDemoPlayer()
        {
            // Actually, this is a good way of doing it, because we need to provide the ability
            // to watch demos offline, since quakelive.exe can be launched to watch demos without authenticating.
            _windowManager.ShowWindow(new DemoPlayerViewModel(_windowManager, _msgBoxService));
        }
    }
}