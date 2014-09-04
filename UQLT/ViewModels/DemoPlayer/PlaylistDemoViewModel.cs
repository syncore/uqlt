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
        /// Initializes a new instance of the <see cref="PlaylistDemoViewModel"/> class.
        /// </summary>
        /// <param name="playlistDemo">The <see cref="PlaylistDemo"/> that this viewmodel wraps.</param>
        public PlaylistDemoViewModel(PlaylistDemo playlistDemo)
        {
            this.PlaylistDemo = playlistDemo;
        }

        public PlaylistDemo PlaylistDemo
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
            get
            {
                return PlaylistDemo.filename;
            }
            set
            {
                PlaylistDemo.filename = value;
                NotifyOfPropertyChange(() => Filename);
            }
        }
    }
}
