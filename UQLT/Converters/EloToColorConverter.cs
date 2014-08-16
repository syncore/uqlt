using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace UQLT.Converters
{
    /// <summary>
    /// Converts a QLRanks Elo value to a color for the UI.
    /// </summary>
    public class EloToColorConverter : IValueConverter
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
            var elo = System.Convert.ToInt64(value);
            Brush color;
            if (elo >= 1700)
            {
                color = Brushes.LimeGreen;
            }
            else if ((elo > 1350) && (elo < 1699))
            {
                color = Brushes.DarkOrange;
            }
            else
            {
                color = Brushes.Crimson;
            }
            return color;
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