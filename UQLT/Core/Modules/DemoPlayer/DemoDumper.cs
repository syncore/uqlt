using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Windows;
using UQLT.ViewModels;
using Path = System.IO.Path;

namespace UQLT.Core.Modules.DemoPlayer
{
    /// <summary>
    /// Class responsible for the dumping of the .dm_73 and/or .dm_90 demo information.
    /// </summary>
    public class DemoDumper
    {
        private readonly object _processOutputLock = new Object();
        private Dictionary<Process, string> _processes;
        private volatile int _processesCompleted;
        private StringBuilder _processOutputBuilder;
        private int _totalProcessesToComplete;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoDumper" /> class.
        /// </summary>
        /// <param name="dpvm">The <see cref="DemoPlayerViewModel"/>associated with this class.</param>
        public DemoDumper(DemoPlayerViewModel dpvm)
        {
            DpVm = dpvm;
            Cleanup();
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
        /// Collects the demos and creates a list that contains lists of filepaths to the demos, with maxDemos per list.
        /// </summary>
        /// <param name="demoFiles">The demofiles.</param>
        /// <returns>Split demo lists to be processed by the dumper.</returns>
        public List<List<string>> CollectDemos(List<string> demoFiles)
        {
            long totalMem = GetTotalInstalledMemory();
            int maxDemosPerProcess = 64;
            if ((totalMem > 1025) && (totalMem <= 4097))
            {
                maxDemosPerProcess = 96;
            }
            else if ((totalMem > 4097))
            {
                maxDemosPerProcess = 128;
            }

            var demosperprocess = new List<List<string>>();
            _totalProcessesToComplete = 0;
            for (int i = 0; i < demoFiles.Count; i += maxDemosPerProcess)
            {
                demosperprocess.Add(demoFiles.GetRange(i, Math.Min(maxDemosPerProcess, demoFiles.Count - i)));
                ++_totalProcessesToComplete;
            }
            Debug.WriteLine("QLDemoDumper will run a total of {0} processes. Will process {1} demos per process based" +
                            " on total installed RAM of {2} MB.", _totalProcessesToComplete, maxDemosPerProcess, totalMem);
            return demosperprocess;
        }

        /// <summary>
        /// Processes the demos.
        /// </summary>
        /// <param name="demoList">The demo list.</param>
        public void ProcessDemos(List<List<string>> demoList)
        {
            DpVm.IsProcessingDemos = true;
            DpVm.HasReceivedProcessCancelation = false;
            WriteDemoDumperExecutable();
            CreateTempDemoTexts(demoList);
            CreateDemoJson();
        }

        /// <summary>
        /// All demo processing processes have been completed (either by running through to termination or by being canceled).
        /// Resets various progress and process values.
        /// </summary>
        private void AllProcessesCompleted()
        {
            _totalProcessesToComplete = 0;
            _processesCompleted = 0;
            //UI
            DpVm.ProcessingProgress = 0;
            DpVm.IsProcessingDemos = false;
            DpVm.CanCancelProcess = true;
            // Populate
            var demoPopulate = new DemoPopulate(DpVm);
            demoPopulate.PopulateUserDemoList();
            //File ops
            DeleteTempDemoTexts();
            DeleteDemoDumperExecutable();
        }

        /// <summary>
        /// Removes the demo dumper executable, demo parser temporary directory, and temporary text files.
        /// </summary>
        private void Cleanup()
        {
            DeleteDemoDumperExecutable();
            DeleteDemoParseTempDirectory();
        }

        /// <summary>
        /// Processes the demos.
        /// </summary>
        private void CreateDemoJson()
        {
            var tmpFiles = GetTempDemoTexts();
            Debug.WriteLine("Found a total of {0} qdparse temp files", tmpFiles.Count);
            _processes = new Dictionary<Process, string>();
            foreach (var file in tmpFiles)
            {
                _processes.Add(new Process(), file);
            }
            StartDumperProcessThread(_processes);
        }

        /// <summary>
        /// Creates the demo parser temporary directory.
        /// </summary>
        private void CreateDemoParseTempDirectory()
        {
            if (!Directory.Exists(UQltFileUtils.GetDemoParseTempDirectory()))
            {
                try
                {
                    Directory.CreateDirectory(UQltFileUtils.GetDemoParseTempDirectory());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to create demo parser directory", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Creates the text files containing the dmeo paths that will be parsed by the QLDemoDumper executable.
        /// </summary>
        /// <param name="demoList">The demo list.</param>
        private void CreateTempDemoTexts(List<List<string>> demoList)
        {
            DeleteDemoParseTempDirectory();
            CreateDemoParseTempDirectory();

            for (int i = 0; i < demoList.Count; ++i)
            {
                try
                {
                    File.WriteAllLines(
                        Path.Combine(UQltFileUtils.GetDemoParseTempDirectory(), "qdparse0" + i + ".tmp"), demoList[i]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Deletes the demo dumper executable from the disk if it exists.
        /// </summary>
        private void DeleteDemoDumperExecutable()
        {
            if (!File.Exists(UQltFileUtils.GetQlDemoDumperPath())) { return; }
            try
            {
                File.Delete(UQltFileUtils.GetQlDemoDumperPath());
                Debug.WriteLine("[CLEANUP]: Successfully deleted QLDemoDumper executable.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problem deleting QLDemoDumper executable: " + ex.Message);
            }
        }

        /// <summary>
        /// Deletes the demo parser temporary directory and any files within it.
        /// </summary>
        private void DeleteDemoParseTempDirectory()
        {
            if (!Directory.Exists(UQltFileUtils.GetDemoParseTempDirectory())) return;
            try
            {
                Directory.Delete(UQltFileUtils.GetDemoParseTempDirectory(), true);
                Debug.WriteLine("[CLEANUP]: Successfully deleted demo parser temporary directory.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Deletes the temporary demo text files.
        /// </summary>
        private void DeleteTempDemoTexts()
        {
            var tmpFiles = GetTempDemoTexts();
            foreach (var file in tmpFiles)
            {
                if (!File.Exists(file)) { break; }
                try
                {
                    File.Delete(file);
                    Debug.WriteLine(string.Format("[CLEANUP]: Deleted temporary file {0}", file));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to delete file: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Starts the dumper process.
        /// </summary>
        /// <param name="data">The data.</param>
        private void DumperProcess(object data)
        {
            // 0.1 sec
            //int waitAmount = 100;

            object[] parameters = data as object[];
            if (parameters == null) { return; }
            var processes = (Dictionary<Process, string>)parameters[0];

            foreach (var process in processes)
            {
                var proc = process.Key;
                var tmpFile = process.Value;
                _processOutputBuilder = new StringBuilder("");

                // Process cancellation received from the UI
                if (DpVm.HasReceivedProcessCancelation)
                {
                    // UI responsive
                    DpVm.IsProcessingDemos = false;
                    AllProcessesCompleted();
                    return;
                }
                try
                {
                    var outputFile = Path.Combine(UQltFileUtils.GetDemoParseTempDirectory(),
                        Path.GetFileNameWithoutExtension(process.Value) + ".uql");
                    proc.StartInfo.FileName = UQltFileUtils.GetQlDemoDumperPath();
                    proc.StartInfo.Arguments = string.Format("-l {0} -o {1}", tmpFile, outputFile);
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.OutputDataReceived += DumperProcessOutputHandler;
                    proc.EnableRaisingEvents = true;
                    proc.Exited += DumperProcessFinished;
                    proc.Start();
                    proc.BeginOutputReadLine();
                    Debug.WriteLine("Starting dumper process on demo list: " + tmpFile);
                    // Wait for previous process to exit and block current thread until it does so
                    proc.WaitForExit();
                    // Console ouput after exit
                    Debug.WriteLine(_processOutputBuilder);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Method called to handle the process's Exited event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DumperProcessFinished(object sender, EventArgs e)
        {
            ++_processesCompleted;
            UpdateProgress();
            if (_processesCompleted == _totalProcessesToComplete)
            {
                AllProcessesCompleted();
            }

            Debug.WriteLine("Dumper process has exited.");
        }

        /// <summary>
        /// Handles any output received from the demo dumper process.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="line">The <see cref="DataReceivedEventArgs"/> instance containing the event data.</param>
        private void DumperProcessOutputHandler(object sender, DataReceivedEventArgs line)
        {
            if (!string.IsNullOrEmpty(line.Data))
            {
                //StringBuilder is not thread-safe
                //lock (_processOutputLock)
                //{
                _processOutputBuilder.Append(Environment.NewLine + line.Data);
                if (line.Data.Contains("Traceback (most"))
                {
                    MessageBox.Show("Error occurred while parsing demos!", "Demo parse error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    //}
                }
            }
        }

        /// <summary>
        /// Gets the temporary demo texts files.
        /// </summary>
        /// <returns>A list of the paths to the temporary text files that are to be sent to the dumper and/or cleaned up.</returns>
        private List<string> GetTempDemoTexts()
        {
            return Directory.EnumerateFiles(UQltFileUtils.GetDemoParseTempDirectory(), "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => file.ToLowerInvariant().EndsWith("tmp", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Gets the total installed memory.
        /// </summary>
        /// <returns>The total installed memory in megabytes.</returns>
        private long GetTotalInstalledMemory()
        {
            long mem = 0;
            try
            {
                var searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_ComputerSystem");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    Debug.WriteLine("System's total installed RAM {0}", queryObj["TotalPhysicalMemory"]);
                    mem = Convert.ToInt64(queryObj["TotalPhysicalMemory"]);
                }
            }
            catch (ManagementException e)
            {
                Debug.WriteLine("An error occurred while querying for WMI data: " + e.Message);
            }
            return (mem / 1048576);
        }

        /// <summary>
        /// Starts the dumper process thread.
        /// </summary>
        /// <param name="processes">The processes.</param>
        private void StartDumperProcessThread(Dictionary<Process, string> processes)
        {
            object[] param = new object[1];
            param[0] = processes;

            var bgThread = new Thread(DumperProcess);
            bgThread.Start(param);
        }

        /// <summary>
        /// Updates the total progress value.
        /// </summary>
        private void UpdateProgress()
        {
            double progress = 0.0;
            progress += (double)(_processesCompleted) / (_totalProcessesToComplete) * 100;
            DpVm.ProcessingProgress = progress;
            Debug.WriteLine("Total progress: {0}", progress);
        }

        /// <summary>
        /// Writes the demo dumper executable to the disk.
        /// </summary>
        private void WriteDemoDumperExecutable()
        {
            UQltFileUtils.ExtractDemoDumperExecutable();
        }
    }
}