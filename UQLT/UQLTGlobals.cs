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
        public static string SavedUserFilterPath { get; private set; }

        public static string CurrentFilterPath { get; private set; }

        // ip address, ping
        public static ConcurrentDictionary<string, long> IPAddressDict { get; private set; }

        // player elos
        public static ConcurrentDictionary<string, int> PlayerEloDuel { get; private set; }

        public static ConcurrentDictionary<string, int> PlayerEloCa { get; private set; }

        public static ConcurrentDictionary<string, int> PlayerEloTdm { get; private set; }

        public static ConcurrentDictionary<string, int> PlayerEloFfa { get; private set; }

        public static ConcurrentDictionary<string, int> PlayerEloCtf { get; private set; }

        // various quakelive.com things
        public static string QLDomainBase { get; private set; }

        public static string QLDomainListFilter { get; private set; }

        public static string QLDomainDetailsIds { get; private set; }

        public static string QLDefaultFilter { get; private set; }

        public static string QLUserAgent { get; private set; }

        public static string QLXMPPDomain { get; private set; }

        static UQLTGlobals()
        {
            // Apparently Visual Studio Designer's ShadowCache cannot create directories, so in debug mode, do not include the data directory in the file path          
          #if (DEBUG)
            SavedUserFilterPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savedfilters.json");
            CurrentFilterPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "currentfilters.json");
          #elif (!DEBUG)
            SavedUserFilterPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\savedfilters.json");
            CurrentFilterPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\currentfilters.json");
           #endif
            IPAddressDict = new ConcurrentDictionary<string, long>();
            PlayerEloDuel = new ConcurrentDictionary<string, int>();
            PlayerEloCa = new ConcurrentDictionary<string, int>();
            PlayerEloTdm = new ConcurrentDictionary<string, int>();
            PlayerEloFfa = new ConcurrentDictionary<string, int>();
            PlayerEloCtf = new ConcurrentDictionary<string, int>();
            QLDomainBase = "http://www.quakelive.com"; // http://focus.quakelive.com
            QLDomainListFilter = QLDomainBase+"/browser/list?filter=";
            QLDomainDetailsIds = QLDomainBase+"/browser/details?ids=";
            
            // default filter: any gametype, any location, etc.
            QLDefaultFilter = QLDomainListFilter + "eyJmaWx0ZXJzIjp7Imdyb3VwIjoiYW55IiwiZ2FtZV90eXBlIjoiYW55IiwiYXJlbmEiOiJhbnkiLCJzdGF0ZSI6ImFueSIsImRpZmZpY3VsdHkiOiJhbnkiLCJsb2NhdGlvbiI6IkFMTCIsInByaXZhdGUiOjAsInByZW1pdW1fb25seSI6MCwicmFua2VkIjoiYW55IiwiaW52aXRhdGlvbl9vbmx5IjowfSwiYXJlbmFfdHlwZSI6IiIsInBsYXllcnMiOltdLCJnYW1lX3R5cGVzIjpbNSw0LDMsMCwxLDksMTAsMTEsOCw2XSwiaWciOiJhbnkifQ==&_=";

            QLUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1003.1 Safari/535.19 Awesomium/1.7.1";

            QLXMPPDomain = "synserv"; // xmpp.quakelive.com, focus.quakelive.com
        }
    }
}
