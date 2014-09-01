using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using UQLT.Interfaces;

namespace UQLT.ViewModels
{
    /// <summary>
    /// View model for the demo options
    /// </summary>
    [Export(typeof(DemoOptionsViewModel))]
    public class DemoOptionsViewModel : PropertyChangedBase, IUqltConfiguration, IHaveDisplayName, IViewAware
    {
        private Window _dialogWindow;
        // options here

        public DemoOptionsViewModel()
        {
            DisplayNameAttribute = "Demo options";
            LoadConfig();
        }

        /// <summary>
        /// Raised when a view is attached.
        /// </summary>
        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

        /// <summary>
        /// Gets or Sets the name of this window.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Attaches a view to this instance.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The context in which the view appears.</param>
        public void AttachView(object view, object context = null)
        {
            _dialogWindow = view as Window;
            if (ViewAttached != null)
            {
                ViewAttached(this, new ViewAttachedEventArgs()
                {
                    Context = context,
                    View = view
                });
            }
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <remarks>For different ways to implement window closing, see: http://stackoverflow.com/questions/10090584/how-to-close-dialog-window-from-viewmodel-caliburnwpf</remarks>
        public void CloseWin()
        {
            _dialogWindow.Close();
        }
    }
}
