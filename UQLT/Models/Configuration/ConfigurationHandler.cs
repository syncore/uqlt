﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace UQLT.Models.Configuration
{
    /// <summary>
    /// This class is responsible for examining the configuration file and loading default values if
    /// there are problems.
    /// </summary>
    public class ConfigurationHandler
    {
        // Valid values for validation, see: ServerBrowserViewModel.cs
        private readonly int[] _validAutoRefreshIndices = { 0, 1, 2, 3 };

        private readonly int[] _validAutoRefreshSeconds = { 30, 60, 90, 300 };

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
        /// Gets or sets a value indicating whether the server browser is set to automatically refresh.
        /// </summary>
        /// <value><c>true</c> if server browser is set to automatically refresh; otherwise, <c>false</c>.</value>
        public bool SbOptAutoRefresh { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether server browser should display elo search options.
        /// </summary>
        /// <value>
        /// <c>true</c> if server browser is to display elo search options; otherwise, <c>false</c>.
        /// </value>
        public bool SbOptDisplayEloSearch { get; set; }
        
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
        /// Reads the configuration.
        /// </summary>
        public void ReadConfig()
        {
            try
            {
                // Failsafe
                ValidateConfig();

                using (var sr = new StreamReader(UQltGlobals.ConfigPath))
                {
                    string json = sr.ReadToEnd();
                    var cfg = JsonConvert.DeserializeObject<Configuration>(json);

                    // Set the properties Server browser
                    SbOptAutoRefresh = cfg.serverbrowser_options.sb_auto_refresh;
                    SbOptAutoRefreshIndex = cfg.serverbrowser_options.sb_auto_refresh_index;
                    SbOptAutoRefreshSeconds = cfg.serverbrowser_options.sb_auto_refresh_seconds;
                    SbOptDisplayEloSearch = cfg.serverbrowser_options.sb_display_elo_search;
                    // Chat
                    ChatOptLogging = cfg.chat_options.chat_logging;
                    ChatOptSound = cfg.chat_options.chat_sound;
                    ChatOptDisableInGame = cfg.chat_options.chat_disable_ingame;
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
                chat_disable_ingame = true
            };

            var config = new Configuration()
            {
                serverbrowser_options = serverbrowseroptions,
                chat_options = chatoptions
            };

            // Write to disk.
            string json = JsonConvert.SerializeObject(config);
            using (FileStream fs = File.Create(UQltGlobals.ConfigPath))
            using (TextWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine(json);
            }
            Debug.WriteLine("** Wrote DEFAULT/FAILSAFE configuration to disk at: " + UQltGlobals.ConfigPath + " **");
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        public void ValidateConfig()
        {
            try
            {
                using (StreamReader sr = new StreamReader(UQltGlobals.ConfigPath))
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
            var serverbrowseroptions = new ServerBrowserOptions()
            {
                sb_auto_refresh = SbOptAutoRefresh,
                sb_auto_refresh_index = SbOptAutoRefreshIndex,
                sb_auto_refresh_seconds = SbOptAutoRefreshSeconds,
                sb_display_elo_search = SbOptDisplayEloSearch
            };
            var chatoptions = new ChatOptions()
            {
                chat_logging = ChatOptLogging,
                chat_sound = ChatOptSound,
                chat_disable_ingame = ChatOptDisableInGame
            };
            var config = new Configuration()
            {
                serverbrowser_options = serverbrowseroptions,
                chat_options = chatoptions
            };
            // write to disk
            string json = JsonConvert.SerializeObject(config);
            using (FileStream fs = File.Create(UQltGlobals.ConfigPath))
            using (TextWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine(json);
            }
            Debug.WriteLine("** Wrote configuration to disk at: " + UQltGlobals.ConfigPath + " **");
        }
    }
}