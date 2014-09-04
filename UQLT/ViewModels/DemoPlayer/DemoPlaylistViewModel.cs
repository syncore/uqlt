using System.Collections.ObjectModel;
using Caliburn.Micro;
using UQLT.Models.DemoPlayer;

namespace UQLT.ViewModels.DemoPlayer
{
    /// <summary>
    /// Viewmodel that wraps a <see cref="DemoPlaylist"/> model.
    /// </summary>
    public class DemoPlaylistViewModel : PropertyChangedBase
    {
        private ObservableCollection<PlaylistDemoViewModel> _demos = new ObservableCollection<PlaylistDemoViewModel>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DemoPlaylistViewModel"/> class.
        /// </summary>
        /// <param name="demoPlaylist">The demo playlist model wrapped by this viewmodel.</param>
        public DemoPlaylistViewModel(DemoPlaylist demoPlaylist)
        {
            this.DemoPlaylist = demoPlaylist;
        }

        /// <summary>
        /// Gets the demo playlist class that this viewmodel wraps.
        /// </summary>
        /// <value>
        /// The demo playlist wrapped by this viewmodel.
        /// </value>
        public DemoPlaylist DemoPlaylist
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the name of the playlist.
        /// </summary>
        /// <value>
        /// The name of the playlist.
        /// </value>
        public string PlaylistName
        {
            get
            {
                return DemoPlaylist.Name;
            }
            set
            {
                DemoPlaylist.Name = value;
                NotifyOfPropertyChange(() => PlaylistName);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return PlaylistName;
        }

        /// <summary>
        /// Gets or sets the demos contained in this playlist.
        /// </summary>
        /// <value>
        /// The demos contained in this playlist.
        /// </value>
        public ObservableCollection<PlaylistDemoViewModel> Demos
        {
            get
            {
                return _demos;
            }
            set
            {
                _demos = value;
                // necessary?
                NotifyOfPropertyChange(() => Demos);
            }
        }
    }
}