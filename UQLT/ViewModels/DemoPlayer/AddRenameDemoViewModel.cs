using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;
using UQLT.Interfaces;
using UQLT.Models.DemoPlayer;

namespace UQLT.ViewModels.DemoPlayer
{
    /// <summary>
    /// Viewmodel reponsible for the Add/Rename demo view.
    /// </summary>
    [Export(typeof(AddRenameDemoViewModel))]
    public class AddRenameDemoViewModel : PropertyChangedBase, IHaveDisplayName, IViewAware
    {
        private readonly IMsgBoxService _msgBoxService;
        private Window _dialogWindow;
        private string _playlistName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddRenameDemoViewModel" /> class.
        /// </summary>
        /// <param name="dpvm">The <see cref="DemoPlayerViewModel" />associated with this viewmodel.</param>
        /// <param name="msgBoxService">The message box service.</param>
        /// <param name="isRename">if set to <c>true</c> then this instance is responsible for renaming the current playlist.</param>
        /// <remarks>
        /// This ctor is used for creating a new playlist.
        /// </remarks>
        public AddRenameDemoViewModel(DemoPlayerViewModel dpvm, IMsgBoxService msgBoxService, bool isRename)
        {
            DpVm = dpvm;
            _msgBoxService = msgBoxService;
            DisplayName = "Create a playlist";
            IsRename = isRename;
            Description = "Create a new playlist";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddRenameDemoViewModel" /> class.
        /// </summary>
        /// <param name="dpvm">The <see cref="DemoPlayerViewModel" />associated with this viewmodel.</param>
        /// <param name="playlistname">The playlist name.</param>
        /// <param name="msgBoxService">The message box service.</param>
        /// <param name="isRename">if set to <c>true</c> then this instance is responsible for renaming the current playlist.</param>
        /// <remarks>
        /// This ctor is used to rename an existing playlist.
        /// </remarks>
        public AddRenameDemoViewModel(DemoPlayerViewModel dpvm, string playlistname, IMsgBoxService msgBoxService, bool isRename)
        {
            DpVm = dpvm;
            _msgBoxService = msgBoxService;
            DisplayName = "Rename a playlist";
            PlaylistName = playlistname;
            IsRename = isRename;
            Description = string.Format("Rename playlist: {0}", playlistname);
        }

        /// <summary>
        /// Raised when a view is attached.
        /// </summary>
        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

        /// <summary>
        /// Gets or sets the text to be used in the view for what this viewmodel is responsible
        /// for doing (creating a playlist or renaming an existing playlist).
        /// </summary>
        /// <value>
        /// The description text.
        /// </value>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the title of this window.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="DemoPlayerViewModel"/>associated with this viewmodel.
        /// </summary>
        /// <value>
        /// The dp vm.
        /// </value>
        public DemoPlayerViewModel DpVm
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is responsible for
        /// renaming the current playlist.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance was called to rename an existing playlist, <c>false</c> if
        /// this instance was called to create a new playlist.
        /// </value>
        public bool IsRename
        {
            get;
            set;
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
                return _playlistName;
            }
            set
            {
                _playlistName = value;
                NotifyOfPropertyChange(() => PlaylistName);
            }
        }

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
                ViewAttached(this, new ViewAttachedEventArgs
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

        /// <summary>
        /// Perform the appropriate playlist action (rename or create).
        /// </summary>
        public void DoPlaylistAction()
        {
            if (string.IsNullOrEmpty(PlaylistName))
            {
                _msgBoxService.ShowError("The playlist name cannot be empty!", "Playlist name cannot be blank!");
                return;
            }
            if (IsRename)
            {
                RenamePlaylist();
            }
            else
            {
                CreatePlaylist();
            }
            // Close the window from the VM.
            CloseWin();
        }

        /// <summary>
        /// Gets a view previously attached to this instance.
        /// </summary>
        /// <param name="context">The context denoting which view to retrieve.</param>
        /// <returns>The view.</returns>
        public object GetView(object context = null)
        {
            return _dialogWindow;
        }

        /// <summary>
        /// Creates a new playlist.
        /// </summary>
        private void CreatePlaylist()
        {
            Debug.WriteLine(string.Format("Creating a playlist named: {0}", PlaylistName));
            var newPlaylist = new DemoPlaylistViewModel(new DemoPlaylist(PlaylistName));
            DpVm.Playlists.Add(newPlaylist);
            DpVm.SelectedPlaylist = newPlaylist;
        }

        /// <summary>
        /// Renames the existing playlist.
        /// </summary>
        private void RenamePlaylist()
        {
            Debug.WriteLine(string.Format("Renaming existing playlist to: {0}", PlaylistName));
            var newPlaylist = new DemoPlaylistViewModel(new DemoPlaylist(PlaylistName));
            foreach (var demo in DpVm.SelectedPlaylist.Demos)
            {
                newPlaylist.Demos.Add(demo);
            }
            DpVm.Playlists.Remove(DpVm.SelectedPlaylist);
            DpVm.Playlists.Add(newPlaylist);
            DpVm.SelectedPlaylist = newPlaylist;
        }
    }
}