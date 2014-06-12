using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UQLT.Models.QLRanks;

namespace UQLT
{
    /// <summary>
    /// Static class (!!!) that contains key settings that must be available application-wide.
    /// </summary>
    public static class UQLTGlobals
    {
        /// <summary>
        /// Initializes the <see cref="UQLTGlobals" /> class.
        /// </summary>
        static UQLTGlobals()
        {
            SavedUserFilterPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\savedfilters.json");
            CurrentFilterPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\currentfilters.json");
            SavedFavFriendPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\rosterfav.json");
            ConfigPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\configuration.json");

            IPAddressDict = new ConcurrentDictionary<string, long>();
            PlayerEloInfo = new ConcurrentDictionary<string, EloData>();
            SavedFavoriteFriends = new List<string>();
            QLDomainBase = "http://www.quakelive.com"; // http://focus.quakelive.com
            QLDomainListFilter = QLDomainBase + "/browser/list?filter=";
            QLDomainDetailsIds = QLDomainBase + "/browser/details?ids=";

            // default filter: any gametype, any location, etc.
            //QLDefaultFilter = QLDomainListFilter + "eyJmaWx0ZXJzIjp7Imdyb3VwIjoiYW55IiwiZ2FtZV90eXBlIjoiYW55IiwiYXJlbmEiOiJhbnkiLCJzdGF0ZSI6ImFueSIsImRpZmZpY3VsdHkiOiJhbnkiLCJsb2NhdGlvbiI6IkFMTCIsInByaXZhdGUiOjAsInByZW1pdW1fb25seSI6MCwicmFua2VkIjoiYW55IiwiaW52aXRhdGlvbl9vbmx5IjowfSwiYXJlbmFfdHlwZSI6IiIsInBsYXllcnMiOltdLCJnYW1lX3R5cGVzIjpbNSw0LDMsMCwxLDksMTAsMTEsOCw2XSwiaWciOiJhbnkifQ==&_=";
            QLDefaultFilter = QLDomainListFilter + "eyJmaWx0ZXJzIjp7Imdyb3VwIjoiYW55IiwiZ2FtZV90eXBlIjoiYW55IiwiYXJlbmEiOiJhbnkiLCJzdGF0ZSI6ImFueSIsImRpZmZpY3VsdHkiOiJhbnkiLCJsb2NhdGlvbiI6ImFueSIsInByaXZhdGUiOjAsInByZW1pdW1fb25seSI6MCwiaW52aXRhdGlvbl9vbmx5IjowfSwiYXJlbmFfdHlwZSI6IiIsInBsYXllcnMiOltdLCJnYW1lX3R5cGVzIjpbXSwiaWciOjB9&_=";

            QLUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1003.1 Safari/535.19 Awesomium/1.7.1";

            QLXMPPDomain = "xmpp.quakelive.com"; // xmpp.quakelive.com, focus.quakelive.com
            //QLXMPPDomain = ***REMOVED***; // xmpp.quakelive.com, focus.quakelive.com

            UQLTXMPPResource = "uqlt";
            QuakeLiveXMPPResource = "quakelive";
        }

        /// <summary>
        /// Gets the configuration path.
        /// </summary>
        /// <value>The configuration path.</value>
        public static string ConfigPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current downloaded filter path.
        /// </summary>
        /// <value>The current downloaded filter path.</value>
        public static string CurrentFilterPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ip address, ping concurrent dictionary.
        /// </summary>
        /// <value>The ip address, ping information.</value>
        public static ConcurrentDictionary<string, long> IPAddressDict
        {
            get;
            private set;
        }

        // <summary>
        // Gets the player elo information concurrent dictionary.
        // </summary>
        // <value>The player elo information.</value>
        public static ConcurrentDictionary<string, EloData> PlayerEloInfo
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the quakelive.com default game filter information.
        /// </summary>
        /// <value>The quakelive.com default game filter information.</value>
        public static string QLDefaultFilter
        {
            get;
            private set;
        }

        // <summary>
        // Gets the quakelive.com domain base.
        // </summary>
        // <value>The quakelive.com domain base.</value>
        public static string QLDomainBase
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the quakelive.com domain details ids part of the URL.
        /// </summary>
        /// <value>The quakelive.com domain details ids part of the URL.</value>
        public static string QLDomainDetailsIds
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the quakelive.com domain list filter part of the URL.
        /// </summary>
        /// <value>The quakelive.com domain list filter part of the URL.</value>
        public static string QLDomainListFilter
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the QL client user agent.
        /// </summary>
        /// <value>The QL client user agent.</value>
        public static string QLUserAgent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the quakelive.com XMPP domain.
        /// </summary>
        /// <value>The quakelive.com XMPP domain.</value>
        public static string QLXMPPDomain
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the saved favorite friend path.
        /// </summary>
        /// <value>The saved favorite friend path.</value>
        public static string SavedFavFriendPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the saved favorite friends.
        /// </summary>
        /// <value>The saved favorite friends.</value>
        /// <remarks>
        /// This is necessary because we don't have access to the XMPP roster until authenticated.
        /// </remarks>
        public static List<string> SavedFavoriteFriends
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the saved user filter path.
        /// </summary>
        /// <value>The saved user filter path.</value>
        public static string SavedUserFilterPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the UQLT XMPP resource name
        /// </summary>
        /// <value>
        /// The UQLT XMPP resource name
        /// </value>
        public static string UQLTXMPPResource
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Quake Live XMPP resource name
        /// </summary>
        /// <value>
        /// The Quake Live XMPP resource name
        /// </value>
        public static string QuakeLiveXMPPResource
        {
            get;
            private set;
        }
    }
}