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
        private readonly IEventAggregator _events;
        private readonly IWindowManager _windowManager;

        public string DisplayName
        {
            get { return _displayName;  }
            set { _displayName = value; }
        }

        public FilterViewModel FilterViewModel { get; private set; }
        
        [ImportingConstructor]
    public MainViewModel(IWindowManager WindowManager, IEventAggregator events)
    {
        _windowManager = WindowManager;
        _events = events;
        this.FilterViewModel = new FilterViewModel(_events);
    }

        public void HideFilters()
        {
            Console.WriteLine("Attempting to publish event");
            _events.Publish(new FilterVisibilityEvent(false));
            //_events.Publish(new FilterVisibilityEvent { FilterViewVisibility = false});
        }
    }
}
