using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using Caliburn.Micro;
using UQLT.Core;
using UQLT.Interfaces;
using UQLT.Models.Configuration;

namespace UQLT.ViewModels
{
    /// <summary>
    /// View model for the chat options
    /// </summary>
    [Export(typeof(ChatOptionsViewModel))]
    public class ChatOptionsViewModel : PropertyChangedBase, IUqltConfiguration, IHaveDisplayName, IViewAware
    {
        private Window _dialogWindow;
        private bool _isChatInGameEnabled;
        private bool _isChatLoggingEnabled;
        private bool _isChatSoundEnabled;
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatOptionsViewModel" /> class.
        /// </summary>

        [ImportingConstructor]
        public ChatOptionsViewModel()
        {
            DisplayName = "Chat options";
            LoadConfig();
        }

        /// <summary>
        /// Raised when a view is attached.
        /// </summary>
        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

        /// <summary>
        /// Gets or Sets the name of this window.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has chosen to disable UQLT chat when
        /// launching QL.
        /// </summary>
        /// <value><c>true</c> if the user disables UQLT chat upon launching QL; otherwise, <c>false</c>.</value>
        public bool IsChatInGameEnabled
        {
            get
            {
                return _isChatInGameEnabled;
            }
            set
            {
                _isChatInGameEnabled = value;
                NotifyOfPropertyChange(() => IsChatInGameEnabled);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user has chat history logging enabled.
        /// </summary>
        /// <value><c>true</c> if the user has logging enabled; otherwise, <c>false</c>.</value>
        public bool IsChatLoggingEnabled
        {
            get
            {
                return _isChatLoggingEnabled;
            }
            set
            {
                _isChatLoggingEnabled = value;
                NotifyOfPropertyChange(() => IsChatLoggingEnabled);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user has the chat sound enabled.
        /// </summary>
        /// <value><c>true</c> if the user has the chat sound enabled; otherwise, <c>false</c>.</value>
        public bool IsChatSoundEnabled
        {
            get
            {
                return _isChatSoundEnabled;
            }
            set
            {
                _isChatSoundEnabled = value;
                NotifyOfPropertyChange(() => IsChatSoundEnabled);
            }
        }

        /// <summary>
        /// Attaches a view to this instance.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The context in which the view appears.</param>
        public void AttachView(object view, object context = null)
        {
            _dialogWindow = view as Window;
            if (ViewAttached != null)
            {
                ViewAttached(this, new ViewAttachedEventArgs
                {
                    Context = context,
                    View = view
                });
            }
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <remarks>For different ways to implement window closing, see: http://stackoverflow.com/questions/10090584/how-to-close-dialog-window-from-viewmodel-caliburnwpf</remarks>
        public void CloseWin()
        {
            _dialogWindow.Close();
        }

        /// <summary>
        /// Checks whether the configuration already exists
        /// </summary>
        /// <returns><c>true</c> if configuration exists, otherwise <c>false</c></returns>
        public bool ConfigExists()
        {
            return File.Exists(UQltFileUtils.GetConfigurationPath());
        }

        /// <summary>
        /// Gets a view previously attached to this instance.
        /// </summary>
        /// <param name="context">The context denoting which view to retrieve.</param>
        /// <returns>The view.</returns>
        public object GetView(object context = null)
        {
            return _dialogWindow;
        }

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        public void LoadConfig()
        {
            if (!ConfigExists())
            {
                LoadDefaultConfig();
            }

            var cfghandler = new ConfigurationHandler();
            cfghandler.ReadConfig();

            IsChatLoggingEnabled = cfghandler.ChatOptLogging;
            IsChatSoundEnabled = cfghandler.ChatOptSound;
            IsChatInGameEnabled = cfghandler.ChatOptDisableInGame;
        }

        /// <summary>
        /// Loads the default configuration.
        /// </summary>
        public void LoadDefaultConfig()
        {
            var cfghandler = new ConfigurationHandler();
            cfghandler.RestoreDefaultConfig();
        }

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        public void SaveConfig()
        {
            var cfghandler = new ConfigurationHandler();
            cfghandler.ReadConfig();

            cfghandler.ChatOptLogging = IsChatLoggingEnabled;
            cfghandler.ChatOptSound = IsChatSoundEnabled;
            cfghandler.ChatOptDisableInGame = IsChatInGameEnabled;

            cfghandler.WriteConfig();
        }
    }
}