using System.Windows;
using UQLT.Interfaces;

namespace UQLT.Helpers
{
    /// <summary>
    /// Helper class used to implement functionality for various Windows' MessageBoxes using the MVVM pattern.
    /// </summary>
    public class MsgBoxService : IMsgBoxService
    {
        /// <summary>
        /// Shows a message that asks the user a yes or no question.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="title">The title of the message window.</param>
        /// <returns><c>True</c>if the user selects yes, <c>false</c> if the user selects no.</returns>
        public bool AskConfirmationMessage(string message, string title)
        {
            MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            // When using HasFlag, the enum that = 0 (here MessageBox.Yes) will always return true, because HasFlag
            // is equivalent to: HasFlag = (GivenFlag & Value) == GivenFlag;
            // "If the underlying value of flag is zero, the method returns true. If this behavior is not desirable,
            // you can use the Equals method to test for equality with zero and call HasFlag only if the underlying
            // value of flag is non-zero -- see: http://msdn.microsoft.com/en-US/library/system.enum.hasflag.aspx
            return !result.HasFlag(MessageBoxResult.No);
        }

        /// <summary>
        /// Shows an error message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="title">The title of the message window.</param>
        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Shows an information message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="title">The title of the message window.</param>
        public void ShowInfoMessage(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Shows a the simple message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        public void ShowSimpleMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}