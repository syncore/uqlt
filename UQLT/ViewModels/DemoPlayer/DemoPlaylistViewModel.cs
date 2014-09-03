using Caliburn.Micro;
using UQLT.Models.DemoPlayer;

namespace UQLT.ViewModels.DemoPlayer
{
    /// <summary>
    /// Viewmodel that wraps a <see cref="DemoPlaylist"/> model.
    /// </summary>
    public class DemoPlaylistViewModel : PropertyChangedBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DemoPlaylistViewModel"/> class.
        /// </summary>
        /// <param name="demoPlaylist">The demo playlist model wrapped by this viewmodel.</param>
        public DemoPlaylistViewModel(DemoPlaylist demoPlaylist)
        {
            this.DemoPlaylist = demoPlaylist;
        }

        public DemoPlaylist DemoPlaylist
        {
            get;
            private set;
        }

        public string PlaylistName
        {
            get
            {
                return DemoPlaylist.playlist_name;
            }
            set
            {
                DemoPlaylist.playlist_name = value;
                NotifyOfPropertyChange(() => PlaylistName);
            }
        }
    }
}