using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UQLT.Converters
{
    public class ScoreToRaceScoreConverter : IMultiValueConverter
    {
        // values[0] = game type
        // values[1] = score
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}