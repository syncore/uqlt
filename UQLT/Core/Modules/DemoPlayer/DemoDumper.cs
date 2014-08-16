using System;
using System.Collections.Generic;

namespace UQLT.Core.Modules.DemoPlayer
{
    /// <summary>
    /// Class responsible for the dumping of the .dm_73 and/or .dm_90 demo information.
    /// </summary>
    public class DemoDumper
    {
        public DemoDumper()
        {
        }

        /// <summary>
        /// Gets the maximum demos per dump.
        /// </summary>
        /// <param name="demofiles">The demofiles.</param>
        /// <param name="maxdemos">The maxdemos.</param>
        /// <returns>Split demo lists to be processed by the dumper.</returns>
        public List<List<string>> GetMaxDemosPerDump(List<string> demofiles, int maxdemos = 96)
        {
            var demosperprocess = new List<List<string>>();
            for (int i = 0; i < demofiles.Count; i += maxdemos)
            {
                demosperprocess.Add(demofiles.GetRange(i, Math.Min(maxdemos, demofiles.Count - i)));
            }
            return demosperprocess;
        }
    }
}