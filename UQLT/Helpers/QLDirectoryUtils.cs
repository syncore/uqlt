using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace UQLT.Helpers
{
    /// <summary>
    /// Helper class for detection and creation of various Quake Live and Focus directories.
    /// </summary>
    public static class QLDirectoryUtils
    {
        private static readonly Guid FOLDERID_LocalAppDataLow = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");

        /// <summary>
        /// Creates the home (and) baseq3 subdirectory as well that contains user config files (qzconfig.cfg, repconfig.cfg) and essential
        /// game .dll files (uix86.dll, cgamex86.dll) if it does not already exist for a given Quake Live type.
        /// </summary>
        /// <param name="qltype">The Quake Live type.</param>
        /// <remarks>This is the baseq3 directory inside of the home directory (id Software\qltype\quakelive\home\baseq3)</remarks>
        public static void CreateBaseQ3HomeDirectory(QuakeLiveTypes qltype)
        {
            switch (qltype)
            {
                case QuakeLiveTypes.Production:
                    if (IsWindowsXp())
                    {
                        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "id Software\\quakelive\\home\\baseq3");
                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine("[WINDOWS XP, PROD]: Created directory: " + path);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[WINDOWS XP, PROD]: Unable to create directory: " + path + " exception: " + e);
                            }
                        }
                    }
                    if (IsVistaOrNewer())
                    {
                        var path = Path.Combine(GetLocalAppDataLowPath(),
                            "id Software\\quakelive\\home\\baseq3");
                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine("[WINDOWS VISTA OR NEWER, PROD]: Created directory: " + path);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[WINDOWS VISTA OR NEWER, PROD]: Unable to create directory: " + path + " exception: " + e);
                            }
                        }
                    }
                    return;

                case QuakeLiveTypes.Focus:
                    if (IsWindowsXp())
                    {
                        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "id Software\\focus\\home\\baseq3");
                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine("[WINDOWS XP, FOCUS]: Created directory: " + path);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[WINDOWS XP, FOCUS]: Unable to create directory: " + path + " exception: " + e);
                            }
                        }
                    }
                    if (IsVistaOrNewer())
                    {
                        var path = Path.Combine(GetLocalAppDataLowPath(), "id Software\\focus\\home\\baseq3");
                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine("[WINDOWS VISTA OR NEWER, FOCUS]: Created directory: " + path);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[WINDOWS VISTA OR NEWER, FOCUS]: Unable to create directory: " + path + " exception: " + e);
                            }
                        }
                    }
                    return;
            }
        }

        /// <summary>
        /// Creates the baseq3 (map) directory if it does not already exist for a given Quake Live type.
        /// </summary>
        /// <param name="qltype">The Quake Live type.</param>
        /// <remarks>This is the baseq3 map directory that contains the map .pk3 files (id Software\qltype\baseq3)</remarks>
        public static void CreateBaseQ3MapDirectory(QuakeLiveTypes qltype)
        {
            switch (qltype)
            {
                case QuakeLiveTypes.Production:
                    if (IsWindowsXp())
                    {
                        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "id Software\\quakelive\\baseq3");
                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine("[WINDOWS XP, PROD]: Created directory: " + path);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[WINDOWS XP, PROD]: Unable to create directory: " + path + " exception: " + e);
                            }
                        }
                    }
                    if (IsVistaOrNewer())
                    {
                        var path = Path.Combine(GetLocalAppDataLowPath(), "id Software\\quakelive\\baseq3");
                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine("[WINDOWS VISTA OR NEWER, PROD]: Created directory: " + path);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[WINDOWS VISTA OR NEWER, PROD]: Unable to create directory: " + path + " exception: " + e);
                            }
                        }
                    }
                    return;

                case QuakeLiveTypes.Focus:
                    if (IsWindowsXp())
                    {
                        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "id Software\\focus\\baseq3");
                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine("[WINDOWS XP, FOCUS]: Created directory: " + path);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[WINDOWS XP, FOCUS]: Unable to create directory: " + path + " exception: " + e);
                            }
                        }
                    }
                    if (IsVistaOrNewer())
                    {
                        var path = Path.Combine(GetLocalAppDataLowPath(), "id Software\\focus\\baseq3");
                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine("[WINDOWS VISTA OR NEWER, FOCUS]: Created directory: " + path);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[WINDOWS VISTA OR NEWER, FOCUS]: Unable to create directory: " + path + " exception: " + e);
                            }
                        }
                    }
                    return;
            }
        }

        /// <summary>
        /// Creates the home (and) baseq3 (and) demo subdirectories as well that contains user demo files if it does not already exist for a given
        ///  Quake Live type.
        /// </summary>
        /// <param name="qltype">The Quake Live type.</param>
        public static void CreateDemoDirectory(QuakeLiveTypes qltype)
        {
            switch (qltype)
            {
                case QuakeLiveTypes.Production:
                    if (IsWindowsXp())
                    {
                        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "id Software\\quakelive\\home\\baseq3\\demos");
                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine("[WINDOWS XP, PROD]: Created directory: " + path);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[WINDOWS XP, PROD]: Unable to create directory: " + path + " exception: " + e);
                            }
                        }
                    }
                    if (IsVistaOrNewer())
                    {
                        var path = Path.Combine(GetLocalAppDataLowPath(),
                            "id Software\\quakelive\\home\\baseq3\\demos");
                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine("[WINDOWS VISTA OR NEWER, PROD]: Created directory: " + path);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[WINDOWS VISTA OR NEWER, PROD]: Unable to create directory: " + path + " exception: " + e);
                            }
                        }
                    }
                    return;

                case QuakeLiveTypes.Focus:
                    if (IsWindowsXp())
                    {
                        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "id Software\\focus\\home\\baseq3\\demos");
                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine("[WINDOWS XP, FOCUS]: Created directory: " + path);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[WINDOWS XP, FOCUS]: Unable to create directory: " + path + " exception: " + e);
                            }
                        }
                    }
                    if (IsVistaOrNewer())
                    {
                        var path = Path.Combine(GetLocalAppDataLowPath(), "id Software\\focus\\home\\baseq3\\demos");
                        if (!Directory.Exists(path))
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                                Debug.WriteLine("[WINDOWS VISTA OR NEWER, FOCUS]: Created directory: " + path);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[WINDOWS VISTA OR NEWER, FOCUS]: Unable to create directory: " + path + " exception: " + e);
                            }
                        }
                    }
                    return;
            }
        }

        /// <summary>
        /// Determines whether the Quake Live or Quake Live Focus basepath exists.
        /// </summary>
        /// <param name="qltype">The qltype.</param>
        /// <returns><c>True</c> if WindowsXP and: appdata%\id Software\quakelive or appdata%\id Software\focus exists, <c>true</c>
        /// if Windows Vista or newer and: %AppData%\..\LocalLow\id Software\quakelive\ or %AppData%\..\LocalLow\id Software\focus\ exists. Otherwise <c>false</c>.
        /// </returns>
        public static bool DoesQuakeLiveBasePathExist(QuakeLiveTypes qltype)
        {
            if (qltype == QuakeLiveTypes.Production)
            {
                if (IsWindowsXp())
                {
                    if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "id Software\\quakelive")))
                    {
                        return true;
                    }
                }
                if (IsVistaOrNewer())
                {
                    if (Directory.Exists(Path.Combine(GetLocalAppDataLowPath(), "id Software\\quakelive")))
                    {
                        return true;
                    }
                }
            }
            if (qltype == QuakeLiveTypes.Focus)
            {
                if (IsWindowsXp())
                {
                    if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "id Software\\focus")))
                    {
                        return true;
                    }
                }
                if (IsVistaOrNewer())
                {
                    if (Directory.Exists(Path.Combine(GetLocalAppDataLowPath(), "id Software\\focus")))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the Quake Live or Quake Live Focus basepath exists.
        /// </summary>
        /// <param name="qltype">The qltype.</param>
        /// <returns><c>True</c> if WindowsXP and: appdata%\id Software\quakelive\home or appdata%\id Software\focus\home exists, <c>true</c>
        /// if Windows Vista or newer and: %AppData%\..\LocalLow\id Software\quakelive\home\ or %AppData%\..\LocalLow\id Software\focus\home\ exists. Otherwise <c>false</c>.
        /// </returns>
        public static bool DoesQuakeLiveHomePathExist(QuakeLiveTypes qltype)
        {
            if (qltype == QuakeLiveTypes.Production)
            {
                if (IsWindowsXp())
                {
                    if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "id Software\\quakelive\\home")))
                    {
                        return true;
                    }
                }
                if (IsVistaOrNewer())
                {
                    if (Directory.Exists(Path.Combine(GetLocalAppDataLowPath(), "id Software\\quakelive\\home")))
                    {
                        return true;
                    }
                }
            }
            if (qltype == QuakeLiveTypes.Focus)
            {
                if (IsWindowsXp())
                {
                    if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "id Software\\focus\\home")))
                    {
                        return true;
                    }
                }
                if (IsVistaOrNewer())
                {
                    if (Directory.Exists(Path.Combine(GetLocalAppDataLowPath(), "id Software\\focus\\home")))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string GetQuakeLiveDemoDirectory(QuakeLiveTypes qltype)
        {
            var path = string.Empty;
            switch (qltype)
            {
                case QuakeLiveTypes.Production:
                    if (IsWindowsXp())
                    {
                        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "id Software\\quakelive\\home\\baseq3\\demos");
                    }
                    if (IsVistaOrNewer())
                    {
                        path = Path.Combine(GetLocalAppDataLowPath(),
                            "id Software\\quakelive\\home\\baseq3\\demos");
                    }
                    Debug.WriteLine("[PROD]: Returned Quake Live demo directory path: " + path);
                    break;

                case QuakeLiveTypes.Focus:
                    if (IsWindowsXp())
                    {
                        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "id Software\\focus\\home\\baseq3\\demos");
                    }
                    if (IsVistaOrNewer())
                    {
                        path = Path.Combine(GetLocalAppDataLowPath(),
                            "id Software\\focus\\home\\baseq3\\demos");
                    }
                    Debug.WriteLine("[FOCUS]: Returned Quake Live demo directory path: " + path);
                    break;
            }
            return path;
        }

        /// <summary>
        /// Determines whether the Quake Live (focus) game executable is installed.
        /// </summary>
        /// <returns><c>true</c> if quakelive.exe is found in the correct location depending on the OS installed, otherwise <c>false</c>.</returns>
        public static bool IsQuakeLiveFocusInstalled()
        {
            if (IsWindowsXp())
            {
                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "id Software\\focus\\quakelive.exe")))
                {
                    return true;
                }
            }
            if (IsVistaOrNewer())
            {
                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "id Software\\focus\\quakelive.exe")))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the Quake Live game executable is installed.
        /// </summary>
        /// <returns><c>true</c> if quakelive.exe is found in the correct location depending on the OS installed, otherwise <c>false</c>.</returns>
        public static bool IsQuakeLiveInstalled()
        {
            if (IsWindowsXp())
            {
                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "id Software\\quakelive\\quakelive.exe")))
                {
                    return true;
                }
            }
            if (IsVistaOrNewer())
            {
                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "id Software\\quakelive\\quakelive.exe")))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the host is running Windows Vista or newer.
        /// </summary>
        /// <returns></returns>
        public static bool IsVistaOrNewer()
        {
            var osVersion = Environment.OSVersion;
            return osVersion.Version.Major >= 6;
        }

        /// <summary>
        /// Determines whether the host is running Windows XP.
        /// </summary>
        /// <returns></returns>
        public static bool IsWindowsXp()
        {
            var osVersion = Environment.OSVersion;
            return osVersion.Version.Major == 5 && osVersion.Version.Minor > 0;
        }

        /// <summary>
        /// Gets the known folder path using a specified FOLDERID Guid.
        /// </summary>
        /// <param name="knownFolderId">The known folder identifier.</param>
        /// <returns> The known folder path.
        /// </returns>
        /// <remarks>For GUIDs see: http://msdn.microsoft.com/en-us/library/dd378457.aspx
        /// </remarks>
        private static string GetKnownFolderPath(Guid knownFolderId)
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

        /// <summary>
        /// Gets the DRIVE:\Users\User\AppData\LocalLow directory at the OS level.
        /// </summary>
        /// <returns>The LocalLow directory path as a string.</returns>
        /// <remarks> See: http://stackoverflow.com/a/4495081
        /// </remarks>
        private static string GetLocalAppDataLowPath()
        {
            return GetKnownFolderPath(FOLDERID_LocalAppDataLow);
        }

        [DllImport("shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);
    }
}