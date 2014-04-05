using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.ComponentModel.Composition;
using UQLT.Models.Chat;
using System.Windows;

namespace UQLT.ViewModels
{
    [Export(typeof(ChatListDetailsViewModel))]

    // Individual buddy list information, no associated View
    public class ChatListDetailsViewModel : PropertyChangedBase
    {

        public RosterGroup RostGroup
        {
            get;
            private set;
        }

        public bool IsAutoExpanded { get; set; }

        // Wrapped properties
        public string GroupName
        {
            get
            {
                return RostGroup.GroupName;
            }
        }

        public BindableCollection<Friend> GroupFriends
        {
            get
            {
                return RostGroup.Friends;
            }
        }

        [ImportingConstructor]
        public ChatListDetailsViewModel(RosterGroup rostergroup, bool isautoexpanded)
        {
            RostGroup = rostergroup;
            IsAutoExpanded = isautoexpanded;
        }

    }
}
