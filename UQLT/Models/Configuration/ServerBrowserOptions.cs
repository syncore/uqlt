using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Configuration
{
	/// <summary>
	/// Model representing the user's server browser options saved as a json file on the user's hard disk.
	/// </summary>
	public class ServerBrowserOptions
	{
		public bool auto_refresh
		{
			get;
			set;
		}
		public int auto_refresh_index
		{
			get;
			set;
		}

		public int auto_refresh_seconds
		{
			get;
			set;
		}
	}
}