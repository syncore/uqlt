using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using UQLT.Models.Chat;

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

        public string GroupName
        {
            get
            {
                return RostGroup.GroupName;
            }
        }

        private BindableCollection<FriendViewModel> _friends;
        public BindableCollection<FriendViewModel> Friends
        {
            get
            {
                return _friends;
            }
            set
            {
                _friends = value;
                NotifyOfPropertyChange(() => Friends);
            }
        }


        [ImportingConstructor]
        public RosterGroupViewModel(RosterGroup rostergroup, bool isautoexpanded)
        {
            RostGroup = rostergroup;
            IsAutoExpanded = isautoexpanded;
            _friends = new BindableCollection<FriendViewModel>();
        }

    }
}
