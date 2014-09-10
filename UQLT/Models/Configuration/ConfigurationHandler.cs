using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UQLT.Core;

namespace UQLT.Models.Configuration
{
    /// <summary>
    /// Class responsible for handling various configuration options.
    /// </summary>
    public class ConfigurationHandler
    {
        // Valid values for validation, see: ServerBrowserViewModel.cs
        private readonly int[] _validAutoRefreshIndices = { 0, 1, 2, 3 };

        private readonly int[] _validAutoRefreshSeconds = { 30, 60, 90, 300 };

        /// <summary>
        /// Gets or sets the favorite friends for the currently logged-in user.
        /// </summary>
        /// <value>
        /// The currently logged-in user's favorite friends.
        /// </value>
        public List<string> ChatFavoriteFriends { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether UQLT chat is disabled when in-game.
        /// </summary>
        /// <value><c>true</c> if UQLT chat is disabled when in-game; otherwise, <c>false</c>.</value>
        public bool ChatOptDisableInGame { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether logging of the chat history is enabled.
        /// </summary>
        /// <value><c>true</c> if logging of the chat history is enabled; otherwise, <c>false</c>.</value>
        public bool ChatOptLogging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the chat beep sound is enabled.
        /// </summary>
        /// <value><c>true</c> if chat beep sound is enabled; otherwise, <c>false</c>.</value>
        public bool ChatOptSound { get; set; }

        /// <summary>
        /// Gets or sets file path to the custom demo config to be used with Quake Live client.
        /// </summary>
        /// <value>
        /// File path to the custom demo config to be used with Quake Live client.
        /// </value>
        public string DemoOptQlCfgPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a custom demo config should be used with the Quake Live client to play new .dm_90 demos.
        /// </summary>
        /// <value>
        /// <c>true</c> Use a custom demo cfg with Quake Live client; otherwise, <c>false</c>.
        /// </value>
        public bool DemoOptUseCustomQlCfg { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a custom demo config should be used with WolfcamQL to play old .dm_73 demos.
        /// </summary>
        /// <value>
        /// <c>true</c> Use a custom demo cfg with WolfcamQL; otherwise, <c>false</c>.
        /// </value>
        public bool DemoOptUseCustomWolfcamQlCfg { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a custom demo config should be used with Wolf Whisperer to play old .dm_73 demos.
        /// </summary>
        /// <value>
        /// <c>true</c> Use a custom demo cfg with Wolf Whisperer; otherwise, <c>false</c>.
        /// </value>
        public bool DemoOptUseCustomWolfWhispererCfg { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether WolfcamQL should be used to play old .dm_73 demos.
        /// </summary>
        /// <value>
        ///   <c>true</c> if WolfcamQL should be used to play old .dm_73 demos, otherwise, <c>false</c>.
        /// </value>
        public bool DemoOptUseWolfcamQl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Wolf Whisperer should be used to play old .dm_73 demos.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Wolf Whisperer should be used to play old .dm_73 demos, otherwise, <c>false</c>.
        /// </value>
        public bool DemoOptUseWolfWhisperer { get; set; }

        /// <summary>
        /// Gets or sets file path to the custom demo config to be used with WolfcamQL.
        /// </summary>
        /// <value>
        /// File path to the custom demo config to be used with WolfcamQL.
        /// </value>
        public string DemoOptWolfcamQlCfgPath { get; set; }

        /// <summary>
        /// Gets or sets file path to the WolfcamQL executable.
        /// </summary>
        /// <value>
        /// The WolfcamQL executable file path.
        /// </value>
        public string DemoOptWolfcamQlExePath { get; set; }

        /// <summary>
        /// Gets or sets file path to the custom demo config to be used with Wolf Whisperer.
        /// </summary>
        /// <value>
        /// File path to the custom demo config to be used with Wolf Whisperer.
        /// </value>
        public string DemoOptWolfWhispererCfgPath { get; set; }

        /// <summary>
        /// Gets or sets the file path to the Wolf Whisperer executable.
        /// </summary>
        /// <value>
        /// The Wolf Whisperer executable file path.
        /// </value>
        public string DemoOptWolfWhispererExePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the server browser is set to automatically refresh.
        /// </summary>
        /// <value><c>true</c> if server browser is set to automatically refresh; otherwise, <c>false</c>.</value>
        public bool SbOptAutoRefresh { get; set; }

        /// <summary>
        /// Gets or sets the index of the server browser automatic refresh option for the UI.
        /// </summary>
        /// <value>The index of the server browser automatic refresh option for the UI.</value>
        public int SbOptAutoRefreshIndex { get; set; }

        /// <summary>
        /// Gets or sets the time, in seconds, to automatically refresh the server browser.
        /// </summary>
        /// <value>The time, in seconds, to automatically refresh the server browser.</value>
        public int SbOptAutoRefreshSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether server browser should display elo search options.
        /// </summary>
        /// <value>
        /// <c>true</c> if server browser is to display elo search options; otherwise, <c>false</c>.
        /// </value>
        public bool SbOptDisplayEloSearch { get; set; }

        /// <summary>
        /// Reads the configuration.
        /// </summary>
        public void ReadConfig()
        {
            try
            {
                // Failsafe
                ValidateConfig();

                using (var sr = new StreamReader(UQltFileUtils.GetConfigurationPath()))
                {
                    string json = sr.ReadToEnd();
                    var cfg = JsonConvert.DeserializeObject<Configuration>(json);

                    // Server browser
                    SbOptAutoRefresh = cfg.serverbrowser_options.sb_auto_refresh;
                    SbOptAutoRefreshIndex = cfg.serverbrowser_options.sb_auto_refresh_index;
                    SbOptAutoRefreshSeconds = cfg.serverbrowser_options.sb_auto_refresh_seconds;
                    SbOptDisplayEloSearch = cfg.serverbrowser_options.sb_display_elo_search;
                    // Chat
                    ChatOptLogging = cfg.chat_options.chat_logging;
                    ChatOptSound = cfg.chat_options.chat_sound;
                    ChatOptDisableInGame = cfg.chat_options.chat_disable_ingame;
                    ChatFavoriteFriends = cfg.chat_options.chat_favorite_friends;
                    // Demo Player
                    DemoOptUseWolfcamQl = cfg.demo_options.demo_use_wolfcamql;
                    DemoOptUseWolfWhisperer = cfg.demo_options.demo_use_wolfwhisperer;
                    DemoOptWolfcamQlExePath = cfg.demo_options.demo_wolfcamql_exepath;
                    DemoOptWolfWhispererExePath = cfg.demo_options.demo_wolfwhisperer_exepath;
                    DemoOptQlCfgPath = cfg.demo_options.demo_ql_cfgpath;
                    DemoOptWolfcamQlCfgPath = cfg.demo_options.demo_wolfcamql_cfgpath;
                    DemoOptWolfWhispererCfgPath = cfg.demo_options.demo_wolfwhisperer_cfgpath;
                    DemoOptUseCustomQlCfg = cfg.demo_options.demo_use_cust_ql_cfg;
                    DemoOptUseCustomWolfcamQlCfg = cfg.demo_options.demo_use_cust_wolfcamql_cfg;
                    DemoOptUseCustomWolfWhispererCfg = cfg.demo_options.demo_use_cust_wolfwhisperer_cfg;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading configuration " + ex);
                RestoreDefaultConfig();
            }
        }

        /// <summary>
        /// Restores the default configuration options.
        /// </summary>
        public void RestoreDefaultConfig()
        {
            // Save this configuration as the new default.
            var serverbrowseroptions = new ServerBrowserOptions()
            {
                sb_auto_refresh = false,
                sb_auto_refresh_index = 1,
                sb_auto_refresh_seconds = 60,
                sb_display_elo_search = false
            };
            var chatoptions = new ChatOptions()
            {
                chat_logging = true,
                chat_sound = true,
                chat_disable_ingame = true,
                chat_favorite_friends = new List<string>()
            };
            var demooptions = new DemoOptions()
            {
                demo_use_wolfcamql = false,
                demo_use_wolfwhisperer = false,
                demo_use_cust_ql_cfg = false,
                demo_use_cust_wolfcamql_cfg = false,
                demo_use_cust_wolfwhisperer_cfg = false,
                demo_wolfcamql_exepath = string.Empty,
                demo_wolfwhisperer_exepath = string.Empty,
                demo_ql_cfgpath = string.Empty,
                demo_wolfcamql_cfgpath = string.Empty,
                demo_wolfwhisperer_cfgpath = string.Empty
            };
            var config = new Configuration()
            {
                serverbrowser_options = serverbrowseroptions,
                chat_options = chatoptions,
                demo_options = demooptions
            };

            // Write to disk.
            string json = JsonConvert.SerializeObject(config);
            using (FileStream fs = File.Create(UQltFileUtils.GetConfigurationPath()))
            using (TextWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine(json);
            }
            Debug.WriteLine("** Wrote DEFAULT/FAILSAFE configuration to disk at: " + UQltFileUtils.GetConfigurationPath() + " **");
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        public void ValidateConfig()
        {
            try
            {
                using (StreamReader sr = new StreamReader(UQltFileUtils.GetConfigurationPath()))
                {
                    string json = sr.ReadToEnd();
                    var cfg = JsonConvert.DeserializeObject<Configuration>(json);

                    // Validate. Assume that if one option is invalid the entire config is invalid, so
                    // completely restore from default.

                    // Server Browser options
                    if (!cfg.serverbrowser_options.sb_auto_refresh && !cfg.serverbrowser_options.sb_auto_refresh == false)
                    {
                        RestoreDefaultConfig();
                    }
                    if (!_validAutoRefreshIndices.Contains(cfg.serverbrowser_options.sb_auto_refresh_index))
                    {
                        RestoreDefaultConfig();
                    }
                    if (!_validAutoRefreshSeconds.Contains(cfg.serverbrowser_options.sb_auto_refresh_seconds))
                    {
                        RestoreDefaultConfig();
                    }
                    // Chat options
                    if (!cfg.chat_options.chat_logging && !cfg.chat_options.chat_logging == false)
                    {
                        RestoreDefaultConfig();
                    }
                    if (!cfg.chat_options.chat_sound && !cfg.chat_options.chat_sound == false)
                    {
                        RestoreDefaultConfig();
                    }
                    if (!cfg.chat_options.chat_disable_ingame && !cfg.chat_options.chat_disable_ingame == false)
                    {
                        RestoreDefaultConfig();
                    }
                    // Demo options
                    if (!cfg.demo_options.demo_use_wolfcamql && !cfg.demo_options.demo_use_wolfcamql == false)
                    {
                        RestoreDefaultConfig();
                    }
                    if (!cfg.demo_options.demo_use_wolfwhisperer && !cfg.demo_options.demo_use_wolfwhisperer == false)
                    {
                        RestoreDefaultConfig();
                    }
                    if (!cfg.demo_options.demo_use_cust_ql_cfg && !cfg.demo_options.demo_use_cust_ql_cfg == false)
                    {
                        RestoreDefaultConfig();
                    }
                    if (!cfg.demo_options.demo_use_cust_wolfcamql_cfg && !cfg.demo_options.demo_use_cust_wolfcamql_cfg == false)
                    {
                        RestoreDefaultConfig();
                    }
                    if (!cfg.demo_options.demo_use_cust_wolfwhisperer_cfg && !cfg.demo_options.demo_use_cust_wolfwhisperer_cfg == false)
                    {
                        RestoreDefaultConfig();
                    }
                    // not checking for valid WolfcamQL/Wolfwhisperer file path at this point
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading configuration " + ex);
                RestoreDefaultConfig();
            }
        }

        /// <summary>
        /// Writes the configuration to the disk.
        /// </summary>
        public void WriteConfig()
        {
            var serverbrowseroptions = new ServerBrowserOptions
            {
                sb_auto_refresh = SbOptAutoRefresh,
                sb_auto_refresh_index = SbOptAutoRefreshIndex,
                sb_auto_refresh_seconds = SbOptAutoRefreshSeconds,
                sb_display_elo_search = SbOptDisplayEloSearch
            };
            var chatoptions = new ChatOptions
            {
                chat_logging = ChatOptLogging,
                chat_sound = ChatOptSound,
                chat_disable_ingame = ChatOptDisableInGame,
                chat_favorite_friends = ChatFavoriteFriends
            };
            var demooptions = new DemoOptions
            {
                demo_use_wolfcamql = DemoOptUseWolfcamQl,
                demo_use_wolfwhisperer = DemoOptUseWolfWhisperer,
                demo_use_cust_ql_cfg = DemoOptUseCustomQlCfg,
                demo_use_cust_wolfcamql_cfg = DemoOptUseCustomWolfcamQlCfg,
                demo_use_cust_wolfwhisperer_cfg = DemoOptUseCustomWolfWhispererCfg,
                demo_wolfcamql_exepath = DemoOptWolfcamQlExePath,
                demo_wolfwhisperer_exepath = DemoOptWolfWhispererExePath,
                demo_ql_cfgpath = DemoOptQlCfgPath,
                demo_wolfcamql_cfgpath = DemoOptWolfcamQlCfgPath,
                demo_wolfwhisperer_cfgpath = DemoOptWolfWhispererCfgPath
            };
            var config = new Configuration
            {
                serverbrowser_options = serverbrowseroptions,
                chat_options = chatoptions,
                demo_options = demooptions
            };
            // write to disk
            string json = JsonConvert.SerializeObject(config);
            using (FileStream fs = File.Create(UQltFileUtils.GetConfigurationPath()))
            using (TextWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine(json);
            }
            Debug.WriteLine("** Wrote configuration to disk at: " + UQltFileUtils.GetConfigurationPath() + " **");
        }
    }
}