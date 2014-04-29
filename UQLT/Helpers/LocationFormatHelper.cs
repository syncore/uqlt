using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using UQLT.Models.Filters.User;

namespace UQLT.Helpers
{
    public static class LocationFormatHelper
    {

        static LocationFormatHelper()
        {
            _locations = new Dictionary<int, LocationData>();
        }

        private static Dictionary<int, LocationData> _locations;

        public static Dictionary<int, LocationData> Locations
        {
            get
            {
                return _locations;
            }
            set
            {
                _locations = value;
            }
        }

        private static BitmapImage GetFlag(int location_id)
        {
            try
            {
                return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/flags/" + location_id.ToString() + ".gif", UriKind.RelativeOrAbsolute));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/flags/unknown_flag.gif", UriKind.RelativeOrAbsolute));
            }
        }

        public static void LoadLocations()
        {

            if (!File.Exists(UQLTGlobals.CurrentFilterPath))
            {
                FailsafeFilterHelper failsafe = new FailsafeFilterHelper();
                failsafe.DumpBackupFilters();
            }

            try
            {
                using (StreamReader sr = new StreamReader(UQLTGlobals.CurrentFilterPath))
                {
                    string s = sr.ReadToEnd();
                    ImportedFilters json = JsonConvert.DeserializeObject<ImportedFilters>(s);
                    foreach (var loc in json.serverbrowser_locations)
                    {
                        Debug.WriteLine("Adding location --> Location ID: {0} - Fullname: {1} - City: {2}", loc.location_id, loc.display_name, loc.city);
                        Locations[Convert.ToInt32(loc.location_id)] = new LocationData() { City = loc.city, FullLocationName = loc.display_name, Flag = GetFlag(Convert.ToInt32(loc.location_id)) };
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

    }

    public class LocationData
    {
        public string FullLocationName { get; set; }
        public string City { get; set; }
        public BitmapImage Flag { get; set; }

    }
}
