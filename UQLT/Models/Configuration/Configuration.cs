using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Configuration
{
	/// <summary>
	/// Model representing the user's various application configuration settings saved as json on user's hard disk.
	/// </summary>
	public class Configuration
	{
		public ServerBrowserOptions serverbrowser_options
		{
			get;
			set;
		}

	}
}

