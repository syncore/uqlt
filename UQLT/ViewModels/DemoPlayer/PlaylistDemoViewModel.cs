using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using UQLT.Models.DemoPlayer;

namespace UQLT.ViewModels.DemoPlayer
{
    /// <summary>
    /// Viewmodel that wraps a <see cref="PlaylistDemo"/>
    /// </summary>
    public class PlaylistDemoViewModel : PropertyChangedBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistDemoViewModel" /> class.
        /// </summary>
        /// <param name="demo">The demo wrapped by this <see cref="PlaylistDemoViewModel"/></param>
        public PlaylistDemoViewModel(Demo demo)
        {
            this.Demo = demo;
        }

        public Demo Demo
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the demo filename.
        /// </summary>
        /// <value>
        /// The filename.
        /// </value>
        public string Filename
        {
            get { return Demo.filename; }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Filename;
        }
    }
}
