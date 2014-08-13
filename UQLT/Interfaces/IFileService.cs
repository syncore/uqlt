using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Interfaces
{
    /// <summary>
    /// Interface responsible for various file operations.
    /// </summary>
    internal interface IFileService
    {
        string OpenFileDialog(string filePath);
    }
}
