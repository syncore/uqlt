using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UQLT.Helpers
{
    /// <summary>
    /// Helper class (attached behavior) that only allows textboxes to contain numeric content.
    /// </summary>
    /// <remarks>Attach to XAML with: <TextBox helpers:NumericTextBoxFilter.OnlyAllowNumbers="True" />
    /// </remarks>
    public static class NumericTextBoxFilter
    {
        /// <summary>
        /// Gets the only allow numbers object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static bool GetOnlyAllowNumbers(DependencyObject obj)
        {
            return (bool)obj.GetValue(OnlyAllowNumbersProperty);
        }

        /// <summary>
        /// Sets the only allow numbers object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetOnlyAllowNumbers(DependencyObject obj, bool value)
        {
            obj.SetValue(OnlyAllowNumbersProperty, value);
        }

        /// <summary>
        /// The only allow numbers property.
        /// </summary>
        public static readonly DependencyProperty OnlyAllowNumbersProperty =
          DependencyProperty.RegisterAttached("OnlyAllowNumbers",
          typeof(bool), typeof(NumericTextBoxFilter),
          new UIPropertyMetadata(false, OnOnlyAllowNumbersChanged));

        /// <summary>
        /// Called when only allow numbers property is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnOnlyAllowNumbersChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // ignoring error checking
            var textBox = (TextBox)sender;
            bool isDigitOnly = (bool)(e.NewValue);

            if (isDigitOnly)
                textBox.PreviewTextInput += BlockNonNumericCharacters;
            else
                textBox.PreviewTextInput -= BlockNonNumericCharacters;
        }

        private static void BlockNonNumericCharacters(object sender, TextCompositionEventArgs e)
        {
            foreach (char ch in e.Text)
                if (!Char.IsDigit(ch))
                    e.Handled = true;
        }

    }
}