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
    [Export(typeof(RosterGroupViewModel))]

    // Individual rostergroup viewmodel, no associated View
    public class RosterGroupViewModel : PropertyChangedBase
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

        private BindableCollection<FriendViewModel> _testFriends;
        public BindableCollection<FriendViewModel> TestFriends {
            get
            {
                return _testFriends;
            }
            set
            {
                _testFriends = value;
                NotifyOfPropertyChange(() => TestFriends);
            }
        }

        public void AddFriend(FriendViewModel fvm)
        {
            if (!TestFriends.Contains(fvm))
                TestFriends.Add(fvm);
        }

        private BindableCollection<FriendViewModel> AddFriends(BindableCollection<Friend> friends)
        {
            //_testFriends.Clear();
            foreach (var friend in friends)
            {
                _testFriends.Add(new FriendViewModel(friend, true));
            }

            return _testFriends;
        }

        public BindableCollection<Friend> GroupFriends
        {
            get
            {
                return RostGroup.Friends;
            }
        }

        [ImportingConstructor]
        public RosterGroupViewModel(RosterGroup rostergroup, bool isautoexpanded)
        {
            RostGroup = rostergroup;
            IsAutoExpanded = isautoexpanded;
            _testFriends = new BindableCollection<FriendViewModel>();
            _testFriends = AddFriends(rostergroup.Friends);
        }

    }
}
