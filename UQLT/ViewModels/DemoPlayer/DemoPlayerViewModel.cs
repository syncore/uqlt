using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using UQLT.Core.Modules.DemoPlayer;
using UQLT.Helpers;
using UQLT.Interfaces;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;

namespace UQLT.ViewModels.DemoPlayer
{
    /// <summary>
    /// ViewModel for the DemoPlayerView
    /// </summary>
    [Export(typeof(DemoPlayerViewModel))]
    public class DemoPlayerViewModel : PropertyChangedBase, IHaveDisplayName, IDropTarget
    {
        private const string WolfCamDemoDir = "wolfcam-ql\\demos";
        private readonly IMsgBoxService _msgBoxService;
        private readonly IWindowManager _windowManager;
        private string _actionText;
        private double _archivingProgress;
        private bool _canCancelArchive = true;
        private bool _canCancelProcess = true;
        private string _cancelText = "Cancel";
        private bool _canPlayDemo;
        private ObservableCollection<DemoInfoViewModel> _demos = new ObservableCollection<DemoInfoViewModel>();
        private volatile bool _hasReceivedArchiveCancelation;
        private volatile bool _hasReceivedProcessCancelation;
        private bool _isArchivingDemos;
        private bool _isProcessingDemos;
        private ObservableCollection<DemoPlaylistViewModel> _playlists = new ObservableCollection<DemoPlaylistViewModel>();
        private double _processingProgress;
        private string _qlDemoDirectoryPath;
        private DemoInfoViewModel _selectedDemo;
        private DemoPlaylistViewModel _selectedPlaylist;
        private PlaylistDemoViewModel _selectedPlaylistDemo;
        private bool _showBusyIndicator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoPlayerViewModel" /> class.
        /// </summary>
        [ImportingConstructor]
        public DemoPlayerViewModel(IWindowManager windowManager, IMsgBoxService msgBoxService)
        {
            _windowManager = windowManager;
            _msgBoxService = msgBoxService;
            DisplayName = "UQLT v0.1 - Demo Player";
            DoDemoPlayerAutoSort("Filename");
            //TODO: Avoid Production hard-code. Detect if UQLT is being launched from Focus context and automatically set.
            QlDemoDirectoryPath = QLDirectoryUtils.GetQuakeLiveDemoDirectory(QuakeLiveTypes.Production);
            var s = StartupLoadDemoDatabase();
            //TODO: Avoid Production hard-code. Detect if UQLT is being launched from Focus context and automatically set.
            ScanForNewDemos(QuakeLiveTypes.Production);
        }

        /// <summary>
        /// Gets the text used to display what type of action is occurring (processing or archiving) in UI
        /// </summary>
        /// <value>
        /// The action text.
        /// </value>
        public string ActionText
        {
            get { return _actionText; }
            set
            {
                _actionText = value;
                NotifyOfPropertyChange(() => ActionText);
            }
        }

