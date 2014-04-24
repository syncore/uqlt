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

        public bool HasStatus
        {
            get
            {
            //return (string.IsNullOrEmpty(RosterFriend.Status))
                return RosterFriend.HasStatus;
            }
            set
            {
                RosterFriend.HasStatus = value;
                NotifyOfPropertyChange(() => HasStatus);
            }

        }

        public string Status
        {
            get
            {
                return RosterFriend.Status;
            }
            set
            {
                RosterFriend.Status = value;
                NotifyOfPropertyChange(() => Status);
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
                    case 0:
                    default:
                        return default(BitmapImage);
                    case 1:
                        return image_demo;
                    case 2:
                        return image_practice;
                    case 3:
                        return image_ingame;
                    
                }
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
