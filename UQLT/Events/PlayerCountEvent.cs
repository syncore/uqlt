using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Events
{

	/// <summary>
	/// Event that is fired whenever we receive a new player count from the Server Browser
	/// </summary>
	public class PlayerCountEvent
	{

		public PlayerCountEvent(int playercount)
		{
			PlayerCount = playercount;
		}

		public int PlayerCount
		{
			get;
			set;
		}

	}
}