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

        /*public bool HasStatus
        {
            get
            {
            return (string.IsNullOrEmpty(RosterFriend.Status))
            }
        }*/

        public string Status
        {
            get
            {
                return RosterFriend.Status;
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
 
        public bool IsAutoExpanded { get; set; }

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
        
        [ImportingConstructor]
        public FriendViewModel(Friend friend, bool isautoexpanded)
        {
            RosterFriend = friend;
            IsAutoExpanded = isautoexpanded;
        }

    }
}
