using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using UQLT.Core.Chat;
using UQLT.Models.Chat;

namespace UQLT.ViewModels
{
    /// <summary>
    /// Individual friend viewmodel. This class wraps the Friend class and exposes additional
    /// properties specific to the View (in this case, ChatListView).
    /// </summary>
    [Export(typeof(FriendViewModel))]
    public class FriendViewModel : PropertyChangedBase
    {
        private readonly BitmapImage _imageDemo = new BitmapImage(new Uri("pack://application:,,,/UQLTRes;component/images/chat/demo.gif", UriKind.RelativeOrAbsolute));
        private readonly BitmapImage _imageIngame = new BitmapImage(new Uri("pack://application:,,,/UQLTRes;component/images/chat/ingame.gif", UriKind.RelativeOrAbsolute));
        private readonly BitmapImage _imagePractice = new BitmapImage(new Uri("pack://application:,,,/UQLTRes;component/images/chat/practice.gif", UriKind.RelativeOrAbsolute));
        private ServerDetailsViewModel _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="FriendViewModel" /> class.
        /// </summary>
        /// <param name="friend">The friend.</param>
        [ImportingConstructor]
        public FriendViewModel(Friend friend)
        {
            RosterFriend = friend;
            // Auto expansion is required for treeview
            IsAutoExpanded = true;
        }

        /// <summary>
        /// Gets or sets this friend's XMPP resource.
        /// </summary>
        /// <value>The XMPP resource.</value>
        /// <remarks>This will either be "uqlt" or "quakelive"</remarks>
        public string ActiveXmppResource
        {
            get
            {
                return RosterFriend.ActiveXmppResource;
            }
            set
            {
                RosterFriend.ActiveXmppResource = value;
                NotifyOfPropertyChange(() => ActiveXmppResource);
            }
        }

        /// <summary>
        /// Gets the favorite image.
        /// </summary>
        /// <value>The favorite image.</value>
        public ImageSource FavoriteImage
        {
            get
            {
                return new BitmapImage(new Uri("pack://application:,,,/UQLTRes;component/images/chat/favorite.gif", UriKind.RelativeOrAbsolute));
            }
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
        /// Gets the name of the friend.
        /// </summary>
        /// <value>The name of the friend.</value>
        public string FriendName
        {
            get
            {
                return RosterFriend.FriendName;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this friend has multiple XMPP clients.
        /// </summary>
        /// <value><c>true</c> if this friend has multiple XMPP clients; otherwise, <c>false</c>.</value>
        public bool HasMultipleXmppClients
        {
            get
            {
                return RosterFriend.HasMultipleXmppClients;
            }
            set
            {
                RosterFriend.HasMultipleXmppClients = value;
                NotifyOfPropertyChange(() => HasMultipleXmppClients);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the friend has a XMPP status.
        /// </summary>
        /// <value><c>true</c> if this instance has XMPP status; otherwise, <c>false</c>.</value>
        public bool HasXmppStatus
        {
            get
            {
                return RosterFriend.HasXmppStatus;
            }
            set
            {
                RosterFriend.HasXmppStatus = value;
                NotifyOfPropertyChange(() => HasXmppStatus);
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
        /// Gets or sets a value indicating whether this friend is a favorite.
        /// </summary>
        /// <value><c>true</c> if this friend is a favorite; otherwise, <c>false</c>.</value>
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
        /// <value><c>true</c> if this friend is in game; otherwise, <c>false</c>.</value>
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
        /// <value><c>true</c> if this friend is online; otherwise, <c>false</c>.</value>
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
                    case TypeOfStatus.WatchingDemo:
                        return true;

                    case TypeOfStatus.PlayingPracticeGame:
                        return true;

                    case TypeOfStatus.PlayingRealGame:
                        return false;

                    case TypeOfStatus.Nothing:
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Gets the message to display if friend is in practice match or viewing a demo.
        /// </summary>
        /// <value>The message.</value>
        public string PracticeDemoMessage
        {
            get
            {
                switch (StatusType)
                {
                    case TypeOfStatus.WatchingDemo:
                        return "Watching a demo";

                    case TypeOfStatus.PlayingPracticeGame:
                        return "Playing a practice match";

                    case TypeOfStatus.Nothing:
                    case TypeOfStatus.PlayingRealGame:
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// Gets the friend that this viewmodel wraps.
        /// </summary>
        /// <value>The friend that this viewmodel wraps.</value>
        public Friend RosterFriend
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the friend's server.
        /// </summary>
        /// <value>The friend's Quake Live server object.</value>
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
        /// Gets the status image.
        /// </summary>
        /// <value>The status image.</value>
        public ImageSource StatusImage
        {
            get
            {
                switch (StatusType)
                {
                    case TypeOfStatus.WatchingDemo:
                        return _imageDemo;

                    case TypeOfStatus.PlayingPracticeGame:
                        return _imagePractice;

                    case TypeOfStatus.PlayingRealGame:
                        return _imageIngame;

                    case TypeOfStatus.Nothing:
                    default:
                        return default(BitmapImage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the status.
        /// </summary>
        /// <value>The type of the status.</value>
        public TypeOfStatus StatusType
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
        /// Gets or sets the set of this friend's XMPP resources.
        /// </summary>
        /// <value>The set of this friend's XMPP resources.</value>
        public HashSet<string> XmppResources
        {
            get
            {
                return RosterFriend.XmppResources;
            }
            set
            {
                RosterFriend.XmppResources = value;
                NotifyOfPropertyChange(() => XmppResources);
            }
        }

        /// <summary>
        /// Gets or sets the status
        /// </summary>
        /// <value>
        /// The status
        /// </value>
        public string Status
        {
            get { return RosterFriend.Status; }
            set { RosterFriend.Status = value; }
        }
    }
}