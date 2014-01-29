using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using UQLT.Events;
using System.ComponentModel.Composition;
using System.Collections.Concurrent;

namespace UQLT.ViewModels
{
    [Export (typeof(MainViewModel))]
    public class MainViewModel : PropertyChangedBase, IHaveDisplayName
    {
        private string _displayName = "UQLT v0.1";
        private FilterViewModel _FilterViewModel;
        private SBMasterViewModel _SBMasterViewModel;
        private readonly IEventAggregator _events;
        private readonly IWindowManager _windowManager;
        
        public string DisplayName
        {
            get { return _displayName;  }
            set { _displayName = value; }
        }

        public FilterViewModel FilterViewModel {
            get { return _FilterViewModel; }
            set { _FilterViewModel = value; }
        }
        public SBMasterViewModel SBMasterViewModel
        {
            get { return _SBMasterViewModel; }
            set { _SBMasterViewModel = value; }
        }
        
        [ImportingConstructor]
    public MainViewModel(IWindowManager WindowManager, IEventAggregator events)
    {
        _windowManager = WindowManager;
        _events = events;
        _FilterViewModel = new FilterViewModel(_events);
        _SBMasterViewModel = new SBMasterViewModel();
    }

        public void HideFilters()
        {
            //Console.WriteLine("Attempting to publish event");
            if (_FilterViewModel.isVisible)
            {
                _events.Publish(new FilterVisibilityEvent(false));
            }
            else
            {
                _events.Publish(new FilterVisibilityEvent(true));
            }

        }
    }
}
