using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using UQLT.Models.Filters.User;

namespace UQLT.Helpers
{
    /// <summary>
    /// The relevant gametype data that we need
    /// </summary>
    public class GametypeData
    {
        /// <summary>
        /// Gets or sets the full name of the gametype.
        /// </summary>
        /// <value>The full name of the gametype.</value>
        public string FullGametypeName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the game icon.
        /// </summary>
        /// <value>The game icon.</value>
        public BitmapImage GameIcon
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the short name of the gametype.
        /// </summary>
        /// <value>The short name of the gametype.</value>
        public string ShortGametypeName
        {
            get;
            set;
        }
    }

    /// <summary>
    /// The relevant location data that we need but the QL API does not provide by default
    /// </summary>
    public class LocationData
    {
        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>The city.</value>
        public string City
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the flag.
        /// </summary>
        /// <value>The flag.</value>
        public BitmapImage Flag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the full name of the location.
        /// </summary>
        /// <value>The full name of the location.</value>
        public string FullLocationName
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Simple singleton (!!!) for storing a few QL server details that would otherwise be expensive
    /// and inefficient to look up and/or that the QL API unfortunately does not provide by default,
    /// i.e.: QL API does not define the location name for location_id's, and while it does define
    /// full game type titles (i.e. "Capture the Flag") for game_type id's, it does not define a
    /// short game type type title (i.e. "CTF"). This type of information is useful in multiple
    /// parts of the application and is stored here for ease of use.
    /// </summary>
    public sealed class QlFormatHelper
    {
        private static readonly QlFormatHelper _instance = new QlFormatHelper();

        private readonly Dictionary<int, GametypeData> _gametypes;

        private readonly Dictionary<object, LocationData> _locations;

        // Static constructor so C# compiler will not mark type as 'beforefieldinit', see http://csharpindepth.com/articles/general/singleton.aspx
        static QlFormatHelper()
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="QlFormatHelper" /> class from being created.
        /// </summary>
        private QlFormatHelper()
        {
            _locations = new Dictionary<object, LocationData>();
            _gametypes = new Dictionary<int, GametypeData>();
            Populate();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static QlFormatHelper Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// Gets the gametypes.
        /// </summary>
        /// <value>The gametypes.</value>
        public Dictionary<int, GametypeData> Gametypes
        {
            get
            {
                return _gametypes;
            }
        }

        /// <summary>
        /// Gets the locations.
        /// </summary>
        /// <value>The locations.</value>
        public Dictionary<object, LocationData> Locations
        {
            get
            {
                return _locations;
            }
        }

        /// <summary>
        /// Populates the dictionaries that will hold the gametype and location data using values
        /// specified from the filter file on the disk.
        /// </summary>
        public void Populate()
        {
            if (!File.Exists(UQltGlobals.CurrentFilterPath))
            {
                var failsafe = new FailsafeFilterHelper();
                failsafe.DumpBackupFilters();
            }

            try
            {
                using (var sr = new StreamReader(UQltGlobals.CurrentFilterPath))
                {
                    string s = sr.ReadToEnd();
                    var json = JsonConvert.DeserializeObject<ImportedFilters>(s);
                    // location data
                    foreach (var loc in json.locations)
                    {
                        // QL API is weird. Some location_ids for filters are not ints but strings
                        // (i.e. "location_id": "North America"). Only add the numeric location ids
                        // to dictionary
                        if (!(loc.location_id is string))
                        {
                            //Debug.WriteLine("Adding location --> Location ID: {0} - Fullname: {1} - City: {2}", loc.location_id, loc.display_name, loc.city);
                            Locations[loc.location_id] = new LocationData
                            {
                                City = loc.city,
                                FullLocationName = loc.display_name,
                                Flag = GetFlag(loc.location_id)
                            };
                        }
                    }
                    // gametype data
                    foreach (var gtype in json.basic_gametypes)
                    {
                        //Debug.WriteLine("Adding gametype --> Full Gametype name: {0} - Short Gametype name: {1}", gtype.display_name, gtype.short_name);
                        Gametypes[(gtype.game_type)] = new GametypeData
                        {
                            FullGametypeName = gtype.display_name,
                            ShortGametypeName = gtype.short_name,
                            GameIcon = GetGameIcon(gtype.game_type)
                        };
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// Gets the flag image.
        /// </summary>
        /// <param name="locationId">The location_id.</param>
        /// <returns>A BitmapImage flag of a specified location_id</returns>
        private BitmapImage GetFlag(object locationId)
        {
            try
            {
                return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/flags/" + locationId + ".gif", UriKind.RelativeOrAbsolute));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/flags/unknown_flag.gif", UriKind.RelativeOrAbsolute));
            }
        }

        /// <summary>
        /// Gets the game icon image.
        /// </summary>
        /// <param name="gameType">The game_type.</param>
        /// <returns>A BitmapImage game icon for a specified game type.</returns>
        private BitmapImage GetGameIcon(int gameType)
        {
            try
            {
                return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/" + gameType.ToString(CultureInfo.InvariantCulture) + ".gif", UriKind.RelativeOrAbsolute));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/unknown_gametype.gif", UriKind.RelativeOrAbsolute));
            }
        }
    }
}