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

	/// <summary>
	/// Simple singleton (!!!) for storing a few QL server details that would otherwise be expensive and inefficient to look up and/or that the QL API unfortunately does not provide by default,
	/// i.e.: QL API does not define the location name for location_id's, and while it does define full game type titles (i.e. "Capture the Flag") for game_type id's, it does not define
	/// a short game type type title (i.e. "CTF"). This type of information is useful in multiple parts of the application and is stored here for ease of use.
	/// </summary>
	public sealed class QLFormatHelper
	{
		private static readonly QLFormatHelper _instance = new QLFormatHelper();

		// Static constructor so C# compiler will not mark type as 'beforefieldinit', see http://csharpindepth.com/articles/general/singleton.aspx
		static QLFormatHelper()
		{
		}

		private QLFormatHelper()
		{
			_locations = new Dictionary<object, LocationData>();
			_gametypes = new Dictionary<int, GametypeData>();
			Populate();
		}

		public static QLFormatHelper Instance
		{
			get
			{
				return _instance;
			}
		}

		private readonly Dictionary<object, LocationData> _locations;

		public Dictionary<object, LocationData> Locations
		{
			get
			{
				return _locations;
			}
		}

		private readonly Dictionary<int, GametypeData> _gametypes;

		public Dictionary<int, GametypeData> Gametypes
		{
			get
			{
				return _gametypes;
			}
		}

		private BitmapImage GetFlag(object location_id)
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

		private BitmapImage GetGameIcon(int game_type)
		{
			try
			{
				return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/" + game_type.ToString() + ".gif", UriKind.RelativeOrAbsolute));
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/unknown_gametype.gif", UriKind.RelativeOrAbsolute));
			}
		}

		public void Populate()
		{

			if (!File.Exists(UQLTGlobals.CurrentFilterPath))
			{
				FailsafeFilterHelper failsafe = new FailsafeFilterHelper();
				failsafe.DumpBackupFilters();
			}

			try
			{
				using(StreamReader sr = new StreamReader(UQLTGlobals.CurrentFilterPath))
				{
					string s = sr.ReadToEnd();
					ImportedFilters json = JsonConvert.DeserializeObject<ImportedFilters>(s);
					// location data
					foreach (var loc in json.locations)
					{
						// QL API is weird. Some location_ids for filters are not ints but strings (i.e. "location_id": "North America"). Only add the numeric location ids to dictionary
						if (!(loc.location_id is string))
						{
							//Debug.WriteLine("Adding location --> Location ID: {0} - Fullname: {1} - City: {2}", loc.location_id, loc.display_name, loc.city);
							Locations[loc.location_id] = new LocationData()
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
						Gametypes[(gtype.game_type)] = new GametypeData()
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

	}

	/// <summary>
	/// The relevant location data that we need but the QL API does not provide by default
	/// </summary>
	public class LocationData
	{
		public string FullLocationName
		{
			get;
			set;
		}
		public string City
		{
			get;
			set;
		}
		public BitmapImage Flag
		{
			get;
			set;
		}

	}

	/// <summary>
	/// The relevant gametype data that we need
	/// </summary>
	public class GametypeData
	{
		public string FullGametypeName
		{
			get;
			set;
		}
		public string ShortGametypeName
		{
			get;
			set;
		}
		public BitmapImage GameIcon
		{
			get;
			set;
		}
	}
}