using System;
using System.Globalization;
using System.Windows.Data;

namespace UQLT.Converters
{
    /// <summary>
    /// Converter that converts the race score returned by the server into the appropriate time
    /// format for the race gametype
    /// </summary>
    public class ScoreToRaceScoreConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls
        /// this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">
        /// The array of values that the source bindings in the <see
        /// cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see
        /// cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source
        /// binding has no value to provide for conversion.
        /// </param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return
        /// value of <see cref="T:System.Windows.DependencyProperty" />.<see
        /// cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter
        /// did not produce a value, and that the binding will use the <see
        /// cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else
        /// will use the default value.A return value of <see cref="T:System.Windows.Data.Binding"
        /// />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding
        /// does not transfer the value or use the <see
        /// cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = game type values[1] = score

            string score = null;

            if (System.Convert.ToInt32(values[0]) == 2)
            {
                if (System.Convert.ToInt32(values[1]) <= 0 || System.Convert.ToInt32(values[1]) == 2147483647)
                {
                    return "DNF";
                }

                TimeSpan t = TimeSpan.FromMilliseconds(System.Convert.ToInt32(values[1]));
                if ((t.Hours > 0) && (t.Minutes > 0))
                {
                    score = t.ToString(@"hh\h\:mm\m\:ss\s\:fff\m\s");
                }

                if ((t.Hours == 0) && (t.Minutes > 0))
                {
                    score = t.ToString(@"mm\m\:ss\s");
                }

                if ((t.Hours == 0) && (t.Minutes <= 0))
                {
                    score = t.ToString(@"ss\s\:ff\m\s");
                }
            }
            else
            {
                score = values[1].ToString();
            }

            return score;
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">
        /// The array of types to convert to. The array length indicates the number and types of
        /// values that are suggested for the method to return.
        /// </param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}