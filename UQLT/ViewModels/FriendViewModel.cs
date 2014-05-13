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
    [Export(typeof(FriendViewModel))]

    // Individual friend viewmodel, no associated view
    public class FriendViewModel : PropertyChangedBase
    {

        // image types for player status
        private BitmapImage image_demo = new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/demo.gif", UriKind.RelativeOrAbsolute));
        private BitmapImage image_practice = new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/practice.gif", UriKind.RelativeOrAbsolute));
        private BitmapImage image_ingame = new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/ingame.gif", UriKind.RelativeOrAbsolute));

        
        public Friend RosterFriend
        {
            get;
            private set;
        }

        public string FriendName
        {
            get
            {
                return RosterFriend.FriendName;
            }
        }

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

        public string StatusServerId
        {
            get
            {
                return RosterFriend.StatusServerId;
            }
            set
            {
                RosterFriend.StatusServerId = value;
                NotifyOfPropertyChange(() => StatusServerId);
            }
        }

        public bool IsAutoExpanded
        {
            get;
            private set;
        }

        public ImageSource FriendImage
        {
            get
            {
                return new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/friend.gif", UriKind.RelativeOrAbsolute));

            }
        }

        public ImageSource FavoriteImage
        {
            get
            {
                return new BitmapImage(new System.Uri("pack://application:,,,/UQLTRes;component/images/chat/favorite.gif", UriKind.RelativeOrAbsolute));
            }
        }

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

        public string StatusGameType
        {
            get
            {
                return RosterFriend.StatusGameType;
            }
            
            set
            {
                RosterFriend.StatusGameType = value;
                NotifyOfPropertyChange(() => StatusGameType);
            }
        }

        public string StatusGameMap
        {
            get
            {
                return RosterFriend.StatusGameMap;
            }

            set
            {
                RosterFriend.StatusGameMap = value;
                NotifyOfPropertyChange(() => StatusGameMap);
            }
        }

        public string StatusGameLocation
        {
            get
            {
                return RosterFriend.StatusGameLocation;
            }

            set
            {
                RosterFriend.StatusGameLocation = value;
                NotifyOfPropertyChange(() => StatusGameLocation);
            }
        }

        public BitmapImage StatusGameFlag
        {
            get
            {
                return RosterFriend.StatusGameFlag;
            }
            set
            {
                RosterFriend.StatusGameFlag = value;
                NotifyOfPropertyChange(() => StatusGameFlag);
            }
        }
        
        public string StatusGamePlayerCount
        {
            get
            {
                return RosterFriend.StatusGamePlayerCount;
            }

            set
            {
                RosterFriend.StatusGamePlayerCount = value;
                NotifyOfPropertyChange(() => StatusGamePlayerCount);
            }
        }

        [ImportingConstructor]
        public FriendViewModel(Friend friend)
        {
            RosterFriend = friend;
            // required for treeview
            IsAutoExpanded = true;
        }

    }
}
