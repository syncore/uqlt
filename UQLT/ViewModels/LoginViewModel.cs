using System;
using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;

namespace UQLT.ViewModels
{
    [Export(typeof(LoginViewModel))]

    /// <summary>
    /// The viewmodel for the LoginView, which serves as the starting point that the user sees when
    /// launching the application.
    /// </summary>
    public class LoginViewModel : PropertyChangedBase, IHaveDisplayName, IViewAware
    {
        private readonly IEventAggregator _events;
        private readonly IWindowManager _windowManager;
        private string _displayName = "Login to Quake Live";
        private Window dialogWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel" /> class.
        /// </summary>
        /// <param name="windowManager">The window manager.</param>
        /// <param name="events">The events that this viewmodel publishes and/or subscribes to.</param>
        [ImportingConstructor]
        public LoginViewModel(IWindowManager windowManager, IEventAggregator events)
        {
            _windowManager = windowManager;
            _events = events;
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
            dialogWindow = view as Window;
            if (ViewAttached != null)
            {
                ViewAttached(this, new ViewAttachedEventArgs()
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
            dialogWindow.Close();
        }

        /// <summary>
        /// Does the login.
        /// </summary>
        public void DoLogin()
        {
            // TODO: have some login logic here.. if successful then show main window and close this
            //       current window
            _windowManager.ShowWindow(new MainViewModel(_windowManager, _events));
            CloseWin();
        }

        /// <summary>
        /// Gets a view previously attached to this instance.
        /// </summary>
        /// <param name="context">The context denoting which view to retrieve.</param>
        /// <returns>The view.</returns>
        public object GetView(object context = null)
        {
            return dialogWindow;
        }
    }
}