using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace UQLT.ViewModels
{
    [Export(typeof(LoginViewModel))]
    public class LoginViewModel : PropertyChangedBase, IHaveDisplayName, IViewAware
    {
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _events;
        private string _displayName = "Login to Quake Live";
        private Window dialogWindow;
        
        [ImportingConstructor]
        public LoginViewModel(IWindowManager windowManager, IEventAggregator events)
        {
            _windowManager = windowManager;
            _events = events;
        }
        
        // different ways to implement window closing: http://stackoverflow.com/questions/10090584/how-to-close-dialog-window-from-viewmodel-caliburnwpf
        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        public void CloseWin()
        {
            dialogWindow.Close();
        }

        public void AttachView(object view, object context = null)
        {
            dialogWindow = view as Window;
            if (ViewAttached != null)
            {
                ViewAttached(this, new ViewAttachedEventArgs() { Context = context, View = view });
            }
        }

        public object GetView(object context = null)
        {
            return dialogWindow;
        }

        public void DoLogin()
        {
            // have some login logic here.. if successful then show main window and close this current window
            _windowManager.ShowWindow(new MainViewModel(_windowManager, _events));
            CloseWin();
        }
    }   
}
