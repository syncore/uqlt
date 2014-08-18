using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows;

namespace UQLT.Core.Modules.DemoPlayer
{
    /// <summary>
    /// Class responsible for the dumping of the .dm_73 and/or .dm_90 demo information.
    /// </summary>
    public class DemoDumper
    {
        private volatile bool _isProcessPending;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoDumper"/> class.
        /// </summary>
        public DemoDumper()
        {
            Cleanup();
        }

        /// <summary>
        /// Collects the demos and creates a list that contains lists of filepaths to the demos, with maxDemos per list.
        /// </summary>
        /// <param name="demoFiles">The demofiles.</param>
        /// <returns>Split demo lists to be processed by the dumper.</returns>
        public List<List<string>> CollectDemos(List<string> demoFiles)
        {
            long totalMem = GetTotalInstalledMemory();
            int maxDemosPerProcess = 32;
            if ((totalMem > 1025) && (totalMem <= 4097))
            {
                maxDemosPerProcess = 64;
            }
            else if ((totalMem > 4097))
            {
                maxDemosPerProcess = 96;
            }

            var demosperprocess = new List<List<string>>();
            int numdemoprocesses = 0;
            for (int i = 0; i < demoFiles.Count; i += maxDemosPerProcess)
            {
                demosperprocess.Add(demoFiles.GetRange(i, Math.Min(maxDemosPerProcess, demoFiles.Count - i)));
                ++numdemoprocesses;
            }
            Debug.WriteLine("QLDemoDumper will run a total of {0} processes. Will process {1} demos per process based" +
                            " on total installed RAM of {2} MB.", numdemoprocesses, maxDemosPerProcess, totalMem);
            return demosperprocess;
        }

        /// <summary>
        /// Gets the total installed memory.
        /// </summary>
        /// <returns></returns>
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
                MessageBox.Show("An error occurred while querying for WMI data: " + e.Message);
            }
            return (mem / 1048576);
        }

        
        /// <summary>
        /// Processes the demos.
        /// </summary>
        /// <param name="demoList">The demo list.</param>
        public void ProcessDemos(List<List<string>> demoList)
        {
            CreateTempDemoTexts(demoList);
            WriteDemoDumperExecutable();
            CreateDemoJson();
        }

        /// <summary>
        /// Removes the demo dumper executable and demo parser temporary directory.
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
            List<string> tmpFiles =
                Directory.EnumerateFiles(UQltFileUtils.GetDemoParseTempDirectory(), "*.*", SearchOption.TopDirectoryOnly)
                    .Where(
                        file => file.ToLowerInvariant().EndsWith("tmp", StringComparison.OrdinalIgnoreCase)).ToList();
                            

            //var processList = tmpFiles.Select(tmpFile => new Process()).ToList();
            Debug.WriteLine("Found a total of {0} qdparse temp files", tmpFiles.Count);
            List<Process> processList = new List<Process>();
            for (int i = 0; i < tmpFiles.Count; ++i)
            {
                processList.Add(new Process());
            }

            for (int i = 0; i < processList.Count; ++i)
            {
                var process = processList[i];
                var tmpfile = tmpFiles[i];
                StartDumperProcessThread(process, tmpfile);
            }
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
            if (!File.Exists(UQltFileUtils.GetQlDemoDumperPath())) return;
            try
            {
                File.Delete(UQltFileUtils.GetQlDemoDumperPath());
                Debug.WriteLine("[CLEANUP]: Successfully deleted QLDemoDumper executable.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
        /// Method called to handle the process's Exited event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DumperProcessFinished(object sender, EventArgs e)
        {
            _isProcessPending = false;
            Debug.WriteLine("Process has exitted.");
        }

        private void StartDumperProcessThread(Process process, string tmpFile)
        {
            object[] param = new object[2];
            param[0] = process;
            param[1] = tmpFile;

            var bgThread = new Thread(DumperProcess);
            bgThread.Start(param);

        }


        /// <summary>
        /// Starts the dumper process.
        /// </summary>
        /// <param name="data">The data.</param>
        private void DumperProcess(object data)
        {
            const int waitAmount = 100;
            while (_isProcessPending)
            {
                Thread.Sleep(waitAmount);
                Debug.WriteLine("...Waiting for previous demo dumper process to finish.");
            }
            
            object[] parameters = data as object[];
            if (parameters == null) { return; }

            var process = (Process) parameters[0];
            var tmpFile = (string) parameters[1];


                try
                {
                    _isProcessPending = true;
                    var outputFile = Path.Combine(UQltFileUtils.GetDemoParseTempDirectory(),
                        Path.GetFileNameWithoutExtension(tmpFile) + ".uql");
                    process.StartInfo.FileName = UQltFileUtils.GetQlDemoDumperPath();
                    process.StartInfo.Arguments = string.Format("-l {0} -o {1}", tmpFile, outputFile);
                    process.StartInfo.UseShellExecute = false;
                    process.EnableRaisingEvents = true;
                    process.Exited += DumperProcessFinished;
                    process.Start();
                    Debug.WriteLine("Starting dumper process on demo list: " + tmpFile);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            
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