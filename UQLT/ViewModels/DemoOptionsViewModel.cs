using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using UQLT.Core;
using UQLT.Interfaces;
using UQLT.Models.Configuration;

namespace UQLT.ViewModels
{
    /// <summary>
    /// View model for the demo options
    /// </summary>
    [Export(typeof(DemoOptionsViewModel))]
    public class DemoOptionsViewModel : PropertyChangedBase, IUqltConfiguration, IHaveDisplayName, IViewAware
    {
        private Window _dialogWindow;
        private bool _useWolfcamQlForOldDemos;
        private bool _useWolfWhispererForOldDemos;
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
            WolfcamQlExePath = cfghandler.DemoOptWolfcamQlExePath;
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

            cfghandler.WriteConfig();
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
        /// Unsets the wolfcam executable path.
        /// </summary>
        /// <remarks>This is called from the view itself</remarks>
        public void UnsetWolfcamExePath()
        {
            WolfcamQlExePath = string.Empty;
        }

        /// <summary>
        /// Unsets the wolf whisperer executable path.
        /// </summary>
        /// <remarks>This is called from the view itself</remarks>
        public void UnsetWolfWhispererExePath()
        {
            WolfWhispererExePath = string.Empty;
        }
    }
}