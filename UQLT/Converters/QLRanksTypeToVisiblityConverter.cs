using System;
using System.Windows.Data;

namespace UQLT.Converters
{
    /// <summary>
    /// This converter is used to examine whether the gametype in question is of a type supported by QLRanks (currently 3(tdm), 4(ca), 5(ctf), 0(ffa), 1(duel))
    /// If not, then it sets the width of the Elo column to 0 so that it's effectively hidden in the Player Details pane of the server browser.
    /// This is necessary because GridViewColumn does not have a 'Visibility' property.
    /// See:
    /// http://stackoverflow.com/questions/1392811/c-wpf-make-a-gridviewcolumn-visible-false
    /// http://highfieldtales.wordpress.com/2013/08/05/hacking-gridview-hide-columns/
    /// </summary>
    public class QLRanksTypeToVisiblityConverter : IValueConverter
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
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isVisible = (bool)value;
            double width = double.Parse(parameter as string);
            return isVisible ? width : 0.0;
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
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}