using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        private string _qlDemoDirectoryPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoPlayerViewModel" /> class.
        /// </summary>
        [ImportingConstructor]
        public DemoPlayerViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
            DisplayName = "UQLT v0.1 - Demo Player";
            //TODO: Avoid Production hard-code. Detect if UQLT is being launched from Focus context and automatically set.
            QlDemoDirectoryPath = QLDirectoryUtils.GetQuakeLiveDemoDirectory(QuakeLiveTypes.Production);
        }

        /// <summary>
        /// Gets or Sets the display name for this window.
        /// </summary>
        public string DisplayName { get; set; }

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
                openfiledialog.Filter = "Quake Live demo (*.dm_73)|*.dm_73|Quake Live demo (*.dm_90)|*.dm_90";

                if (openfiledialog.ShowDialog() != DialogResult.OK) return;

                var demofilepaths = new List<string>();
                demofilepaths.AddRange(openfiledialog.FileNames);

                // TODO: Fix the hard-coding of Production here. This method and the app in general need to detect if UQLT is being launched from Focus context.
                await CopyDemosToDemoDirectoryAsync(QuakeLiveTypes.Production, demofilepaths);

                // send to list splitter/QLDemoDumper
            }
        }

        /// <summary>
        /// Handles the addition of a demo directory.
        /// </summary>
        public async Task AddDemoDirectory()
        {
            using (var openfolderdialog = new FolderBrowserDialog())
            {
                openfolderdialog.Description = "Select a directory containing .dm_73 and/or .dm_90 QL demo files.";
                openfolderdialog.ShowNewFolderButton = false;
                openfolderdialog.RootFolder = Environment.SpecialFolder.MyComputer;

                if (openfolderdialog.ShowDialog() != DialogResult.OK) return;

                if (AdditionalDirContainsDemoFiles(openfolderdialog.SelectedPath))
                {
                    Debug.WriteLine("Selected directory " + openfolderdialog.SelectedPath + " DOES contain demo files!");

                    // TODO: Fix the hard-coding of Production here. This method and the app in general need to detect if UQLT is being launched from Focus context.
                    //await CopyDemosToDemoDirectoryAsync(QuakeLiveTypes.Production, GetDemosFromSpecifiedDirectory(openfolderdialog.SelectedPath));

                    // send to list splitter/QLDemoDumper
                    var dumper = new DemoDumper();
                    List<List<string>> x =
                        dumper.GetMaxDemosPerDump(GetDemosFromSpecifiedDirectory(openfolderdialog.SelectedPath));
                    for (int i = 0; i < x.Count; ++i)
                    {
                        Debug.WriteLine("Dump process #: " + i);
                    }
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
    }
}