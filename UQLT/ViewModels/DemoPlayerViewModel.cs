using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using UQLT.Helpers;
using UQLT.Interfaces;

namespace UQLT.ViewModels
{
    /// <summary>
    /// ViewModel for the DemoPlayerView
    /// </summary>
    [Export(typeof(DemoPlayerViewModel))]
    public class DemoPlayerViewModel : PropertyChangedBase, IHaveDisplayName
    {

        private string _qlDemoDirectoryPath;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DemoPlayerViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public DemoPlayerViewModel()
        {
            DisplayName = "UQLT v0.1 - Demo Player";
            //TODO: Focus
            QlDemoDirectoryPath = QLDirectoryUtils.GetQuakeLiveDemoDirectory(QuakeLiveTypes.Production);
        }

        /// <summary>
        /// Gets or Sets the display name for this window.
        /// </summary>
        public string DisplayName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the QL demo directory path.
        /// </summary>
        /// <value>
        /// The QL demo directory path.
        /// </value>
        public string QlDemoDirectoryPath
        {
            get
            {
                return _qlDemoDirectoryPath;
                
            }
            set
            {
                _qlDemoDirectoryPath = value;
                NotifyOfPropertyChange(() => QlDemoDirectoryPath);
            }
        }

        /// <summary>
        /// Handles the addition of a demo directory.
        /// </summary>
        public void AddADemoDirectory()
        {
            using (var openfolderdialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                openfolderdialog.Description = "Select a directory containing .dm_73 and/or .dm_90 QL demo files.";
                openfolderdialog.ShowNewFolderButton = false;
                openfolderdialog.RootFolder = Environment.SpecialFolder.MyComputer;
                if (openfolderdialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (AdditionalDirContainsDemoFiles(openfolderdialog.SelectedPath))
                    {
                        // send to QLDemoDumper
                    }
                    else
                    {
                        MessageBox.Show(
                            string.Format("{0} and its sub-directories do not contain any Quake Live demo files!",
                                openfolderdialog.SelectedPath), "No demo files found!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the addition of a single demo file.
        /// </summary>
        public void AddASingleDemo()
        {

            using (var openfiledialog = new System.Windows.Forms.OpenFileDialog())
            {
                openfiledialog.CheckFileExists = true;
                openfiledialog.CheckPathExists = true;
                openfiledialog.Multiselect = true;
                openfiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openfiledialog.Filter = "Quake Live demo (*.dm_73)|*.dm_73|Quake Live demo (*.dm_90)|*.dm_90";

            }   
        }

        /// <summary>
        /// Checks whether the additional demo directory (and sub-directories) that the user has specified contains Quake Live demo files.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if the directory contains at least 1 file with the .dm_73 and/or .dm_90 extension.</returns>
        private bool AdditionalDirContainsDemoFiles(string path)
        {
                var files =
                    Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                        .Where(
                            file =>
                                file.ToLowerInvariant().EndsWith("dm_73", StringComparison.OrdinalIgnoreCase) ||
                                file.EndsWith("dm_90", StringComparison.OrdinalIgnoreCase));

            return files.Any();
        }


    }
}
