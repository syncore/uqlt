using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UQLT.Core.Modules.Chat;

namespace UQLT.Core
{
    /// <summary>
    /// Helper class responsible for various UQLT file retrieval/extraction and directory operations.
    /// </summary>
    public static class UQltFileUtils
    {
        // TODO: Configurations will be saved in sub-directory named after currently logged in Quake Live account (DataDirectory\User\file.uql)

        private const string ChatHistoryDatabaseFile = "chist.udb";
        private const string ConfigurationFile = "uqltconfig.uql";
        private const string CoreResourceLocation = "UQLT.";
        private const string CurrentFilterFile = "currentfilters.uql";
        private const string DataResourceLocation = "UQLT.Data.";
        private const string DataSoundsResourceLocation = "UQLT.Data.Sounds.";
        private const string DemoDatabaseFile = "demdb.udb";
        private const string DemoPlaylistFile = "dplaylist.uql";
        private const string FriendRequestSound = "friendrequest.wav";
        private const string InviteSound = "invite.wav";
        private const string MessageSound = "notice.wav";
        private const string QlDemoDumperFile = "qldemodumper.exe";
        private const string QlImagesFile = "QLImages.dll";
        private const string SavedUserFilterFile = "savedfilters.uql";
        private static readonly string DataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        private static readonly string ChatHistoryPath = Path.Combine(DataDirectory, ChatHistoryDatabaseFile);
        private static readonly string ConfigurationPath = Path.Combine(DataDirectory, ConfigurationFile);
        private static readonly string CurrentFilterPath = Path.Combine(DataDirectory, CurrentFilterFile);
        private static readonly string DataSoundsDirectory = Path.Combine(DataDirectory, "Sounds");
        private static readonly string DemoDatabasePath = Path.Combine(DataDirectory, DemoDatabaseFile);
        private static readonly string DemoParseTempDirectory = Path.Combine(DataDirectory, "DParse");
        private static readonly string DemoPlaylistPath = Path.Combine(DataDirectory, DemoPlaylistFile);
        private static readonly string QlDemoDumperPath = Path.Combine(DataDirectory, QlDemoDumperFile);
        private static readonly string SavedUserFilterPath = Path.Combine(DataDirectory, SavedUserFilterFile);

        private static readonly List<string> UqltRequiredCoreFiles = new List<string>
        {
            QlImagesFile
        };

        private static readonly List<string> UqltRequiredDataFiles = new List<string>
        {
            CurrentFilterFile
        };

        private static readonly AssemblyName UqltResourceDll = AssemblyName.GetAssemblyName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UQLTRes.dll"));

        private static readonly List<string> UqltSoundFiles = new List<string>
        {
            FriendRequestSound,
            InviteSound,
            MessageSound
        };

        /// <summary>
        /// Creates the uqlt data directory.
        /// </summary>
        public static void CreateUqltDataDirectories()
        {
            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
            }
            if (!Directory.Exists(DataSoundsDirectory))
            {
                Directory.CreateDirectory(DataSoundsDirectory);
            }
        }

        /// <summary>
        /// Extracts the QL demo dumper executable.
        /// </summary>
        public static void ExtractDemoDumperExecutable()
        {
            ExtractEmbeddedUqltResource(UqltResourceDll, DataDirectory, UqltResourceTypes.Data, new List<string> { QlDemoDumperFile });
        }

