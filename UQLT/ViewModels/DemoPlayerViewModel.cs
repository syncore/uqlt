using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.ViewModels
{
    /// <summary>
    /// ViewModel for the DemoPlayerView
    /// </summary>
    [Export(typeof(DemoPlayerViewModel))]
    public class DemoPlayerViewModel
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoPlayerViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public DemoPlayerViewModel()
        {
            
        }
    }
}
