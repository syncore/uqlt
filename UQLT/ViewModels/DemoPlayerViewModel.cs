using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using UQLT.Interfaces;

namespace UQLT.ViewModels
{
    /// <summary>
    /// ViewModel for the DemoPlayerView
    /// </summary>
    [Export(typeof(DemoPlayerViewModel))]
    public class DemoPlayerViewModel : PropertyChangedBase, IHaveDisplayName, IOSVersion
    {

        private string _qlDemoDirectoryPath;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DemoPlayerViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public DemoPlayerViewModel()
        {
            DisplayName = "UQLT v0.1 - Demo Player";
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
        /// Handles the selection of the QL demo directory path.
        /// </summary>
        /// <returns>
        /// The new demo directory path as a string.
        /// </returns>
        //TODO: On first login, UQLT will prompt user for Quake Live directory, so we will likely read a default value for this from that prompt
        public void SetQlDemoDirectory()
        {
            using (var openfolderdialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                openfolderdialog.Description = "Select your Quake Live demo folder (id Software\\quakelive\\home\baseq3)";
                openfolderdialog.ShowNewFolderButton = false;
                if (IsWindowsXp())
                {
                    openfolderdialog.RootFolder = Environment.SpecialFolder.LocalApplicationData;
                }
                else if (IsVistaOrNewer())
                {
                    openfolderdialog.SelectedPath = GetLocalAppDataLowPath();
                    // Automatically append "id Software\quakelive\home\baseq3" for now?
                }
                else
                {
                    openfolderdialog.RootFolder = Environment.SpecialFolder.LocalApplicationData;
                    
                }
                if (openfolderdialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Have some detection here to make sure it is indeed a valid QL directory...
                    QlDemoDirectoryPath = openfolderdialog.SelectedPath;
                }
            }
        }

        private readonly Guid FOLDERID_LocalAppDataLow = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");

        /// <summary>
        /// Gets the known folder path using a specified FOLDERID Guid.
        /// </summary>
        /// <param name="knownFolderId">The known folder identifier.</param>
        /// <returns> The known folder path.
        /// </returns>
        /// <remarks>For GUIDs see: http://msdn.microsoft.com/en-us/library/dd378457.aspx
        /// </remarks>
        private string GetKnownFolderPath(Guid knownFolderId)
        {
            IntPtr pszPath = IntPtr.Zero;
            try
            {
                int hr = SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
                if (hr >= 0)
                    return Marshal.PtrToStringAuto(pszPath);
                throw Marshal.GetExceptionForHR(hr);
            }
            finally
            {
                if (pszPath != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pszPath);
            }
        }

        [DllImport("shell32.dll")]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

        /// <summary>
        /// Gets the DRIVE:\Users\User\AppData\LocalLow directory at the OS level.
        /// </summary>
        /// <returns>The LocalLow directory path as a string.</returns>
        /// <remarks> See: http://stackoverflow.com/a/4495081
        /// </remarks>
        private string GetLocalAppDataLowPath()
        {
            return GetKnownFolderPath(FOLDERID_LocalAppDataLow);
        }

        /// <summary>
        /// Determines whether the host is running Windows XP.
        /// </summary>
        /// <returns></returns>
        public bool IsWindowsXp()
        {
            var osVersion = Environment.OSVersion;
            return osVersion.Version.Major == 5 && osVersion.Version.Minor > 0;
        }

        /// <summary>
        /// Determines whether the host is running Windows Vista or newer.
        /// </summary>
        /// <returns></returns>
        public bool IsVistaOrNewer()
        {
            var osVersion = Environment.OSVersion;
            return osVersion.Version.Major >= 6;
        }
    }
}
