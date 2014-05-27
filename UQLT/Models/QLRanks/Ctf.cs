using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.QLRanks
{

	/// <summary>
	/// Model representing the capture the flag rank and elo information returned from the QLRanks API
	/// </summary>
	public class Ctf
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

