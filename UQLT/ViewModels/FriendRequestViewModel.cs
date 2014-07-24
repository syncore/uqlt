using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using agsXMPP;
using Caliburn.Micro;
using UQLT.Core.Chat;
using Uri = System.Uri;

namespace UQLT.ViewModels
{
    /// <summary>
    /// Viewmodel representing an incoming friend request, FriendRequestView.
    /// </summary>
    [Export(typeof(FriendRequestViewModel))]
    public class FriendRequestViewModel : IHaveDisplayName, IViewAware
    {
        private readonly ChatHandler _handler;

        private readonly Jid _jid;

        private readonly IWindowManager _windowManager;

        private Window _dialogWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="FriendRequestViewModel"/> class.
        /// </summary>
        /// <param name="jid">The jid.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="windowManager">The window manager.</param>
        [ImportingConstructor]
        public FriendRequestViewModel(Jid jid, ChatHandler handler, IWindowManager windowManager)
        {
            _jid = jid;
            _handler = handler;
            _windowManager = windowManager;
            DisplayName = "Incoming friend request from " + _jid.User;
        }

        /// <summary>
        /// Raised when a view is attached.
        /// </summary>
        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

        /// <summary>
        /// Gets or sets the name of this window.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets the friend image.
        /// </summary>
        /// <value>The friend image.</value>
        public ImageSource FriendImage
        {
            get
            {
                return new BitmapImage(new Uri("pack://application:,,,/UQLTRes;component/images/chat/friend.gif", UriKind.RelativeOrAbsolute));
            }
        }

        /// <summary>
        /// Gets the name of the user who is sending the incoming friend request.
        /// </summary>
        /// <value>
        /// The name of the user sending the incoming friend request.
        /// </value>
        public string RequestFrom
        {
            get
            {
                return _jid.User;
            }
        }

        /// <summary>
        /// Accepts the friend request.
        /// </summary>
        public void AcceptFriendRequest()
        {
            _handler.AcceptFriendRequest(_jid);
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
            _dialogWindow.Close();
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
        /// Rejects the friend request.
        /// </summary>
        public void RejectFriendRequest()
        {
            _handler.RejectFriendRequest(_jid);
        }
    }
}