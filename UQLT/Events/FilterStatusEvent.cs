using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Events
{

	/// <summary>
	/// Event that is fired whenever we receive a new default filter, either through the "make new default" button or "reset filters" button in the filterviewmodel
	/// </summary>
	public class FilterStatusEvent
	{

		public FilterStatusEvent(string gametype, string arena, string location, string gamestate, string gamevisibility, string premium)
		{
			Gametype = gametype;
			Arena = arena;
			Location = location;
			Gamestate = gamestate;
			GameVisibility = gamevisibility;
			Premium = premium;
		}
		public string Gametype
		{
			get;
			set;
		}
		public string Arena
		{
			get;
			set;
		}
		public string Location
		{
			get;
			set;
		}

		public string Gamestate
		{
			get;
			set;
		}
		public string GameVisibility
		{
			get;
			set;
		}
		public string Premium
		{
			get;
			set;
		}
	}
}