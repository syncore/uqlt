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
    /// ViewModel responsible for the "Add a Friend" view, AddAFriendView
    /// </summary>
    [Export(typeof(AddFriendViewModel))]
    public class AddFriendViewModel : PropertyChangedBase, IHaveDisplayName, IViewAware
    {
        private readonly ChatHandler _handler;
        private Window _dialogWindow;
        private string _friendToAdd;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddFriendViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public AddFriendViewModel(ChatHandler handler)
        {
            _handler = handler;
            DisplayName = "Add a friend";
        }

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
        /// Raised when a view is attached.
        /// </summary>
        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

        /// <summary>
        /// Gets or sets the name of this window.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the user to add to the contact list.
        /// </summary>
        /// <value>
        /// The user to add to the contact list.
        /// </value>
        public string FriendToAdd
        {
            get
            {
                return _friendToAdd;
            }
            set
            {
                _friendToAdd = value;
                NotifyOfPropertyChange(() => FriendToAdd);
            }
        }

        /// <summary>
        /// Adds the friend to the contact list.
        /// </summary>
        public void AddFriend()
        {
            if (IsValidQuakeLiveUser())
            {
                // Manual jid construction. Only the bare jid is needed here.
                var friend = FriendToAdd.ToLowerInvariant();
                var jid = new Jid(friend + "@" + UQltGlobals.QlXmppDomain);
                _handler.AddFriend(jid);

                MessageBox.Show(string.Format("Friend request sent to player {0}", FriendToAdd), "Friend request sent.",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Close the window from VM instead of view so the above success message can be sent.
                CloseWin();
            }
            else
            {
                MessageBox.Show(string.Format("Unable to add '{0}' because that is not a valid Quake Live player!", FriendToAdd), "Error: unable to add: " + FriendToAdd, MessageBoxButton.OK, MessageBoxImage.Error);
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
        /// Determines whether the specified user is a valid Quake Live player.
        /// </summary>
        /// <returns><c>true</c>, if the user is a valid QL player per quakelive.com, otherwise false.</returns>
        /// <remarks>This is verified whenever the user tries to add a contact to his buddy list.</remarks>
        private bool IsValidQuakeLiveUser()
        {
            return !string.IsNullOrEmpty(FriendToAdd);
        }
    }
}