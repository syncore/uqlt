using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using UQLT.Events;
using System.ComponentModel.Composition;

namespace UQLT.ViewModels
{
    [Export (typeof(MainViewModel))]
    public class MainViewModel : PropertyChangedBase, IHaveDisplayName
    {
        private string _displayName = "UQLT v0.1";
        private FilterViewModel _FilterViewModel;
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
        
        [ImportingConstructor]
    public MainViewModel(IWindowManager WindowManager, IEventAggregator events)
    {
        _windowManager = WindowManager;
        _events = events;
        _FilterViewModel = new FilterViewModel(_events);
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
