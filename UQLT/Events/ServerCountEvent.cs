using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Events
{

	/// <summary>
	/// Event that is fired whenever we receive a new server count from the Server Browser
	/// </summary>
	public class ServerCountEvent
	{

		public ServerCountEvent(int servercount)
		{
			ServerCount = servercount;
		}

		public int ServerCount
		{
			get;
			set;
		}

	}
}