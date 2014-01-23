using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT
{
    public class UQLTFilepaths
    {

        // Apparently Visual Studio Designer's ShadowCache cannot create directories, so in debug mode, do not include the data directory in the file path
#if (DEBUG)
        public static string saveduserfilterpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savedfilters.json");
        public static string currentfilterpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "currentfilters.json");
#elif (!DEBUG)
            public static string saveduserfilterpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\savedfilters.json");
            public static string currentfilterpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\currentfilters.json");
#endif
    
    }
}