        /// <summary>
        /// Extracts the UQLT embedded resource to the specified output directory.
        /// </summary>
        /// <param name="name">The assembly name.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="restype">The restype.</param>
        /// <param name="files">The files to copy from the internal project resource location, <see cref="GetInternalResourceLocation" /></param>
        /// <remarks>
        /// Currently this is primarily used to extract an embedded resource from the UQLTRes.dll file. This separate assembly was created to
        /// avoid bloating the main executable any more than need be.
        /// Note: filename in <param name="files"></param> must match the exact filename in the Solution Explorer - Data folder of the UQLTRes project and the files
        /// in Solution Explorer - Data folder in the UQLTRes project must have Build Action set to "Embedded Resource".
        /// </remarks>
        public static void ExtractEmbeddedUqltResource(AssemblyName name, string outputDir, UqltResourceTypes restype, List<string> files)
        {
            var assembly = LoadAssembly(name);
            if (assembly == null) { return; }

            foreach (var file in files)
            {
                using (
                    var stream =
                        assembly.GetManifestResourceStream(GetInternalResourceLocation(restype) + file))
                {
                    if (stream == null) { return; }
                    using (var fileStream = new FileStream(Path.Combine(outputDir, file), FileMode.Create))
                    {
                        for (int i = 0; i < stream.Length; i++)
                        {
                            fileStream.WriteByte((byte)stream.ReadByte());
                        }
                        fileStream.Close();
                        Debug.WriteLine("[RESOURCE EXTRACTED]: Wrote {0} from {1} to {2}", file, name, outputDir);
                    }
                }
            }
        }

        /// <summary>
        /// Extracts the failsafe currentfilters.uql from the resources in case of an error reading
        /// it from the disk.
        /// </summary>
        public static void ExtractFailsafeFilters()
        {
            ExtractEmbeddedUqltResource(UqltResourceDll, DataDirectory, UqltResourceTypes.Data, new List<string> { CurrentFilterFile });
        }

        /// <summary>
        /// Gets the chat history database's file path.
        /// </summary>
        /// <returns>The path to the chat history database file as a string.</returns>
        public static string GetChatHistoryDatabasePath()
        {
            return ChatHistoryPath;
        }

        /// <summary>
        /// Gets the UQLT configuration path.
        /// </summary>
        /// <returns>The path to the user's UQLT configuration file.</returns>
        public static string GetConfigurationPath()
        {
            return ConfigurationPath;
        }

        /// <summary>
        /// Gets the current filter path.
        /// </summary>
        /// <returns>The path to 'data\currentfilters.uql'</returns>
        public static string GetCurrentFilterPath()
        {
            return CurrentFilterPath;
        }

        /// <summary>
        /// Gets the demo database's file path.
        /// </summary>
        /// <returns>The path to the demo database file as a string.</returns>
        public static string GetDemoDatabasePath()
        {
            return DemoDatabasePath;
        }

        /// <summary>
        /// Gets temporary directory used for storing parsed demo information.
        /// </summary>
        /// <returns>The path to the demo parser temporary directory.</returns>
        public static string GetDemoParseTempDirectory()
        {
            return DemoParseTempDirectory;
        }

        /// <summary>
        /// Gets the demo playlist's file path.
        /// </summary>
        /// <returns>The path to the demo playlist file as a string.</returns>
        public static string GetDemoPlaylistPath()
        {
            return DemoPlaylistPath;
        }

        /// <summary>
        /// Gets the resource location.
        /// </summary>
        /// <returns>The constant path to the internal project resources.</returns>
        public static string GetInternalResourceLocation(UqltResourceTypes restype)
        {
            string resourcelocation = string.Empty;
            switch (restype)
            {
                case UqltResourceTypes.Data:
                    resourcelocation = DataResourceLocation;
                    break;

                case UqltResourceTypes.DataSounds:
                    resourcelocation = DataSoundsResourceLocation;
                    break;

                case UqltResourceTypes.Core:
                    resourcelocation = CoreResourceLocation;
                    break;
            }
            return resourcelocation;
        }

        /// <summary>
        /// Gets the path to the QLDemoDumper executable.
        /// </summary>
        /// <returns>The path to the QLDemoDumper executable as a string.</returns>
        public static string GetQlDemoDumperPath()
        {
            return QlDemoDumperPath;
        }

        /// <summary>
        /// Gets the saved user filter path.
        /// </summary>
        /// <returns>The path to 'data\savedfilters.uql'</returns>
        public static string GetSavedUserFilterPath()
        {
            return SavedUserFilterPath;
        }

