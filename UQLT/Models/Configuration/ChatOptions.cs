namespace UQLT.Models.Configuration
{
    /// <summary>
    /// Model that represents the chat options.
    /// </summary>
    public class ChatOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user has enabled/disabled the chat history.
        /// </summary>
        /// <value><c>true</c> if chat history is enabled otherwise, <c>false</c>.</value>

        /// <summary>
        /// Gets or sets a value indicating whether the user has allowed/disallowed UQLT to handle
        /// incoming chat messages when user is in game.
        /// </summary>
        /// <value><c>true</c> if UQLT handles in-game chat messages, otherwise, <c>false</c>.</value>
        public bool chat_disable_ingame { get; set; }

        public bool chat_logging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has enabled/disabled the chat beep sound.
        /// </summary>
        /// <value><c>true</c> if chat sound is enabled otherwise, <c>false</c>.</value>

        public bool chat_sound { get; set; }
    }
}