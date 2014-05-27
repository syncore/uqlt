using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.QLRanks
{

	/// <summary>
	/// Model representing the outer container object returned from the QLRanks API
	/// </summary>
	public class QLRanks
	{
		public List<QLRanksPlayer> players
		{
			get;
			set;
		}
	}
}
