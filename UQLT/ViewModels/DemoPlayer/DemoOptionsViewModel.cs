using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using UQLT.Core;
using UQLT.Core.Modules.DemoPlayer;
using UQLT.Helpers;
using UQLT.Interfaces;
using UQLT.Models.Configuration;

namespace UQLT.ViewModels.DemoPlayer
{
    /// <summary>
    /// View model for the demo options
    /// </summary>
    [Export(typeof(DemoOptionsViewModel))]
    public class DemoOptionsViewModel : PropertyChangedBase, IUqltConfiguration, IHaveDisplayName, IViewAware
    {
        private Window _dialogWindow;
        private string _qlCustomDemoCfgPath;
        private bool _useQlCustomDemoCfg;
        private bool _useWolfcamQlCustomDemoCfg;
        private bool _useWolfcamQlForOldDemos;
        private bool _useWolfWhispererForOldDemos;
        private string _wolfcamQlCustomDemoCfgPath;
        private string _wolfCamQlExePath;
        private string _wolfWhispererExePath;

        [ImportingConstructor]
        public DemoOptionsViewModel()
        {
            DisplayName = "Demo options";
            LoadConfig();
        }

        /// <summary>
        /// Raised when a view is attached.
        /// </summary>
        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

        /// <summary>
        /// Gets or Sets the name of this window.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the file path to the custom demo .cfg file for the Quake Live client.
        /// </summary>
        /// <value>
        /// The file path to the custom demo .cfg file for the Quake Live client.
        /// </value>
        public string QlCustomDemoCfgPath
        {
            get
            {
                return _qlCustomDemoCfgPath;
            }
            set
            {
                _qlCustomDemoCfgPath = value;
                NotifyOfPropertyChange(() => QlCustomDemoCfgPath);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Quake Live client should use a custom demo .cfg file when playing new .dm_90 demos.
        /// </summary>
        /// <value>
        /// <c>true</c> if the Quake Live client should use a custom demo .cfg file when playing new .dm_90 demos.; otherwise, <c>false</c>.
        /// </value>
        public bool UseQlCustomDemoCfg
        {
            get
            {
                return _useQlCustomDemoCfg;
            }
            set
            {
                _useQlCustomDemoCfg = value;
                NotifyOfPropertyChange(() => UseQlCustomDemoCfg);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether WolfcamQL should use a custom demo .cfg file when playing old .dm_73 demos.
        /// </summary>
        /// <value>
        /// <c>true</c> if WolfcamQL should use a custom demo .cfg file when playing old .dm_73 demos.; otherwise, <c>false</c>.
        /// </value>
        public bool UseWolfcamQlCustomDemoCfg
        {
            get
            {
                return _useWolfcamQlCustomDemoCfg;
            }
            set
            {
                _useWolfcamQlCustomDemoCfg = value;
                NotifyOfPropertyChange(() => UseWolfcamQlCustomDemoCfg);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether WolfcamQL should be used to play old .dm_73 demos.
        /// </summary>
        /// <value>
        /// <c>true</c> if WolfcamQL should be used to play old .dm_73 demos otherwise, <c>false</c>.
        /// </value>
        /// <remarks>This setting and <see cref="UseWolfWhispererForOldDemos"/> are mutually exclusive.</remarks>
        public bool UseWolfcamQlForOldDemos
        {
            get
            {
                return _useWolfcamQlForOldDemos;
            }
            set
            {
                _useWolfcamQlForOldDemos = value;
                if (value)
                {
                    UseWolfWhispererForOldDemos = false;
                }
                NotifyOfPropertyChange(() => UseWolfcamQlForOldDemos);
                NotifyOfPropertyChange(() => UseWolfWhispererForOldDemos);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Wolf Whisperer should be used to play old .dm_73 demos.
        /// </summary>
        /// <value>
        /// <c>true</c> if Wolf Whisperer should be used to play old .dm_73 demos otherwise, <c>false</c>.
        /// </value>
        /// <remarks>This setting and <see cref="UseWolfcamQlForOldDemos"/> are mutually exclusive.</remarks>
        public bool UseWolfWhispererForOldDemos
        {
            get
            {
                return _useWolfWhispererForOldDemos;
            }
            set
            {
                _useWolfWhispererForOldDemos = value;
                if (value)
                {
                    UseWolfcamQlForOldDemos = false;
                }
                NotifyOfPropertyChange(() => UseWolfWhispererForOldDemos);
                NotifyOfPropertyChange(() => UseWolfcamQlForOldDemos);
            }
        }

        /// <summary>
        /// Gets or sets the file path to the custom demo .cfg file for WolfcamQL.
        /// </summary>
        /// <value>
        /// The file path to the custom demo .cfg file for WolfcamQL.
        /// </value>
        public string WolfcamQlCustomDemoCfgPath
        {
            get
            {
                return _wolfcamQlCustomDemoCfgPath;
            }
            set
            {
                _wolfcamQlCustomDemoCfgPath = value;
                NotifyOfPropertyChange(() => WolfcamQlCustomDemoCfgPath);
            }
        }

        /// <summary>
        /// Gets or sets the file path to the WolfcamQL executable.
        /// </summary>
        /// <value>
        /// The WolfcamQL executable file path.
        /// </value>
        public string WolfcamQlExePath
        {
            get
            {
                return _wolfCamQlExePath;
            }
            set
            {
                _wolfCamQlExePath = value;
                NotifyOfPropertyChange(() => WolfcamQlExePath);
            }
        }


        /// <summary>
        /// Gets or sets file path to the Wolf Whisperer executable.
        /// </summary>
        /// <value>
        /// The Wolf Whisperer executable  file path.
        /// </value>
        public string WolfWhispererExePath
        {
            get
            {
                return _wolfWhispererExePath;
            }
            set
            {
                _wolfWhispererExePath = value;
                NotifyOfPropertyChange(() => WolfWhispererExePath);
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
        /// Checks whether the configuration already exists
        /// </summary>
        /// <returns><c>true</c> if configuration exists, otherwise <c>false</c></returns>
        public bool ConfigExists()
        {
            return File.Exists(UQltFileUtils.GetConfigurationPath());
        }

        /// <summary>
        /// Downloads a third party demo player (WolfcamQL or Wolf Whisperer)
        /// </summary>
        /// <param name="url">The URL to download from.</param>
        /// <remarks>This is called from the view itself.</remarks>
        public void DownloadThirdPartyPlayer(string url)
        {
            Process.Start(url);
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
        /// Loads the configuration.
        /// </summary>
        public void LoadConfig()
        {
            if (!ConfigExists())
            {
                LoadDefaultConfig();
            }
            var cfghandler = new ConfigurationHandler();
            cfghandler.ReadConfig();

            UseWolfcamQlForOldDemos = cfghandler.DemoOptUseWolfcamQl;
            UseWolfWhispererForOldDemos = cfghandler.DemoOptUseWolfWhisperer;
            UseQlCustomDemoCfg = cfghandler.DemoOptUseCustomQlCfg;
            UseWolfcamQlCustomDemoCfg = cfghandler.DemoOptUseCustomWolfcamQlCfg;
            QlCustomDemoCfgPath = cfghandler.DemoOptQlCfgPath;
            WolfcamQlExePath = cfghandler.DemoOptWolfcamQlExePath;
            WolfcamQlCustomDemoCfgPath = cfghandler.DemoOptWolfcamQlCfgPath;
            WolfWhispererExePath = cfghandler.DemoOptWolfWhispererExePath;
        }

        /// <summary>
        /// Loads the default configuration.
        /// </summary>
        public void LoadDefaultConfig()
        {
            var cfghandler = new ConfigurationHandler();
            cfghandler.RestoreDefaultConfig();
        }

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        public void SaveConfig()
        {
            var cfghandler = new ConfigurationHandler();
            cfghandler.ReadConfig();

            cfghandler.DemoOptUseWolfcamQl = UseWolfcamQlForOldDemos;
            cfghandler.DemoOptUseWolfWhisperer = UseWolfWhispererForOldDemos;
            cfghandler.DemoOptWolfcamQlExePath = WolfcamQlExePath;
            cfghandler.DemoOptWolfWhispererExePath = WolfWhispererExePath;
            cfghandler.DemoOptUseCustomQlCfg = UseQlCustomDemoCfg;
            cfghandler.DemoOptUseCustomWolfcamQlCfg = UseWolfcamQlCustomDemoCfg;
            cfghandler.DemoOptQlCfgPath = QlCustomDemoCfgPath;
            cfghandler.DemoOptWolfcamQlCfgPath = WolfcamQlCustomDemoCfgPath;

            cfghandler.WriteConfig();
        }

        /// <summary>
        /// Sets the Quake Live custom demo config file path.
        /// </summary>
        /// <remarks>This is called from the view itself</remarks>
        public async Task SetQlCustomDemoCfgPath()
        {
            using (var openfiledialog = new OpenFileDialog())
            {
                openfiledialog.CheckFileExists = true;
                openfiledialog.CheckPathExists = true;
                openfiledialog.Multiselect = false;
                //TODO: Avoid Production hard-code. Detect if UQLT is being launched from Focus context and automatically set.
                openfiledialog.InitialDirectory = QLDirectoryUtils.GetQuakeLiveBasePath(QuakeLiveTypes.Production);
                openfiledialog.Filter = "Config file (*.cfg)|*.cfg";

                if (openfiledialog.ShowDialog() != DialogResult.OK)
                {
                    UseQlCustomDemoCfg = false;
                    return;
                }

                QlCustomDemoCfgPath = openfiledialog.FileName;
                await CopyCustomCfgToDirAsync(DemoPlayerTypes.QuakeLive, openfiledialog.FileName);
            }
        }

        /// <summary>
        /// Sets the wolfcam executable path.
        /// </summary>
        /// <remarks>This is called from the view itself</remarks>
        public void SetWolfcamExePath()
        {
            using (var openfiledialog = new OpenFileDialog())
            {
                openfiledialog.CheckFileExists = true;
                openfiledialog.CheckPathExists = true;
                openfiledialog.Multiselect = false;
                openfiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openfiledialog.Filter = "wolfcamql.exe|wolfcamql.exe";

                if (openfiledialog.ShowDialog() != DialogResult.OK)
                {
                    UseWolfcamQlForOldDemos = false;
                    return;
                }

                WolfcamQlExePath = openfiledialog.FileName;
            }
        }

        /// <summary>
        /// Sets the WolfcamQL custom demo config file path.
        /// </summary>
        /// <remarks>This is called from the view itself</remarks> 
        public async Task SetWolfcamQlCustomDemoCfgPath()
        {
            using (var openfiledialog = new OpenFileDialog())
            {
                openfiledialog.CheckFileExists = true;
                openfiledialog.CheckPathExists = true;
                openfiledialog.Multiselect = false;
                //TODO: Avoid Production hard-code. Detect if UQLT is being launched from Focus context and automatically set.
                openfiledialog.InitialDirectory = QLDirectoryUtils.GetQuakeLiveBasePath(QuakeLiveTypes.Production);
                openfiledialog.Filter = "Config file (*.cfg)|*.cfg";

                if (openfiledialog.ShowDialog() != DialogResult.OK)
                {
                    UseWolfcamQlCustomDemoCfg = false;
                    return;
                }

                WolfcamQlCustomDemoCfgPath = openfiledialog.FileName;
                await CopyCustomCfgToDirAsync(DemoPlayerTypes.WolfcamQl, openfiledialog.FileName);
            }
        }

        /// <summary>
        /// Sets the Wolf Whisperer executable path.
        /// </summary>
        /// <remarks>This is called from the view itself</remarks>
        public void SetWolfWhispererExePath()
        {
            using (var openfiledialog = new OpenFileDialog())
            {
                openfiledialog.CheckFileExists = true;
                openfiledialog.CheckPathExists = true;
                openfiledialog.Multiselect = false;
                openfiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openfiledialog.Filter = "WolfWhisperer.exe|WolfWhisperer.exe";

                if (openfiledialog.ShowDialog() != DialogResult.OK)
                {
                    UseWolfWhispererForOldDemos = false;
                    return;
                }

                WolfWhispererExePath = openfiledialog.FileName;
            }
        }

        /// <summary>
        /// Unsets the Quake Live custom demo config file path.
        /// </summary>
        /// <remarks>This is called from the view itself</remarks>
        public void UnsetQlCustomDemoCfgPath()
        {
            QlCustomDemoCfgPath = string.Empty;
        }

        /// <summary>
        /// Unsets the wolfcam executable path.
        /// </summary>
        /// <remarks>This is called from the view itself</remarks>
        public void UnsetWolfcamExePath()
        {
            WolfcamQlExePath = string.Empty;
        }

        /// <summary>
        /// Unsets the WolfcamQL custom demo config file path.
        /// </summary>
        /// <remarks>This is called from the view itself</remarks>
        public void UnsetWolfcamQlCustomDemoCfgPath()
        {
            WolfcamQlCustomDemoCfgPath = string.Empty;
        }

        /// <summary>
        /// Unsets the wolf whisperer executable path.
        /// </summary>
        /// <remarks>This is called from the view itself</remarks>
        public void UnsetWolfWhispererExePath()
        {
            WolfWhispererExePath = string.Empty;
        }

        /// <summary>
        /// Copies the custom demo config to the appropriate directory based on the demo player type.
        /// </summary>
        /// <param name="playerType">Type of demo player.</param>
        /// <param name="originalCfgLocation">The full file path to the original demo configuration.</param>
        private async Task CopyCustomCfgToDirAsync(DemoPlayerTypes playerType, string originalCfgLocation)
        {
            var filename = Path.GetFileName(originalCfgLocation);
            var directory = string.Empty;
            
            switch (playerType)
            {
                case DemoPlayerTypes.QuakeLive:
                    //TODO: Avoid Production hard-code. Detect if UQLT is being launched from Focus context and automatically set.
                    directory = QLDirectoryUtils.GetQuakeLiveHomeBaseQ3Directory(QuakeLiveTypes.Production);
                    break;

                case DemoPlayerTypes.WolfcamQl:
                    directory = Path.Combine(Path.GetDirectoryName(WolfcamQlExePath), "wolfcam-ql");
                    break;

            }
            // Don't perform file copy operation if the source and destination are the same (avoid 'in use by another process' exception)
            if (Path.GetFullPath(originalCfgLocation).Equals(Path.Combine(directory, filename)))
            {
                Debug.WriteLine("SRC {0} is same as DEST {1}...skipping file copy process", Path.GetFullPath(originalCfgLocation), Path.Combine(directory, filename));
                return;
            }
            
            using (FileStream sourceStream = File.Open(originalCfgLocation, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                if (directory != null && (filename != null && File.Exists(Path.Combine(directory, filename))))
                {
                    var msgBoxService = new MsgBoxService();
                    if (msgBoxService.AskConfirmationMessage(string.Format(
                        "{0} already exists in your {1} directory. Would you like to overwrite this file in {1}?",
                        filename, directory),
                        "File already exists!"))
                    {
                        using (
                            FileStream destinationStream =
                                File.Create(Path.Combine(directory, filename)))
                        {
                            Debug.WriteLine("Overwriting existing file {0} in {1}", filename, directory);
                            await sourceStream.CopyToAsync(destinationStream);
                            // Update the .cfg location to new location
                            UpdateNewCfgFilePath(playerType, Path.Combine(directory, filename));
                        }
                    }
                }
                else
                {
                    using (
                        FileStream destinationStream =
                            File.Create(Path.Combine(directory, filename)))
                    {
                        Debug.WriteLine("{0} did not exist in {1}. Copying to {1}", filename, directory);
                        await sourceStream.CopyToAsync(destinationStream);
                        // Update the .cfg location to new location
                        UpdateNewCfgFilePath(playerType, Path.Combine(directory, filename));
                    }
                }
            }
        }

        /// <summary>
        /// Updates the location to the new custom demo .cfg file path after it has been copied to the appropriate directory.
        /// </summary>
        /// <param name="playerType">Type of demo player.</param>
        /// <param name="newLocation">The full filepath to the demo config at its new location.</param>
        private void UpdateNewCfgFilePath(DemoPlayerTypes playerType, string newLocation)
        {
            if (playerType == DemoPlayerTypes.QuakeLive)
            {
            QlCustomDemoCfgPath = newLocation;
            }
            if (playerType == DemoPlayerTypes.WolfcamQl)
            {
                WolfcamQlCustomDemoCfgPath = newLocation;
            }
        }
    }
}