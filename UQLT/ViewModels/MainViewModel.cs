using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.ComponentModel.Composition;

namespace UQLT.ViewModels
{
    [Export (typeof(MainViewModel))]
    public class MainViewModel : PropertyChangedBase, IHaveDisplayName
    {
        private string _displayName = "UQLT v0.1";
        private readonly IWindowManager _windowManager;

        public string DisplayName
        {
            get { return _displayName;  }
            set { _displayName = value; }
        }
        
        [ImportingConstructor]
    public MainViewModel(IWindowManager WindowManager)
    {
        _windowManager = WindowManager;
    }
    
    }
}
