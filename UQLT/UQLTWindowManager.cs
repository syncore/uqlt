using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using System.Windows.Media.Imaging;

namespace UQLT
{
    //-----------------------------------------------------------------------------------------------------
    /// <summary>
    /// Necessary Caliburn Micro boilerplate
    /// </summary>
    public class UQLTWindowManager : WindowManager
    {
        //-----------------------------------------------------------------------------------------------------
        protected override System.Windows.Window EnsureWindow(object model, object view, bool isDialog)
        {
            Window window = base.EnsureWindow(model, view, isDialog);
            window.SizeToContent = SizeToContent.WidthAndHeight;
            // Set an icon using code
            Uri iconUri = new Uri("pack://application:,,,/icon.ico", UriKind.RelativeOrAbsolute);
            window.Icon = BitmapFrame.Create(iconUri);
            return window;
        }
    }
}