using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Newtonsoft.Json;
using UQLT.Helpers;
using UQLT.Models.Filters.User;

namespace UQLT.Converters
{

	public class LocIdToNameConverter : IValueConverter
	{

		public LocIdToNameConverter()
		{
			if (!File.Exists(UQLTGlobals.CurrentFilterPath))
			{
				FailsafeFilterHelper failsafe = new FailsafeFilterHelper();
				failsafe.DumpBackupFilters();
			}
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string realname = null;
			try
			{
				using(StreamReader sr = new StreamReader(UQLTGlobals.CurrentFilterPath))
				{
					var x = sr.ReadToEnd();
					var filters = JsonConvert.DeserializeObject<ImportedFilters>(x);

					foreach (var loc in filters.locations)
					{
						if (System.Convert.ToInt32(loc.location_id) == System.Convert.ToInt32(value))
						{
							realname = loc.display_name;

							// Debug.WriteLine("Match found: " + loc.location_id + " matches: " + value);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error: " + ex);
			}

			return realname;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}