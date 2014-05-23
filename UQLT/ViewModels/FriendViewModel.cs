using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using UQLT.Models.Chat;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UQLT.ViewModels
{
    //-----------------------------------------------------------------------------------------------------
    [Export(typeof(FriendViewModel))]
    //-----------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// Individual friend viewmodel. Wraps Friend class and exposes additional properties specific to the View (in this case, ChatlistView)
    /// </summary>
    public class FriendViewModel : PropertyChangedBase
    {

        // image types for player status
        private BitmapImage image_demo = new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/demo.gif", UriKind.RelativeOrAbsolute));
        private BitmapImage image_practice = new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/practice.gif", UriKind.RelativeOrAbsolute));
        private BitmapImage image_ingame = new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/ingame.gif", UriKind.RelativeOrAbsolute));

        //-----------------------------------------------------------------------------------------------------
        public Friend RosterFriend
        {
            get;
            private set;
        }

        private ServerDetailsViewModel _server;
        //-----------------------------------------------------------------------------------------------------
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

        //-----------------------------------------------------------------------------------------------------
        public string FriendName
        {
            get
            {
                return RosterFriend.FriendName;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public bool HasXMPPStatus
        {
            get
            {
                //return (string.IsNullOrEmpty(RosterFriend.Status))
                return RosterFriend.HasXMPPStatus;
            }
            set
            {
                RosterFriend.HasXMPPStatus = value;
                NotifyOfPropertyChange(() => HasXMPPStatus);
            }

        }

        //-----------------------------------------------------------------------------------------------------
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

        //-----------------------------------------------------------------------------------------------------
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

        //-----------------------------------------------------------------------------------------------------
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

        //-----------------------------------------------------------------------------------------------------
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

        //-----------------------------------------------------------------------------------------------------
        public bool IsAutoExpanded
        {
            get;
            private set;
        }

        //-----------------------------------------------------------------------------------------------------
        public ImageSource FriendImage
        {
            get
            {
                return new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/friend.gif", UriKind.RelativeOrAbsolute));

            }
        }

        //-----------------------------------------------------------------------------------------------------
        public ImageSource FavoriteImage
        {
            get
            {
                return new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/favorite.gif", UriKind.RelativeOrAbsolute));
            }
        }

        //-----------------------------------------------------------------------------------------------------
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

        //-----------------------------------------------------------------------------------------------------
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

        //-----------------------------------------------------------------------------------------------------
        [ImportingConstructor]
        public FriendViewModel(Friend friend)
        {
            RosterFriend = friend;
            // required for treeview
            IsAutoExpanded = true;
        }

    }
}