        /// <summary>
        /// Gets the UQLT chat sound file path.
        /// </summary>
        /// <param name="soundtype">The soundtype.</param>
        /// <returns>The file path for a given chat sound type.</returns>
        public static string GetUqltChatSoundFilePath(ChatSoundTypes soundtype)
        {
            var path = string.Empty;
            switch (soundtype)
            {
                case ChatSoundTypes.MessageSound:
                    path = Path.Combine(DataSoundsDirectory, MessageSound);
                    break;

                case ChatSoundTypes.FriendRequest:
                    path = Path.Combine(DataSoundsDirectory, FriendRequestSound);
                    break;

                case ChatSoundTypes.InvitationSound:
                    path = Path.Combine(DataSoundsDirectory, InviteSound);
                    break;
            }
            return path;
        }

        /// <summary>
        /// Gets the UQLT data directory.
        /// </summary>
        /// <returns>The 'data' directory relative to the main UQLT executable as a string.</returns>
        public static string GetUqltDataDirectory()
        {
            return DataDirectory;
        }

        /// <summary>
        /// Gets the UQLT data sounds directory.
        /// </summary>
        /// <returns>The 'data\sounds' directory relative to the main UQLT executable as a string.</returns>
        public static string GetUqltDataSoundsDirectory()
        {
            return DataSoundsDirectory;
        }

        /// <summary>
        /// Verifies whether or not key UQLT files exist and extracts them from the embedded
        /// resources if they do not. Makes failsafe assumption that if one file of that type
        /// doesn't exist, then none exist.
        /// </summary>
        public static void VerifyUqltFiles()
        {
            int numsoundfiles = 0;
            int numrequiredcorefiles = 0;
            int numrequiredfiles = 0;
            foreach (var soundfile in UqltSoundFiles)
            {
                if (File.Exists(Path.Combine(DataSoundsDirectory, soundfile)))
                {
                    numsoundfiles++;
                }
                if (numsoundfiles != UqltSoundFiles.Count)
                {
                    ExtractUqltSoundFiles();
                }
            }
            foreach (var requiredcorefile in UqltRequiredCoreFiles)
            {
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, requiredcorefile)))
                {
                    numrequiredcorefiles++;
                }
                if (numrequiredcorefiles != UqltRequiredCoreFiles.Count)
                {
                    ExtractUqltRequiredCoreFiles();
                }
            }
            foreach (var requiredfile in UqltRequiredDataFiles)
            {
                if (File.Exists(Path.Combine(DataDirectory, requiredfile)))
                {
                    numrequiredfiles++;
                }
                if (numrequiredfiles != UqltRequiredDataFiles.Count)
                {
                    ExtractUqltRequiredDataFiles();
                }
            }
        }

        /// <summary>
        /// Extracts the required UQLT core files from the embedded resources.
        /// </summary>
        private static void ExtractUqltRequiredCoreFiles()
        {
            ExtractEmbeddedUqltResource(UqltResourceDll, AppDomain.CurrentDomain.BaseDirectory, UqltResourceTypes.Core, UqltRequiredCoreFiles);
        }

        /// <summary>
        /// Extracts the required UQLT data files from the embedded resources.
        /// </summary>
        /// <remarks>Not all files in the data directory are required as of now.</remarks>
        private static void ExtractUqltRequiredDataFiles()
        {
            ExtractEmbeddedUqltResource(UqltResourceDll, DataDirectory, UqltResourceTypes.Data, UqltRequiredDataFiles);
        }

        /// <summary>
        /// Extracts the UQLT sounds from the embedded resources.
        /// </summary>
        private static void ExtractUqltSoundFiles()
        {
            ExtractEmbeddedUqltResource(UqltResourceDll, DataSoundsDirectory, UqltResourceTypes.DataSounds, UqltSoundFiles);
        }

        /// <summary>
        /// Loads the assembly.
        /// </summary>
        /// <param name="name">The assembly name.</param>
        /// <returns>An assembly.</returns>
        private static Assembly LoadAssembly(AssemblyName name)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to load assembly with name: {0}, exception: {1}", name, ex.Message);
            }
            return assembly;
        }
    }
}