using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Chat
{

	/// <summary>
	/// Model for roster groups on the friend list (i.e.: "Online Friends" and "Offline Friends")
	/// </summary>
	public class RosterGroup
	{

		public RosterGroup(string name)
		{
			GroupName = name;
			Friends = new List<Friend>();
		}

		public string GroupName
		{
			get;
			set;
		}

		public List<Friend> Friends
		{
			get;
			set;
		}
	}

}