using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace UQLT
{
    public class UQLTWindowManager : WindowManager
    {
        protected override System.Windows.Window EnsureWindow(object model, object view, bool isDialog)
        {
            Window window = base.EnsureWindow(model, view, isDialog);
            window.SizeToContent = SizeToContent.WidthAndHeight;
            return window;
        }
    }
}
