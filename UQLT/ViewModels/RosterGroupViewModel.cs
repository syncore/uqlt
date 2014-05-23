using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using UQLT.Helpers;
using UQLT.Models.Chat;

namespace UQLT.ViewModels
{
    //-----------------------------------------------------------------------------------------------------
    [Export(typeof(RosterGroupViewModel))]
    //-----------------------------------------------------------------------------------------------------
    
        /// <summary>
    /// Individual roster group viewmodel. Wraps RosterGroup class and exposes additional properties specific to the View (in this case, ChatlistView)
    /// </summary>

    //-----------------------------------------------------------------------------------------------------
    public class RosterGroupViewModel : PropertyChangedBase
    {

        //-----------------------------------------------------------------------------------------------------
        public RosterGroup RostGroup
        {
            get;
            private set;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool IsAutoExpanded
        {
            get;
            private set;
        }

        //-----------------------------------------------------------------------------------------------------
        public string GroupName
        {
            get
            {
                return RostGroup.GroupName;
            }
        }

        private ObservableDictionary<string, FriendViewModel> _friends;

        public ObservableDictionary<string, FriendViewModel> Friends
        {
            get
            {
                return _friends;
            }

            set
            {
                _friends = value;
                // Not really needed, ObservableDictionary class handles INPC
                NotifyOfPropertyChange(() => Friends);
            }
        }

        //-----------------------------------------------------------------------------------------------------
        [ImportingConstructor]
        public RosterGroupViewModel(RosterGroup rostergroup, bool isautoexpanded)
        {
            RostGroup = rostergroup;
            IsAutoExpanded = isautoexpanded;
            _friends = new ObservableDictionary<string, FriendViewModel>();
        }

    }
}