using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using UQLT.Models.Chat;

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

        public string FName
        {
            get
            {
                return RosterFriend.FriendName;
            }
        }

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

        [ImportingConstructor]
        public FriendViewModel(Friend friend, bool isautoexpanded)
        {
            RosterFriend = friend;
            IsAutoExpanded = isautoexpanded;
        }

    }
}
