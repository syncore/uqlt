using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.QLRanks
{

	/// <summary>
	/// Model representing all of the elo information for supported QLRanks gametypes. Used as the value of a dictionary storing the QLRanks elo information for each player returned by the QL API.
	/// </summary>
	public class EloData
	{
		public long DuelElo
		{
			get;
			set;
		}
		public long CaElo
		{
			get;
			set;
		}
		public long TdmElo
		{
			get;
			set;
		}
		public long FfaElo
		{
			get;
			set;
		}
		public long CtfElo
		{
			get;
			set;
		}

	}
}
