using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UQLT.Converters
{
    //-----------------------------------------------------------------------------------------------------
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

        //-----------------------------------------------------------------------------------------------------
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isVisible = (bool)value;
            double width = double.Parse(parameter as string);
            return isVisible ? width : 0.0;
        }

        //-----------------------------------------------------------------------------------------------------
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}