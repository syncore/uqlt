using System;
using System.Globalization;
using System.Windows.Data;

namespace UQLT.Converters
{
    /// <summary>
    /// Converter that displays the correct score type heading based on the gametype being played.
    /// </summary>
    public class GametypeToRoundtypeConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string roundtype = null;
            switch (System.Convert.ToInt32(value))
            {
                case 4:
                case 9:
                case 12:
                    roundtype = "Round Limit: ";
                    break;

                case 5:
                case 6:
                case 8:
                    roundtype = "Capture Limit: ";
                    break;

                case 10:
                case 11:
                    roundtype = "Score Limit: ";
                    break;

                default:
                    roundtype = "Frag Limit: ";
                    break;
            }

            return roundtype;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}