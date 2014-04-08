using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;


namespace UQLT.ViewModels
{
    [Export(typeof(RosterViewModel))]
    // Container viewmodel for buddy list, no associated view
    public class RosterViewModel : PropertyChangedBase
    {
    public RosterGroupViewModel RosterGroupVM
        {
            get;
            private set;
        }

        public bool IsAutoExpanded { get; set; }
  

        [ImportingConstructor]
        public RosterViewModel(RosterGroupViewModel rostergroupvm, bool isautoexpanded)
        {
            RosterGroupVM = rostergroupvm;
            IsAutoExpanded = isautoexpanded;
        }
    
    }
}
