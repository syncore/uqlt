using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using Caliburn.Micro;
using UQLT.Core.Modules.DemoPlayer;
using UQLT.Helpers;
using MessageBox = System.Windows.MessageBox;

namespace UQLT.ViewModels
{
    /// <summary>
    /// ViewModel for the DemoPlayerView
    /// </summary>
    [Export(typeof(DemoPlayerViewModel))]
    public class DemoPlayerViewModel : PropertyChangedBase, IHaveDisplayName
    {
        private readonly IWindowManager _windowManager;
        private string _actionText;
        private double _archivingProgress;
        private bool _canCancelArchive = true;
        private bool _canCancelProcess = true;
        private string _cancelText = "Cancel";
        private ObservableCollection<DemoInfoViewModel> _demos = new ObservableCollection<DemoInfoViewModel>();
        private volatile bool _hasReceivedArchiveCancelation;
        private volatile bool _hasReceivedProcessCancelation;
        private bool _isArchivingDemos;
        private bool _isProcessingDemos;
        private double _processingProgress;
        private string _qlDemoDirectoryPath;
        private DemoInfoViewModel _selectedDemo;
        private bool _showBusyIndicator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoPlayerViewModel" /> class.
        /// </summary>
        [ImportingConstructor]
        public DemoPlayerViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
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
                NotifyOfPropertyChange(() => SelectedDemo);
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
                //await CopyDemosToDemoDirectoryAsync(QuakeLiveTypes.Production, demofilepaths);

                // send to list splitter/QLDemoDumper
                var dumper = new DemoDumper(this);
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
                    //await CopyDemosToDemoDirectoryAsync(QuakeLiveTypes.Production, GetDemosFromSpecifiedDirectory(openfolderdialog.SelectedPath));

                    // send to list splitter/QLDemoDumper
                    var dumper = new DemoDumper(this);
                    var demos = dumper.CollectDemos(GetDemosFromSpecifiedDirectory(openfolderdialog.SelectedPath));
                    dumper.ProcessDemos(demos);
                }
                else
                {
                    MessageBox.Show(
                        string.Format("{0} and its sub-directories do not contain any Quake Live demo files!",
                            openfolderdialog.SelectedPath), "No demo files found!", MessageBoxButton.OK,
                        MessageBoxImage.Error);
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
            List<string> files = GetDemosFromSpecifiedDirectory(path);

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
        private async Task CopyDemosToDemoDirectoryAsync(QuakeLiveTypes qltype, List<string> demofilestocopy)
        {
            foreach (string file in demofilestocopy)
            {
                string filename = file.Substring(file.LastIndexOf('\\'));
                using (FileStream sourceStream = File.Open(file, FileMode.Open))
                {
                    if (File.Exists(QLDirectoryUtils.GetQuakeLiveDemoDirectory(qltype) + filename))
                    {
                        MessageBoxResult shouldOverwrite =
                            MessageBox.Show(
                                string.Format(
                                    "{0} already exists in your QL demo folder. Would you like to overwrite this file in your QL demo folder?",
                                    filename.Replace("\\", "")),
                                "File already exists!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (shouldOverwrite != MessageBoxResult.Yes) continue;
                        using (
                            FileStream destinationStream =
                                File.Create(QLDirectoryUtils.GetQuakeLiveDemoDirectory(qltype) + filename))
                        {
                            Debug.WriteLine("Overwriting existing file in demo directory: " + filename);
                            await sourceStream.CopyToAsync(destinationStream);
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
                            file.ToLowerInvariant().EndsWith("dm_73", StringComparison.OrdinalIgnoreCase) ||
                            file.ToLowerInvariant().EndsWith("dm_90", StringComparison.OrdinalIgnoreCase)).ToList();

            return demosfrompath;
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
            var dumper = new DemoDumper(this);
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
    }
}