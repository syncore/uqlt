using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using UQLT.Models.Chat;

namespace UQLT.ViewModels
{
    [Export(typeof(FriendViewModel))]

    /// <summary>
    /// Individual friend viewmodel. This class wraps the Friend class and exposes additional properties specific to the View (in this case, ChatListView)
    /// </summary>
    public class FriendViewModel : PropertyChangedBase
    {
        // Image types for player status.
        private BitmapImage image_demo = new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/demo.gif", UriKind.RelativeOrAbsolute));

        private BitmapImage image_practice = new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/practice.gif", UriKind.RelativeOrAbsolute));
        private BitmapImage image_ingame = new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/ingame.gif", UriKind.RelativeOrAbsolute));

        /// <summary>
        /// Gets the friend that this viewmodel wraps.
        /// </summary>
        /// <value>
        /// The friend that this viewmodel wraps.
        /// </value>
        public Friend RosterFriend
        {
            get;
            private set;
        }

        private ServerDetailsViewModel _server;

        /// <summary>
        /// Gets or sets the friend's server.
        /// </summary>
        /// <value>
        /// The friend's Quake Live server object.
        /// </value>
        public ServerDetailsViewModel Server
        {
            get
            {
                return _server;
            }
            set
            {
                _server = value;
                NotifyOfPropertyChange(() => Server);
            }
        }

        /// <summary>
        /// Gets the name of the friend.
        /// </summary>
        /// <value>
        /// The name of the friend.
        /// </value>
        public string FriendName
        {
            get
            {
                return RosterFriend.FriendName;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the friend has a XMPP status.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has XMPP status; otherwise, <c>false</c>.
        /// </value>
        public bool HasXMPPStatus
        {
            get
            {
                return RosterFriend.HasXMPPStatus;
            }
            set
            {
                RosterFriend.HasXMPPStatus = value;
                NotifyOfPropertyChange(() => HasXMPPStatus);
            }
        }

        /// <summary>
        /// Gets or sets the type of the status.
        /// </summary>
        /// <value>
        /// The type of the status.
        /// </value>
        public int StatusType
        {
            get
            {
                return RosterFriend.StatusType;
            }
            set
            {
                RosterFriend.StatusType = value;
                NotifyOfPropertyChange(() => StatusType);
                NotifyOfPropertyChange(() => StatusImage);
                NotifyOfPropertyChange(() => PracticeDemoMessage);
                NotifyOfPropertyChange(() => IsPracticeOrDemo);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this friend is a favorite.
        /// </summary>
        /// <value>
        /// <c>true</c> if this friend is a favorite; otherwise, <c>false</c>.
        /// </value>
        public bool IsFavorite
        {
            get
            {
                return RosterFriend.IsFavorite;
            }
            set
            {
                RosterFriend.IsFavorite = value;
                NotifyOfPropertyChange(() => IsFavorite);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this friend is in game.
        /// </summary>
        /// <value>
        /// <c>true</c> if this friend is in game; otherwise, <c>false</c>.
        /// </value>
        public bool IsInGame
        {
            get
            {
                return RosterFriend.IsInGame;
            }
            set
            {
                RosterFriend.IsInGame = value;
                NotifyOfPropertyChange(() => IsInGame);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this friend is online.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this friend is online; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnline
        {
            get
            {
                return RosterFriend.IsOnline;
            }
            set
            {
                RosterFriend.IsOnline = value;
                NotifyOfPropertyChange(() => IsOnline);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this friend is in a practice match or viewing a demo.
        /// </summary>
        /// <value>
        /// <c>true</c> if this friend is in a practice match or viewing a demo; otherwise, <c>false</c>.
        /// </value>
        public bool IsPracticeOrDemo
        {
            get
            {
                switch (StatusType)
                {
                    case 1:
                        return true;

                    case 2:
                        return true;

                    case 3:
                        return false;

                    case 0:
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this friend is automatically expanded on the chatlist.
        /// </summary>
        /// <value>
        /// <c>true</c> if this friend is automatically expanded on the chatlist; otherwise, <c>false</c>.
        /// </value>
        public bool IsAutoExpanded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the friend image.
        /// </summary>
        /// <value>
        /// The friend image.
        /// </value>
        public ImageSource FriendImage
        {
            get
            {
                return new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/friend.gif", UriKind.RelativeOrAbsolute));
            }
        }

        /// <summary>
        /// Gets the favorite image.
        /// </summary>
        /// <value>
        /// The favorite image.
        /// </value>
        public ImageSource FavoriteImage
        {
            get
            {
                return new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/favorite.gif", UriKind.RelativeOrAbsolute));
            }
        }

        /// <summary>
        /// Gets the status image.
        /// </summary>
        /// <value>
        /// The status image.
        /// </value>
        public ImageSource StatusImage
        {
            get
            {
                switch (StatusType)
                {
                    case 1:
                        return image_demo;

                    case 2:
                        return image_practice;

                    case 3:
                        return image_ingame;

                    case 0:
                    default:
                        return default(BitmapImage);
                }
            }
        }

        /// <summary>
        /// Gets the message to display if friend is in practice match or viewing a demo.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string PracticeDemoMessage
        {
            get
            {
                switch (StatusType)
                {
                    case 1:
                        return "Watching a demo";

                    case 2:
                        return "Playing a practice match";

                    case 0:
                    case 3:
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FriendViewModel"/> class.
        /// </summary>
        /// <param name="friend">The friend.</param>
        [ImportingConstructor]
        public FriendViewModel(Friend friend)
        {
            RosterFriend = friend;
            // required for treeview
            IsAutoExpanded = true;
        }
    }
}