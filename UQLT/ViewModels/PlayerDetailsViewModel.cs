using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using UQLT.Models;
using UQLT.Models.QuakeLiveAPI;

namespace UQLT.ViewModels
{

	[Export(typeof(PlayerDetailsViewModel))]

	/// <summary>
	/// Viewmodel wrapper for the Player class. Wraps the Player class and exposes additional view-related properties for the View (in this case, ServerBrowserView)
	/// </summary>

	public class PlayerDetailsViewModel : PropertyChangedBase
	{
		private static Regex namecolors = new Regex(@"[\^]\d");

		[ImportingConstructor]
		public PlayerDetailsViewModel(Player player)
		{
			Player = player;
		}

		public Player Player
		{
			get;
			private set;
		}

		public string Clan
		{
			get
			{
				return Player.clan;
			}
		}

		public int SubLevel
		{
			get
			{
				return Player.sub_level;
			}
		}

		public string Name
		{
			get
			{
				return Player.name;
			}
		}

		public int Bot
		{
			get
			{
				return Player.bot;
			}
		}

		public int Rank
		{
			get
			{
				return Player.rank;
			}
		}

		public int Score
		{
			get
			{
				return Player.score;
			}
		}

		public int Team
		{
			get
			{
				return Player.team;
			}
		}

		public string Model
		{
			get
			{
				return Player.model;
			}
		}

		// Custom UI properties
		private string _teamName;

		public string TeamName
		{
			get
			{
				switch (Team)
				{
					case 0:
						_teamName = "None";
						break;
					case 1:
						_teamName = "Red";
						break;
					case 2:
						_teamName = "Blue";
						break;
					case 3:
						_teamName = "Spec";
						break;
				}

				return _teamName;
			}

			set
			{
				_teamName = value;
				NotifyOfPropertyChange(() => TeamName);
			}
		}

		private string _cleanedClan;

		public string CleanedClan
		{
			get
			{
				_cleanedClan = namecolors.Replace(Clan, string.Empty);
				return _cleanedClan;
			}

			set
			{
				_cleanedClan = value;
				NotifyOfPropertyChange(() => CleanedClan);
			}
		}

		public ImageSource AccountImage
		{
			get
			{
				return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/accounttype/" + SubLevel.ToString() + ".gif", UriKind.RelativeOrAbsolute));
			}
		}

		public int PlayerGameType
		{
			get
			{
				return Player.player_game_type;
			}
		}

		public long PlayerTdmElo
		{
			get
			{
				return Player.tdmelo;
			}
			set
			{
				Player.tdmelo = value;
				NotifyOfPropertyChange(() => PlayerTdmElo);
			}
		}

		public long PlayerCaElo
		{
			get
			{
				return Player.caelo;
			}
			set
			{
				Player.caelo = value;
				NotifyOfPropertyChange(() => PlayerCaElo);
				//NotifyOfPropertyChange(() => PlayerElo);
			}
		}

		public long PlayerFfaElo
		{
			get
			{
				return Player.ffaelo;
			}
			set
			{
				Player.ffaelo = value;
				NotifyOfPropertyChange(() => PlayerFfaElo);
			}
		}

		public long PlayerDuelElo
		{
			get
			{
				return Player.duelelo;
			}
			set
			{
				Player.duelelo = value;
				NotifyOfPropertyChange(() => PlayerDuelElo);
			}
		}

		public long PlayerCtfElo
		{
			get
			{
				return Player.ctfelo;
			}
			set
			{
				Player.ctfelo = value;
				NotifyOfPropertyChange(() => PlayerCtfElo);
			}
		}

		public long PlayerElo
		{
			get
			{
				switch (PlayerGameType)
				{
					case 0:
						return PlayerFfaElo;
					case 4:
						return PlayerCaElo;
					case 1:
						return PlayerDuelElo;
					case 3:
						return PlayerTdmElo;
					case 5:
						return PlayerCtfElo;
					default:
						return 0;
				}
			}
		}
	}
}