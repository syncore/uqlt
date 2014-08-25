using System;
using System.Collections.Generic;
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
                    var demos = serializer.Deserialize<List<Demo>>(jsonTextReader);
                    foreach (var d in demos)
                    {
                        var demo = new DemoInfoViewModel(d);
                        if (!DpVm.Demos.Contains(demo))
                        {
                            // Must be done on UI thread
                            Execute.OnUIThread(() => {
                                DpVm.Demos.Add(demo);
                            });
                            Debug.WriteLine(string.Format("Added demo {0} to user's demo list.", demo.Filename));
                        }
                        else
                        {
                            Debug.WriteLine(string.Format("User's file list already contained demo {0}", demo.Filename));
                        }
                    }
                }
                
            }
        }

        /// <summary>
        /// Gets the demo json files that need to be parsed.
        /// </summary>
        /// <returns>The list of filepaths to the demo json files that need to be parsed.</returns>
        private List<string> GetDemoJsonFiles()
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