using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.User
{

	/// <summary>
	/// Model representing the GameState settings in the user's filters
	/// </summary>
	public class GameState
	{
		public string display_name
		{
			get;
			set;
		}
		public string state
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