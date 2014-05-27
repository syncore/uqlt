using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.User
{

	/// <summary>
	/// Model representing the gametypes of type int that are actually encountered in the server browser, NOT the filter menu.
	/// This class is different from GameType.cs, as that class contains gametypes of both types int and string which are used to build filters.
	/// </summary>
	public class BasicGametypes
	{
		public string display_name
		{
			get;
			set;
		}
		public string short_name
		{
			get;
			set;
		}
		public int game_type
		{
			get;
			set;
		}

		public override string ToString()
		{
			return display_name;
		}
	}
}