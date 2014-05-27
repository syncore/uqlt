using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.QLRanks
{

	/// <summary>
	/// Model representing the free for all rank and elo information returned from the QLRanks API
	/// </summary>
	public class Ffa
	{
		public int rank
		{
			get;
			set;
		}

		public int elo
		{
			get;
			set;
		}
	}
}
