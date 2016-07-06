using System.Collections.Concurrent;
using UQLT.Models.QLRanks;

namespace UQLT.Core
{
    /// <summary>
    /// Static class (!!!) that contains key settings that must be available application-wide.
    /// </summary>
    public static class UQltGlobals
    {
        /// <summary>
        /// Initializes the <see cref="UQltGlobals" /> class.
        /// </summary>
        static UQltGlobals()
        {
            PlayerEloInfo = new ConcurrentDictionary<string, EloData>();
            QlDomainBase = "http://www.quakelive.com"; // http://focus.quakelive.com
            QlDomainListFilter = QlDomainBase + "/browser/list?filter=";
            QlDomainDetailsIds = QlDomainBase + "/browser/details?ids=";

            // default filter: any gametype, any location, etc.
            QlDefaultFilter = QlDomainListFilter + "eyJmaWx0ZXJzIjp7Imdyb3VwIjoiYW55IiwiZ2FtZV90eXBlIjoiYW55IiwiYXJlbmEiOiJhbnkiLCJzdGF0ZSI6ImFueSIsImRpZmZpY3VsdHkiOiJhbnkiLCJsb2NhdGlvbiI6ImFueSIsInByaXZhdGUiOjAsInByZW1pdW1fb25seSI6MCwiaW52aXRhdGlvbl9vbmx5IjowfSwiYXJlbmFfdHlwZSI6IiIsInBsYXllcnMiOltdLCJnYW1lX3R5cGVzIjpbXSwiaWciOjB9&_=";

            QlUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1003.1 Safari/535.19 Awesomium/1.7.1";

            QlXmppDomain = ""; // xmpp.quakelive.com, xmpp-focus.quakelive.com

            UqltxmppResource = "uqlt";
            QuakeLiveXmppResource = "quakelive";
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
        public static string QlDefaultFilter
        {
            get;
            private set;
        }

        // <summary>
        // Gets the quakelive.com domain base.
        // </summary>
        // <value>The quakelive.com domain base.</value>
        public static string QlDomainBase
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the quakelive.com domain details ids part of the URL.
        /// </summary>
        /// <value>The quakelive.com domain details ids part of the URL.</value>
        public static string QlDomainDetailsIds
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the quakelive.com domain list filter part of the URL.
        /// </summary>
        /// <value>The quakelive.com domain list filter part of the URL.</value>
        public static string QlDomainListFilter
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the QL client user agent.
        /// </summary>
        /// <value>The QL client user agent.</value>
        public static string QlUserAgent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the quakelive.com XMPP domain.
        /// </summary>
        /// <value>The quakelive.com XMPP domain.</value>
        public static string QlXmppDomain
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Quake Live XMPP resource name
        /// </summary>
        /// <value>The Quake Live XMPP resource name</value>
        public static string QuakeLiveXmppResource
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the UQLT XMPP resource name
        /// </summary>
        /// <value>The UQLT XMPP resource name</value>
        public static string UqltxmppResource
        {
            get;
            private set;
        }
    }
}