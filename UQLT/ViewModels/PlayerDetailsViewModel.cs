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

        private int _playerElo;

        public int PlayerElo
        {
            get
            {
                switch (PlayerGameType)
                {
                    case 0:
                        _playerElo = UQLTGlobals.PlayerEloFfa[Name.ToLower()];
                        break;

                    case 4:
                        _playerElo = UQLTGlobals.PlayerEloCa[Name.ToLower()];
                        break;

                    case 1:
                        _playerElo = UQLTGlobals.PlayerEloDuel[Name.ToLower()];
                        break;

                    case 3:
                        _playerElo = UQLTGlobals.PlayerEloTdm[Name.ToLower()];
                        break;

                    case 5:
                        _playerElo = UQLTGlobals.PlayerEloCtf[Name.ToLower()];
                        break;

                    default:
                        _playerElo = 0;
                        break;
                }

                return _playerElo;
            }
        }
    }
}