        /// <summary>
        /// Gets or sets the demo archiving progress.
        /// </summary>
        /// <value>
        /// The demo archiving progress.
        /// </value>
        public double ArchivingProgress
        {
            get
            {
                return _archivingProgress;
            }
            set
            {
                _archivingProgress = value;
                NotifyOfPropertyChange(() => ArchivingProgress);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can still cancel archiving.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can still cancel archiving; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>This is used to disable the Cancel button in the UI once the user has already canceled.</remarks>
        public bool CanCancelArchive
        {
            get
            {
                return _canCancelArchive;
            }
            set
            {
                _canCancelArchive = value;
                CancelText = value ? "Cancel" : "Canceling... Please wait...";
                NotifyOfPropertyChange(() => CanCancelArchive);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can still cancel processes.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can still cancel processes; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>This is used to disable the Cancel button in the UI once the user has already canceled.</remarks>
        public bool CanCancelProcess
        {
            get
            {
                return _canCancelProcess;
            }
            set
            {
                _canCancelProcess = value;
                CancelText = value ? "Cancel" : "Canceling... Please wait...";
                NotifyOfPropertyChange(() => CanCancelProcess);
            }
        }

        /// <summary>
        /// Gets or sets the text used for the cancel button based on whether a cancelation is pending.
        /// </summary>
        /// <value>
        /// The cancel text.
        /// </value>
        /// <remarks>This is the same for both cancellation of demo processing and archiving.</remarks>
        public string CancelText
        {
            get
            {
                return _cancelText;
            }
            set
            {
                _cancelText = value;
                NotifyOfPropertyChange(() => CancelText);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a demo is selected and can be played.
        /// </summary>
        /// <value>
        /// <c>true</c> if a demo is selected and can be played; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This is also a Caliburn.Micro action guard that automatically hooks up IsEnabled in the View.
        /// See: https://caliburnmicro.codeplex.com/wikipage?title=All%20About%20Actions
        /// </remarks>
        public bool CanPlayDemo
        {
            get { return _canPlayDemo; }
            set
            {
                _canPlayDemo = value;
                NotifyOfPropertyChange(() => CanPlayDemo);
            }
        }

        /// <summary>
        /// Gets or sets the user's demos that this viewmodel will display in the view.
        /// </summary>
        /// <value>The demos that this viewmodel will display in the view.</value>
        public ObservableCollection<DemoInfoViewModel> Demos
        {
            get
            {
                return _demos;
            }

            set
            {
                _demos = value;
                NotifyOfPropertyChange(() => Demos);
            }
        }

        /// <summary>
        /// Gets or Sets the display name for this window.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has received a demo archiving cancelation request.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has received a demo archiving cancelation request; otherwise, <c>false</c>.
        /// </value>
        public bool HasReceivedArchiveCancelation
        {
            get
            {
                return _hasReceivedArchiveCancelation;
            }
            set
            {
                _hasReceivedArchiveCancelation = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has received a demo process cancelation request.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has received a demo process cancelation request; otherwise, <c>false</c>.
        /// </value>
        public bool HasReceivedProcessCancelation
        {
            get
            {
                return _hasReceivedProcessCancelation;
            }
            set
            {
                _hasReceivedProcessCancelation = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether demos are currently being archived.
        /// </summary>
        /// <value>
        /// <c>true</c> if demos are currently being archived; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>This also sets the appropriate action text and
        /// notifies the UI of whether the busy indicator (ShowBusyIndicator) should be shown.
        /// </remarks>
        public bool IsArchivingDemos
        {
            get
            {
                return _isArchivingDemos;
            }
            set
            {
                _isArchivingDemos = value;
                _actionText = "Archiving demos... This might take a while.";
                ShowBusyIndicator = value;
                NotifyOfPropertyChange(() => IsArchivingDemos);
                NotifyOfPropertyChange(() => ActionText);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether demos are currently being processed.
        /// </summary>
        /// <value>
        /// <c>true</c> if demos are currently being processed; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>This also sets the appropriate action text and
        /// notifies the UI of whether the busy indicator (ShowBusyIndicator) should be shown.
        /// </remarks>
        public bool IsProcessingDemos
        {
            get
            {
                return _isProcessingDemos;
            }
            set
            {
                _isProcessingDemos = value;
                _actionText = "Processing demos... This might take a while.";
                ShowBusyIndicator = value;
                NotifyOfPropertyChange(() => IsProcessingDemos);
                NotifyOfPropertyChange(() => ActionText);
            }
        }

        /// <summary>
        /// Gets or sets the user's demo playlists.
        /// </summary>
        /// <value>
        /// The user's demo playlists.
        /// </value>
        public ObservableCollection<DemoPlaylistViewModel> Playlists
        {
            get
            {
                return _playlists;
            }
            set
            {
                _playlists = value;
                NotifyOfPropertyChange(() => Playlists);
            }
        }

        /// <summary>
        /// Gets or sets the demo processing progress.
        /// </summary>
        /// <value>
        /// The demo processing progress.
        /// </value>
        public double ProcessingProgress
        {
            get
            {
                return _processingProgress;
            }
            set
            {
                _processingProgress = value;
                NotifyOfPropertyChange(() => ProcessingProgress);
            }
        }

        /// <summary>
        /// Gets or sets the QL demo directory path.
        /// </summary>
        /// <value>The QL demo directory path.</value>
        public string QlDemoDirectoryPath
        {
            get { return _qlDemoDirectoryPath; }
            set
            {
                _qlDemoDirectoryPath = value;
                NotifyOfPropertyChange(() => QlDemoDirectoryPath);
            }
        }

        /// <summary>
        /// Gets or sets the selected demo.
        /// </summary>
        /// <value>The selected demo.</value>
        public DemoInfoViewModel SelectedDemo
        {
            get
            {
                return _selectedDemo;
            }

            set
            {
                _selectedDemo = value;
                CanPlayDemo = value != null;
                NotifyOfPropertyChange(() => SelectedDemo);
            }
        }

        /// <summary>
        /// Gets or sets the selected playlist.
        /// </summary>
        /// <value>
        /// The selected playlist.
        /// </value>
        public DemoPlaylistViewModel SelectedPlaylist
        {
            get
            {
                return _selectedPlaylist;
            }
            set
            {
                _selectedPlaylist = value;
                NotifyOfPropertyChange(() => SelectedPlaylist);
            }
        }

        /// <summary>
        /// Gets or sets the selected playlist demo.
        /// </summary>
        /// <value>
        /// The selected playlist demo.
        /// </value>
        public PlaylistDemoViewModel SelectedPlaylistDemo
        {
            get
            {
                return _selectedPlaylistDemo;
            }
            set
            {
                _selectedPlaylistDemo = value;
                NotifyOfPropertyChange(() => SelectedPlaylistDemo);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the transparent busy indicator should be shown
        /// in the UI, indicating that demo processing or archiving is taking place.
        /// </summary>
        /// <value>
        /// <c>true</c> if demo processing or archiving is taking place and indicator should be shown,
        /// otherwise, <c>false</c>.
        /// </value>
        public bool ShowBusyIndicator
        {
            get
            {
                return _showBusyIndicator;
            }
            set
            {
                _showBusyIndicator = value;
                NotifyOfPropertyChange(() => ShowBusyIndicator);
            }
        }

        /// <summary>
        /// Handles the addition of one or more demos.
        /// </summary>
        public async Task AddDemo()
        {
            using (var openfiledialog = new OpenFileDialog())
            {
                openfiledialog.CheckFileExists = true;
                openfiledialog.CheckPathExists = true;
                openfiledialog.Multiselect = true;
                openfiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openfiledialog.Filter = "Quake Live demo (*.dm_90)|*.dm_90|Quake Live demo (*.dm_73)|*.dm_73";

                if (openfiledialog.ShowDialog() != DialogResult.OK) return;

                var demofilepaths = new List<string>();
                demofilepaths.AddRange(openfiledialog.FileNames);

                // TODO: Fix the hard-coding of Production here. This method and the app in general need to detect if UQLT is being launched from Focus context.
                //await CopyDemosToQlDemoDirectoryAsync(QuakeLiveTypes.Production, demofilepaths);

                // send to list splitter/QLDemoDumper
                var dumper = new DemoDumper(this, _msgBoxService);
                var demos = dumper.CollectDemos(demofilepaths);
                dumper.ProcessDemos(demos);
            }
        }

        /// <summary>
        /// Handles the addition of a demo directory.
        /// </summary>
        public async Task AddDemoDirectory()
        {
            using (var openfolderdialog = new FolderBrowserDialog())
            {
                openfolderdialog.Description = "Select a directory containing .dm_90 and/or .dm_73 QL demo files.";
                openfolderdialog.ShowNewFolderButton = false;
                openfolderdialog.RootFolder = Environment.SpecialFolder.MyComputer;

                if (openfolderdialog.ShowDialog() != DialogResult.OK) return;

                if (AdditionalDirContainsDemoFiles(openfolderdialog.SelectedPath))
                {
                    Debug.WriteLine("Selected directory " + openfolderdialog.SelectedPath + " DOES contain demo files!");

                    // TODO: Fix the hard-coding of Production here. This method and the app in general need to detect if UQLT is being launched from Focus context.
                    //await CopyDemosToQlDemoDirectoryAsync(QuakeLiveTypes.Production, GetDemosFromSpecifiedDirectory(openfolderdialog.SelectedPath));

                    // send to list splitter/QLDemoDumper
                    var dumper = new DemoDumper(this, _msgBoxService);
                    var demos = dumper.CollectDemos(GetDemosFromSpecifiedDirectory(openfolderdialog.SelectedPath));
                    dumper.ProcessDemos(demos);
                }
                else
                {
                    _msgBoxService.ShowError(string.Format("{0} and its sub-directories do not contain any Quake Live demo files!",
                            openfolderdialog.SelectedPath), "No demo files found!");
                }
            }
        }

        /// <summary>
        /// Adds the demo to playlist.
        /// </summary>
        public void AddDemoToPlaylist()
        {
            if (SelectedDemo == null) { return; }
            if (SelectedPlaylist == null)
            {
                if (
                    _msgBoxService.AskConfirmationMessage(
                        "No playlist is selected would you like to create a new playlist?", "No playlist selected"))
                {
                    OpenCreatePlaylistWindow();
                }
            }
            else
            {
                bool allowDm73Demos = true;
                bool allowDm90Demos = true;

                foreach (var demo in SelectedPlaylist.Demos)
                {
                    if (IsProtocol73Demo(demo.Filename))
                    {
                        allowDm73Demos = true;
                        allowDm90Demos = false;
                    }
                    else if (IsProtocol90Demo(demo.Filename))
                    {
                        allowDm73Demos = false;
                        allowDm90Demos = true;
                    }
                    else
                    {
                        allowDm73Demos = true;
                        allowDm90Demos = true;
                    }
                }
                if ((IsProtocol73Demo(SelectedDemo.Filename) && allowDm73Demos) || (IsProtocol90Demo(SelectedDemo.Filename) && allowDm90Demos))
                {
                    SelectedPlaylist.Demos.Add(new PlaylistDemoViewModel(SelectedDemo.Demo));
                }
                else
                {
                    _msgBoxService.ShowError(string.Format("Playlist {0} is a {1} playlist and can only contain {1} demos!", SelectedPlaylist.PlaylistName, allowDm73Demos ? ".dm_73" : ".dm_90"), "Error");
                }
            }
        }

        /// <summary>
        /// Cancels all demo archiving.
        /// </summary>
        /// <remarks>This is called from the view itself.</remarks>
        public void CancelDemoArchiving()
        {
            HasReceivedArchiveCancelation = true;
            CanCancelArchive = false;
        }

        /// <summary>
        /// Cancels all demo processing.
        /// </summary>
        /// <remarks>This is called from the view itself.</remarks>
        public void CancelDemoProcessing()
        {
            HasReceivedProcessCancelation = true;
            CanCancelProcess = false;
        }

        /// <summary>
        /// Deletes the playlist.
        /// </summary>
        public void DeletePlaylist()
        {
            if (SelectedPlaylist == null) { return; }
            if (_msgBoxService.AskConfirmationMessage(string.Format("Are you sure you want to delete: {0}?", SelectedPlaylist), "Are you sure?"))
            {
                Playlists.Remove(SelectedPlaylist);
            }
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">Information about the drag.</param>
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (!(dropInfo.Data is PlaylistDemoViewModel) || !(dropInfo.TargetItem is PlaylistDemoViewModel)) return;
            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = System.Windows.DragDropEffects.Move;
        }

        /// <summary>
        /// Performs a drop.
        /// </summary>
        /// <param name="dropInfo">Information about the drop.</param>
        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            // Original drop handler logic
            var insertIndex = dropInfo.InsertIndex;
            var destinationList = DDropGetList(dropInfo.TargetCollection);
            var data = DDropExtractData(dropInfo.Data);

            if (dropInfo.DragInfo.VisualSource == dropInfo.VisualTarget)
            {
                var sourceList = DDropGetList(dropInfo.DragInfo.SourceCollection);

                foreach (var o in data)
                {
                    var index = sourceList.IndexOf(o);

                    if (index == -1) continue;
                    sourceList.RemoveAt(index);

                    if (sourceList == destinationList && index < insertIndex)
                    {
                        --insertIndex;
                    }
                }
            }
            foreach (var o in data)
            {
                destinationList.Insert(insertIndex++, o);
            }
            // Call our own configuration save method (write collection as it stands to json)
            Debug.WriteLine("Dropped!");
        }

        /// <summary>
        /// Opens the playlist creation window.
        /// </summary>
        /// <remarks>This is called from the view itself.</remarks>
        public void OpenCreatePlaylistWindow()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _windowManager.ShowWindow(new AddRenameDemoViewModel(this, _msgBoxService, false), null, settings);
        }

        /// <summary>
        /// Opens the demo options window.
        /// </summary>
        public void OpenDemoOptionsWindow()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _windowManager.ShowWindow(new DemoOptionsViewModel(), null, settings);
        }

        /// <summary>
        /// Opens the playlist rename window.
        /// </summary>
        /// <remarks>This is called from the view itself.</remarks>
        public void OpenRenamePlaylistWindow()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            // Might need to verify if a playlist is actually selected first.
            _windowManager.ShowWindow(new AddRenameDemoViewModel(this, SelectedPlaylist.PlaylistName, _msgBoxService, true), null, settings);
        }

        /// <summary>
        /// Plays the demo.
        /// </summary>
        /// <remarks>This is called from the view itself.</remarks>
        public async Task PlayDemo()
        {
            if (SelectedDemo == null) { return; }
            var filename = SelectedDemo.Filename;

            //TODO: Avoid Production hard-code. Detect if UQLT is being launched from Focus context and automatically set.
            var toCopy = new List<string> { Path.Combine(QLDirectoryUtils.GetQuakeLiveDemoDirectory(QuakeLiveTypes.Production), SelectedDemo.Filename) };

            if (IsProtocol90Demo(filename))
            {
                PlayDemoWithQuakeLive();
            }
            if (IsProtocol73Demo(filename))
            {
                VerifyWolfcamDemoDirectory();
                var options = new DemoOptionsViewModel();
                if (options.UseWolfcamQlForOldDemos)
                {
                    var wolfcamBase = Path.GetDirectoryName(options.WolfcamQlExePath);
                    if (wolfcamBase != null)
                    {
                        // copy
                        await CopyDemosToWolfcamDemoDirectoryAsync(toCopy, Path.Combine(wolfcamBase, WolfCamDemoDir));
                        // play
                        PlayDemoWithThirdPartyPlayer(DemoPlayerTypes.WolfcamQl, options.WolfcamQlExePath);
                    }
                    else
                    {
                        Debug.WriteLine("Problem finding WolfcamQL directory. Try resetting it in the options.");
                        _msgBoxService.ShowError("Problem finding WolfcamQL directory. Try resetting it in the options.", "Error");
                    }
                }
                else if (options.UseWolfWhispererForOldDemos)
                {
                    // Wolf Whisperer has a built-in mechanism that automatically copies a demo to its
                    // WolfcamQL\wolfcam-ql\demos directory. Manually copying it ourselves to that directory
                    // will prevent Wolf Whisperer from playing the demo. So let Wolf Whisperer handle it.
                    PlayDemoWithThirdPartyPlayer(DemoPlayerTypes.WolfWhisperer, options.WolfWhispererExePath);
                }
                else
                {
                    if (
                        _msgBoxService.AskConfirmationMessage(
                            "You have selected to play an old .dm_73 demo. You will need WolfcamQL or Wolf Whisperer to play this old demo." +
                            " Would you like to open the options to configure your WolfcamQL or Wolf Whisperer settings?",
                            "Demo player needed!"))
                    {
                        OpenDemoOptionsWindow();
                    }
                }
            }
        }

        /// <summary>
        /// Plays the playlist.
        /// </summary>
        /// <returns></returns>
        public async Task PlayPlaylist()
        {
            if (SelectedPlaylist == null) { return; }
            Debug.WriteLine("Will play demos in the following playlist and order:");
            foreach (var demo in SelectedPlaylist.Demos)
            {
                Debug.WriteLine(demo.Filename);
            }
        }

        /// <summary>
        /// Removes the demo from playlist.
        /// </summary>
        public void RemoveDemoFromPlaylist()
        {
            if (SelectedPlaylist == null) { return; }
            if (SelectedPlaylist == null) { return; }
            SelectedPlaylist.Demos.Remove(SelectedPlaylistDemo);
        }

        /// <summary>
        /// Rescans the user's demo directory for demos that are not contianed in the demo database,
        /// and sends them off for processing if necessary. This calls <see cref="ScanForNewDemos"/> from
        /// a button in the UI.
        /// </summary>
        /// <remarks>This is called directly from the view.</remarks>
        public void RescanDemoDir()
        {
            // TODO: Fix the hard-coding of Production here. This method and the app in general need to detect if UQLT is being launched from Focus context.
            // This will need to get a variable that will determine QuakeLive type since we don't want to pass this from UI button click
            Debug.WriteLine("....Re-scanning demo directory");
            ScanForNewDemos(QuakeLiveTypes.Production);
        }

        /// <summary>
        /// Checks whether the additional demo directory (and sub-directories) that the user has
        /// specified contains Quake Live demo files.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// <c>true</c> if the directory contains at least 1 file with the .dm_73 and/or .dm_90 extension.
        /// </returns>
        private bool AdditionalDirContainsDemoFiles(string path)
        {
            var files = GetDemosFromSpecifiedDirectory(path);

            foreach (string file in files)
            {
                Debug.WriteLine("demo found {0} from path: {1}", file, path);
            }

            return files.Count() > 0;
        }

        /// <summary>
        /// Asynchronously copies the demo files to Quake Live demo directory.
        /// </summary>
        /// <param name="qltype">The qltype.</param>
        /// <param name="demofilestocopy">The list of demo filenames to copy to the QL demo directory.</param>
        private async Task CopyDemosToQlDemoDirectoryAsync(QuakeLiveTypes qltype, IEnumerable<string> demofilestocopy)
        {
            foreach (string file in demofilestocopy)
            {
                var filename = file.Substring(file.LastIndexOf('\\'));
                using (FileStream sourceStream = File.Open(file, FileMode.Open))
                {
                    if (File.Exists(QLDirectoryUtils.GetQuakeLiveDemoDirectory(qltype) + filename))
                    {
                        if (_msgBoxService.AskConfirmationMessage(string.Format(
                            "{0} already exists in your QL demo folder. Would you like to overwrite this file in your QL demo folder?",
                            filename.Replace("\\", "")),
                            "File already exists!"))
                        {
                            using (
                            FileStream destinationStream =
                                File.Create(QLDirectoryUtils.GetQuakeLiveDemoDirectory(qltype) + filename))
                            {
                                Debug.WriteLine("Overwriting existing file in demo directory: " + filename);
                                await sourceStream.CopyToAsync(destinationStream);
                            }
                        }
                    }
                    else
                    {
                        using (
                            FileStream destinationStream =
                                File.Create(QLDirectoryUtils.GetQuakeLiveDemoDirectory(qltype) + filename))
                        {
                            Debug.WriteLine(filename.Replace("\\", "") +
                                            " did not exist in demo directory. Copying to demo directory");
                            await sourceStream.CopyToAsync(destinationStream);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Asynchronously copies the demo files to appropriate third-party demo player directory.
        /// </summary>
        /// <param name="demoFilesToCopy">The demo files to copy.</param>
        /// <param name="directory">The path to Wolfcam's demo directory.</param>
        /// <returns></returns>
        private async Task CopyDemosToWolfcamDemoDirectoryAsync(IEnumerable<string> demoFilesToCopy, string directory)
        {
            foreach (string file in demoFilesToCopy)
            {
                var filename = Path.GetFileName(file);
                Debug.WriteLine("File to copy to Wolfcam's demo directory is: " + filename);
                using (FileStream sourceStream = File.Open(file, FileMode.Open))
                {
                    if (filename != null && File.Exists(Path.Combine(directory, filename)))
                    {
                        if (_msgBoxService.AskConfirmationMessage(string.Format(
                            "{0} already exists in Wolfcam's demo folder. Would you like to overwrite this file in Wolfcam's demo folder?",
                            file),
                            "File already exists!"))
                        {
                            using (
                                FileStream destinationStream =
                                    File.Create(Path.Combine(directory, filename)))
                            {
                                Debug.WriteLine("Overwriting existing file in demo directory: " + filename);
                                await sourceStream.CopyToAsync(destinationStream);
                            }
                        }
                    }
                    else
                    {
                        if (filename == null) continue;
                        using (
                            FileStream destinationStream =
                                File.Create(Path.Combine(directory, filename)))
                        {
                            Debug.WriteLine(string.Format("{0} did not exist in demo directory. Copying to demo directory", filename));
                            await sourceStream.CopyToAsync(destinationStream);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extracts the drop data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The drop data as IEnumerable</returns>
        /// <remarks>Taken from the default drop handler in
        /// GongSolutions.Wpf.DragDrop\DefaultDropHandler.cs, original method "ExtractData"</remarks>
        private IEnumerable DDropExtractData(object data)
        {
            // Original drop handler logic
            if (data is IEnumerable && !(data is string))
            {
                return (IEnumerable)data;
            }
            else
            {
                return Enumerable.Repeat(data, 1);
            }
        }

        /// <summary>
        /// Gets the drop destination list.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns>The drop destination list.</returns>
        /// <remarks>Taken from the default drop handler in
        /// GongSolutions.Wpf.DragDrop\DefaultDropHandler.cs, original method "GetList"</remarks>
        private IList DDropGetList(IEnumerable enumerable)
        {
            // Original drop handler logic
            if (enumerable is ICollectionView)
            {
                return ((ICollectionView)enumerable).SourceCollection as IList;
            }
            else
            {
                return enumerable as IList;
            }
        }

        /// <summary>
        /// Performs the demo browser automatic sort based on specified criteria.
        /// </summary>
        /// <param name="property">The property criteria.</param>
        private void DoDemoPlayerAutoSort(string property)
        {
            var view = CollectionViewSource.GetDefaultView(Demos);
            var sortDescription = new SortDescription(property, ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortDescription);
        }

        /// <summary>
        /// Gets the demos from specified directory path and it's sub-directories.
        /// </summary>
        /// <param name="directorypath">The directory path.</param>
        /// <returns>A list of the demo filenames from a given input directory.</returns>
        private List<string> GetDemosFromSpecifiedDirectory(string directorypath)
        {
            List<string> demosfrompath =
                Directory.EnumerateFiles(directorypath, "*.*", SearchOption.AllDirectories)
                    .Where(
                        file =>
                            IsProtocol73Demo(file.ToLowerInvariant()) ||
                            IsProtocol90Demo(file.ToLowerInvariant())).ToList();

            return demosfrompath;
        }

        /// <summary>
        /// Determines whether the specified demo (filename) is of the older protocol 73 (dm_73) type.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns><c>true</c> if it is a protocol 73 .dm_73 demo, otherwise <c>false</c>.</returns>
        private bool IsProtocol73Demo(string filename)
        {
            return filename.EndsWith("dm_73", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the specified demo (filename) is of the newer protocol 90 (dm_90) type.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns><c>true</c> if it is a protocol 90 .dm_90 demo, otherwise <c>false</c>.</returns>
        private bool IsProtocol90Demo(string filename)
        {
            return filename.EndsWith("dm_90", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Plays the selected demo in the Quake Live client.
        /// </summary>
        private void PlayDemoWithQuakeLive()
        {
            var playerProcess = new Process();
            //TODO: Avoid Production hard-code. Detect if UQLT is being launched from Focus context and automatically set.
            var basePathQlForwardSlashFormat = QLDirectoryUtils.GetQuakeLiveBasePath(QuakeLiveTypes.Production).Replace("\\", "/");
            var homePathQlForwardSlashFormat = QLDirectoryUtils.GetQuakeLiveHomePath(QuakeLiveTypes.Production).Replace("\\", "/");
            try
            {
                //TODO: Avoid Production hard-code. Detect if UQLT is being launched from Focus context and automatically set.
                playerProcess.StartInfo.FileName = QLDirectoryUtils.GetQuakeLiveExePath(QuakeLiveTypes.Production);
                playerProcess.StartInfo.Arguments = string.Format("+set web_sess quakelive_sess= +set gt_user \"\"" +
                                                                  " +set gt_pass \"\" +set gt_realm \"quakelive\" +set fs_basepath \"{0}\" +set fs_homepath \"{1}\"" +
                                                                  " +demo {2}", basePathQlForwardSlashFormat, homePathQlForwardSlashFormat, SelectedDemo.Filename);
                playerProcess.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error launching Quake Live to view demo {0}, error: {1}", SelectedDemo.Filename, ex.Message);
                _msgBoxService.ShowError(string.Format("Error launching Quake Live to view demo {0}", SelectedDemo.Filename),
                        "Error launching Quke Live");
            }
        }

        /// <summary>
        /// Plays the demo with a third party player (WolfcamQL or Wolf Whisperer).
        /// </summary>
        /// <param name="playerType">Type of third-party demo player.</param>
        /// <param name="playerExePath">The path to the third-party demo player's executable file.</param>
        private void PlayDemoWithThirdPartyPlayer(DemoPlayerTypes playerType, string playerExePath)
        {
            var playerProcess = new Process();
            if (playerType == DemoPlayerTypes.WolfcamQl)
            {
                try
                {
                    playerProcess.StartInfo.FileName = playerExePath;
                    // File has already been copied from QL demo dir -> wolfcam demo dir.
                    // Now launching from wolfcamdir\wolfcam-ql\demos context at this point.
                    playerProcess.StartInfo.Arguments = string.Format("+demo {0}", SelectedDemo.Filename);
                    playerProcess.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to start WolfcamQL to play demo: {0}, error: {1}", SelectedDemo.Filename, ex.Message);
                    _msgBoxService.ShowError(string.Format("Unable to start WolfcamQL to play demo: {0}", SelectedDemo.Filename),
                        "Error starting WolfcamQL");
                }
            }
            else if (playerType == DemoPlayerTypes.WolfWhisperer)
            {
                try
                {
                    playerProcess.StartInfo.FileName = playerExePath;
                    //TODO: Avoid Production hard-code. Detect if UQLT is being launched from Focus context and automatically set.
                    // Wolf Whisperer expects full demo filepath surrounded by double quotes, passed as a command line argument to its executable.
                    playerProcess.StartInfo.Arguments = string.Format("\"{0}\"", Path.Combine(QLDirectoryUtils.GetQuakeLiveDemoDirectory(QuakeLiveTypes.Production), SelectedDemo.Filename));
                    playerProcess.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to start Wolf Whisperer to play demo: {0}, error: {1}", SelectedDemo.Filename, ex.Message);
                    _msgBoxService.ShowError(string.Format("Unable to start Wolf Whisperer to play demo: {0}", SelectedDemo.Filename),
                        "Error starting WolfcamQL");
                }
            }
        }

        /// <summary>
        /// Scans the user's demo directory for demos that are not contained in the demo database
        /// and sends them off for processing.
        /// </summary>
        /// <param name="qltype">The qltype.</param>
        private void ScanForNewDemos(QuakeLiveTypes qltype)
        {
            var currentDemoDirDemos = GetDemosFromSpecifiedDirectory(QLDirectoryUtils.GetQuakeLiveDemoDirectory(qltype));
            var populater = new DemoPopulate(this);
            var databaseDemos = populater.GetDatabaseDemos();

            var newDemosToProcess = new List<string>();
            foreach (var curDemo in currentDemoDirDemos)
            {
                // just filename
                var f = Path.GetFileName(curDemo);
                if (!databaseDemos.Contains(f))
                {
                    newDemosToProcess.Add(curDemo);
                    Debug.WriteLine(
                        string.Format(
                            "************ {0} is a new demo in the demo directory that should be added for processing.",
                            f));
                }
            }

            if (newDemosToProcess.Count == 0) return;
            var dumper = new DemoDumper(this, _msgBoxService);
            dumper.ProcessDemos(dumper.CollectDemos(newDemosToProcess));
        }

        /// <summary>
        /// Populates the user's demo list from the SQLite demo database file.
        /// </summary>
        private async Task StartupLoadDemoDatabase()
        {
            IsArchivingDemos = true;
            var demoPopulater = new DemoPopulate(this);
            await Task.Run(() => demoPopulater.PopulateDemoListFromDatabaseAsync());
            IsArchivingDemos = false;
        }

        /// <summary>
        /// Checks to see if the WolfcamQL demo directory exists, and creates it if it does not.
        /// </summary>
        private void VerifyWolfcamDemoDirectory()
        {
            var options = new DemoOptionsViewModel();
            if (options.UseWolfcamQlForOldDemos)
            {
                var wolfcamBase = Path.GetDirectoryName(options.WolfcamQlExePath);
                // should not be empty
                if (!string.IsNullOrEmpty(wolfcamBase))
                {
                    var wolfDemoDir = Path.Combine(wolfcamBase, WolfCamDemoDir);
                    if (!Directory.Exists(wolfDemoDir))
                    {
                        try
                        {
                            Directory.CreateDirectory(wolfDemoDir);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Unable to create WolfcamQL demo directory: " + ex.Message);
                        }
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("WolfcamQL demo directory already exists at {0}", wolfDemoDir));
                    }
                }
            }
        }
    }
}