using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Caliburn.Micro;

namespace UQLT.Core
{
    /// <summary>
    /// Necessary Caliburn Micro boilerplate
    /// </summary>
    public class UqltWindowManager : WindowManager
    {
        /// <summary>
        /// Makes sure the view is a window or is wrapped by one.
        /// </summary>
        /// <param name="model">The view model.</param>
        /// <param name="view">The view.</param>
        /// <param name="isDialog">Whethor or not the window is being shown as a dialog.</param>
        /// <returns>The window.</returns>
        protected override Window EnsureWindow(object model, object view, bool isDialog)
        {
            Window window = base.EnsureWindow(model, view, isDialog);
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Activate();
            // Set an icon using code
            Uri iconUri = new Uri("pack://application:,,,/Icons/icon.ico", UriKind.RelativeOrAbsolute);
            window.Icon = BitmapFrame.Create(iconUri);
            return window;
        }
    }
}