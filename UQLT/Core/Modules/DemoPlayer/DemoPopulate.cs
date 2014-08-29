using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private volatile int _archivesCompleted;
        private int _totalArchivesToComplete;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoPopulate"/> class.
        /// </summary>
        /// <param name="dpvm">The <see cref="DemoPlayerViewModel"/> associated with this class.</param>
        public DemoPopulate(DemoPlayerViewModel dpvm)
        {
            DpVm = dpvm;
            VerifyDemoDatabase();
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
        /// Asynchronously populates the user's demo list (ObservableCollection in UI) using the newly parsed demo json.
        /// </summary>
        public async Task PopulateDemoListAsync()
        {
            if (!JsonFilesToParseExist()) { return; }

            var jsonFilesToParse = GetDemoJsonFiles();
            int newDemosAdded = 0;
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
                            Execute.OnUIThread(() => DpVm.Demos.Add(demotoadd));
                            ++newDemosAdded;
                            Debug.WriteLine(string.Format("Added demo {0} to user's demo list.", jd.filename));
                        }
                    }
                }
            }
            // JSON files are no longer needed
            DeleteParsedJsonFiles();

            // Populate SQLite database from newly added demos in the user's demo list
            await Task.Run(() => PopulateDemoDatabaseAsync(newDemosAdded));
        }

        /// <summary>
        /// Populates the user's demo list entirely from the SQLite database.
        /// Typically used when the <see cref="DemoPlayerViewModel"/> is first instantiated.
        /// </summary>
        public void PopulateDemoListFromDatabaseAsync()
        {
            VerifyDemoDatabase();

            try
            {
                using (var sqlcon = new SQLiteConnection(_sqlConString))
                {
                    sqlcon.Open();

                    using (var cmd = new SQLiteCommand(sqlcon))
                    {
                        cmd.CommandText = "SELECT * FROM demos ORDER BY filename DESC";
                        cmd.Prepare();

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    // Demo (as json string) -> Demo object ->
                                    // Wrap Demo object in DemoInfoViewModel ->
                                    // Add to user's demo list
                                    var demo = JsonConvert.DeserializeObject<Demo>((string)reader["demo_info"]);
                                    Execute.OnUIThread(() => DpVm.Demos.Add(new DemoInfoViewModel(demo)));
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Demo database contains no demos.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problem populating user demo list from database: " + ex.Message);
            }
        }

        /// <summary>
        /// All of the demos have been archived in the SQLite demo database.
        /// Resets various progress and process values.
        /// </summary>
        private void AllArchivingCompleted()
        {
            _totalArchivesToComplete = 0;
            _archivesCompleted = 0;
            //UI
            DpVm.HasReceivedArchiveCancelation = false;
            DpVm.ArchivingProgress = 0;
            DpVm.IsArchivingDemos = false;
            DpVm.CanCancelArchive = true;
            Debug.WriteLine("All demo archiving is complete!");
        }

        /// <summary>
        /// Creates the demo database file on the disk if it doesn't already exist.
        /// </summary>
        private void CreateDemoDatabase()
        {
            if (DemoDatabaseExists()) { return; }

            SQLiteConnection.CreateFile(_sqlDbPath);

            try
            {
                using (var sqlcon = new SQLiteConnection(_sqlConString))
                {
                    sqlcon.Open();
                    string s =
                        "CREATE TABLE demos (id INTEGER PRIMARY KEY AUTOINCREMENT, filename TEXT NOT NULL, demo_info TEXT NOT NULL)";
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
                DeleteDemoDatabase();
            }
        }

        /// <summary>
        /// Deletes the demo database file, if it exists.
        /// </summary>
        private void DeleteDemoDatabase()
        {
            if (!DemoDatabaseExists()) { return; }
            try
            {
                File.Delete(_sqlDbPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Unable to delete demo database, error: {0}, will attempt to clear database instead.", ex.Message));
                ResetDemoDatabaseAndUserDemos();
            }
        }

        /// <summary>
        /// Deletes the parsed json files.
        /// </summary>
        private void DeleteParsedJsonFiles()
        {
            if (!JsonFilesToParseExist()) return;
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

        /// <summary>
        /// Checks whether the demo database file exists on the disk.
        /// </summary>
        /// <returns><c>true</c> if file exists, otherwise <c>false</c></returns>
        private bool DemoDatabaseExists()
        {
            return (File.Exists(_sqlDbPath));
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

        /// <summary>
        /// Asynchronously reads the user's demo list after the user adds demos to it, then
        /// creates the correspnding entry in the SQLite demo database containing the demo's filename
        /// and the demo's json representation, which will be used to populate the user's list the next
        /// time the user loads the demo player.
        /// </summary>
        private async Task PopulateDemoDatabaseAsync(int numDemosToArchive)
        {
            if (DpVm.Demos.Count == 0) { return; }

            DpVm.IsArchivingDemos = true;
            VerifyDemoDatabase();
            _totalArchivesToComplete = 0;

            try
            {
                _totalArchivesToComplete = numDemosToArchive;
                using (var sqlcon = new SQLiteConnection(_sqlConString))
                {
                    sqlcon.Open();
                    using (var cmd = new SQLiteCommand(sqlcon))
                    {
                        foreach (var dpvmdemo in DpVm.Demos)
                        {
                            if (DpVm.HasReceivedArchiveCancelation)
                            {
                                sqlcon.Cancel();
                                sqlcon.Close();
                                AllArchivingCompleted();
                                ResetDemoDatabaseAndUserDemos();
                                return;
                            }
                            var demoInfo = JsonConvert.SerializeObject(dpvmdemo.Demo);
                            cmd.CommandText = "INSERT INTO demos(filename, demo_info) VALUES(@filename, @demo_info)";
                            cmd.Prepare();
                            cmd.Parameters.AddWithValue("@filename", dpvmdemo.Demo.filename);
                            cmd.Parameters.AddWithValue("@demo_info", demoInfo);
                            await cmd.ExecuteNonQueryAsync();
                            ++_archivesCompleted;
                            UpdateProgress();
                            Debug.WriteLine(string.Format("Added demo {0} to demo database.", dpvmdemo.Demo.filename));
                        }

                        // All done
                        AllArchivingCompleted();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problem writing demo info to demo database: " + ex.Message);
                AllArchivingCompleted();
            }
        }

        /// <summary>
        /// Resets the demo database and user's demo list.
        /// </summary>
        private void ResetDemoDatabaseAndUserDemos()
        {
            // Clear user demo list
            Execute.OnUIThread(() => DpVm.Demos.Clear());
            // Clear demo database (better than deleting file - prevent file in use IO exception)
            if (!DemoDatabaseExists()) return;
            try
            {
                using (var sqlcon = new SQLiteConnection(_sqlConString))
                {
                    sqlcon.Open();
                    string deletecmd = "DELETE FROM demos";
                    string cleanupcmd = "VACUUM";
                    using (var cmd = new SQLiteCommand(deletecmd, sqlcon))
                    {
                        int i = cmd.ExecuteNonQuery();
                        Debug.WriteLine("[RESET] Cleared demo database -- ended up deleting {0} demos from the database.", i);
                    }
                    using (var cmd = new SQLiteCommand(cleanupcmd, sqlcon))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to clear demo database: " + ex.Message);
            }
        }

        /// <summary>
        /// Updates the total archiving progress value.
        /// </summary>
        private void UpdateProgress()
        {
            double progress = 0.0;
            progress += (double)(_archivesCompleted) / (_totalArchivesToComplete) * 100;
            DpVm.ArchivingProgress = progress;
            Debug.WriteLine("Total archiving progress: {0}", progress);
        }

        /// <summary>
        /// Verifies that the demo database contains the proper table.
        /// </summary>
        private void VerifyDemoDatabase()
        {
            if (!DemoDatabaseExists())
            {
                CreateDemoDatabase();
                return;
            }
            using (var sqlcon = new SQLiteConnection(_sqlConString))
            {
                sqlcon.Open();

                using (var cmd = new SQLiteCommand(sqlcon))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM sqlite_master WHERE type = 'table' AND name = 'demos'";

                    using (var sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            return;
                        }
                        Debug.WriteLine("Demos table not found in DB... Creating...");
                        CreateDemoDatabase();
                    }
                }
            }
        }
    }
}