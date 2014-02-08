using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT
{
    public static class UQLTGlobals
    {
        // Apparently Visual Studio Designer's ShadowCache cannot create directories, so in debug mode, do not include the data directory in the file path
    #if (DEBUG)
        public static string SavedUserFilterPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savedfilters.json");
        public static string CurrentFilterPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "currentfilters.json");
    #elif (!DEBUG)
            public static string SavedUserFilterPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\savedfilters.json");
            public static string CurrentFilterPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\currentfilters.json");
    #endif

        // ip address, ping
        public static ConcurrentDictionary<string, long> IPAddressDict = new ConcurrentDictionary<string, long>();

        // player elos
        public static ConcurrentDictionary<string, int> PlayerEloDuel = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, int> PlayerEloCa = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, int> PlayerEloTdm = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, int> PlayerEloFfa = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, int> PlayerEloCtf = new ConcurrentDictionary<string, int>();
    }
}
