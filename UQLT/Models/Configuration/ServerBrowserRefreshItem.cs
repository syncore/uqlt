using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Configuration
{
	/// <summary>
	/// Model representing an item in the server browser auto-refresh combobox
	/// </summary>
	public class ServerBrowserRefreshItem
	{

		public string Name
		{
			get;
			set;
		}
		public int Seconds
		{
			get;
			set;
		}

		public override string ToString()
		{
			return Name;
		}

	}
}