using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Interfaces
{
    /// <summary>
    /// Interface for determining host operating system version.
    /// </summary>
    internal interface IOSVersion
    {

        bool IsWindowsXp();
        bool IsVistaOrNewer();

    }
}
