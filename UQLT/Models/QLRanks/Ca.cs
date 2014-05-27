using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.QLRanks
{

	/// <summary>
	/// Model representing the clan arena rank and elo information returned from the QLRanks API
	/// </summary>
	public class Ca
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