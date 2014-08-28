using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Models.DemoPlayer;
using UQLT.ViewModels;

namespace UQLT.Core.Modules.DemoPlayer
{
    /// <summary>
    /// Class responsible for populating the user's demo list.
    /// </summary>
    public class DemoPopulate
    {
        private readonly string _sqlConString = "Data Source=" + UQltFileUtils.GetDemoDatabasePath();
        private readonly string _sqlDbPath = UQltFileUtils.GetDemoDatabasePath();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DemoPopulate"/> class.
        /// </summary>
        /// <param name="dpvm">The <see cref="DemoPlayerViewModel"/> associated with this class.</param>
        public DemoPopulate(DemoPlayerViewModel dpvm)
        {
            DpVm = dpvm;
        }

        /// <summary>
        /// Gets the <see cref="DemoPlayerViewModel"/> associated with this class.
        /// </summary>
        /// <value>
        /// The <see cref="DemoPlayerViewModel"/> associated with this class.
        /// </value>
        public DemoPlayerViewModel DpVm
        {
            get;
            private set;
        }

        /// <summary>
        /// Populates the user's demo list.
        /// </summary>
        public void PopulateUserDemoList()
        {
            if (!JsonFilesToParseExist()) { return; }
            var jsonFilesToParse = GetDemoJsonFiles();
            foreach (var file in jsonFilesToParse)
            {
                using (StreamReader sr = new StreamReader(file))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();
                    var jsondemos = serializer.Deserialize<List<Demo>>(jsonTextReader);
                    foreach (var jd in jsondemos)
                    {
                        if (DpVm.Demos.Any(d => d.Filename == jd.filename))
                        {
                            Debug.WriteLine(string.Format("User's demo list already contains {0}. Skipping...",
                                jd.filename));
                        }
                        else
                        {
                            var demotoadd = new DemoInfoViewModel(jd);
                            // Must be done on UI thread
                            Execute.OnUIThread(() =>
                            {
                                DpVm.Demos.Add(demotoadd);
                            });
                            Debug.WriteLine(string.Format("Added demo {0} to user's demo list.", jd.filename));
                        }
                    }
                }
            }
            // Clean up
            DeleteParsedJsonFiles();
        }

        /// <summary>
        /// Checks whether the demo database file exists on the disk.
        /// </summary>
        /// <returns><c>true</c> if file exists, otherwise <c>false</c></returns>
        private bool DemoDbExists()
        {
            return (File.Exists(_sqlDbPath));
        }

        /// <summary>
        /// Deletes the demo database file, if it exists.
        /// </summary>
        private void DeleteDemoDb()
        {
            if (!DemoDbExists()) { return; }
            try
            {
                File.Delete(_sqlDbPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to delete demo database: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Creates the demo database file on the disk if it doesn't already exist.
        /// </summary>
        private void CreateDemoDb()
        {
            if (DemoDbExists()) { return; }

            SQLiteConnection.CreateFile(_sqlDbPath);

            try
            {
                using (var sqlcon = new SQLiteConnection(_sqlConString))
                {
                    sqlcon.Open();
                    string s =
                        "CREATE TABLE demos (id INTEGER PRIMARY KEY AUTOINCREMENT, srvinfo TEXT NOT NULL, recorded_by TEXT NOT NULL, protocol TEXT NOT NULL," +
                        " timestamp TEXT NOT NULL, gametype_title TEXT NOT NULL, gametype INTEGER, spectators TEXT NOT NULL, size DOUBLE, filename TEXT NOT NULL," +
                        "players TEXT NOT NULL, map_name TEXT NOT NULL)";
                    using (var cmd = new SQLiteCommand(s, sqlcon))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                Debug.WriteLine("Demo database created.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                DeleteDemoDb();
            }

        }

        /// <summary>
        /// Reads the demo database file and creates <see cref="Demo"/> objects from it
        /// that can then be used to populate the user's demo database file.
        /// </summary>
        public void DemoJsonToDbFormat()
        {
            if (DpVm.Demos.Count == 0) { return; }

        }
        
        
        /// <summary>
        /// Deletes the parsed json files.
        /// </summary>
        private void DeleteParsedJsonFiles()
        {
            if (JsonFilesToParseExist())
            {
                var jsonFiles = GetDemoJsonFiles();
                foreach (var file in jsonFiles)
                {
                    if (!File.Exists(file)) { break; }
                    try
                    {
                        File.Delete(file);
                        Debug.WriteLine(string.Format("[CLEANUP]: Deleted json file {0}", file));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Unable to delete json file: " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the demo json files that need to be parsed.
        /// </summary>
        /// <returns>The list of filepaths to the demo json files that need to be parsed.</returns>
        private IEnumerable<string> GetDemoJsonFiles()
        {
            return Directory.EnumerateFiles(UQltFileUtils.GetDemoParseTempDirectory(), "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => file.ToLowerInvariant().EndsWith("uql", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Checks whether the temporary directory for storing json files exists and there are files in it that can be parsed.
        /// </summary>
        /// <returns><c>true</c> if the directory exists and it contains json files. Otherwise, <c>false</c>.</returns>
        private bool JsonFilesToParseExist()
        {
            if (!ParserDirectoryExists()) { return false; }
            return Directory.EnumerateFiles(UQltFileUtils.GetDemoParseTempDirectory(), "*.*", SearchOption.TopDirectoryOnly)
                .Count(file => file.ToLowerInvariant().EndsWith("uql", StringComparison.OrdinalIgnoreCase)) > 0;
        }

        /// <summary>
        /// Checks whether the temporary directory used for storing parsed demo information (json) exists.
        /// </summary>
        /// <returns><c>true</c> if the directory exists, otherwise <c>false</c>.</returns>
        private bool ParserDirectoryExists()
        {
            return Directory.Exists(UQltFileUtils.GetDemoParseTempDirectory());
        }
    }
}