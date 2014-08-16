using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Newtonsoft.Json;
using UQLT.Core;
using UQLT.Models.Filters.User;

namespace UQLT.Converters
{
    /// <summary>
    /// Converter that converts a Quake Live location id to the proper geographic name of the server.
    /// </summary>
    public class LocIdToNameConverter : IValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocIdToNameConverter" /> class.
        /// </summary>
        public LocIdToNameConverter()
        {
            if (!File.Exists(UQltFileUtils.GetCurrentFilterPath()))
            {
                UQltFileUtils.ExtractFailsafeFilters();
            }
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string realname = null;
            try
            {
                using (var sr = new StreamReader(UQltFileUtils.GetCurrentFilterPath()))
                {
                    string x = sr.ReadToEnd();
                    var filters = JsonConvert.DeserializeObject<ImportedFilters>(x);

                    foreach (
                        Location loc in
                            filters.locations.Where(
                                loc => System.Convert.ToInt32(loc.location_id) == System.Convert.ToInt32(value)))
                    {
                        realname = loc.display_name;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex);
            }

            return realname;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}