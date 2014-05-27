using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Newtonsoft.Json;

namespace UQLT.Converters
{

	/// <summary>
	/// Converter that displays the correct score type heading based on the gametype being played.
	/// </summary>
	public class GametypeToRoundtypeConverter : IValueConverter
	{

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

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
