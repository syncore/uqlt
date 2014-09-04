using System.ComponentModel.Composition;
using System.Windows;

namespace UQLT.Interfaces
{
    /// <summary>
    /// MessageBox service interface.
    /// </summary>
    public interface IMsgBoxService
    {
        /// <summary>
        /// Shows an error message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="title">The title of the message window.</param>
        void ShowError(string message, string title);

        /// <summary>
        /// Shows a the simple message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        void ShowSimpleMessage(string message);

        /// <summary>
        /// Shows an information message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="title">The title of the message window.</param>
        void ShowInfoMessage(string message, string title);

        /// <summary>
        /// Shows a the question message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="title">The title of the message window.</param>
        bool AskConfirmationMessage(string message, string title);
    }
}
