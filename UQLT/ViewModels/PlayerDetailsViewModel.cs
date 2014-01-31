using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using UQLT.Models;
using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;

namespace UQLT.ViewModels
{
    [Export(typeof(PlayerDetailsViewModel))]
    public class PlayerDetailsViewModel : PropertyChangedBase
    {
        private string _team_name;
        private string _cleaned_clan;
        private int _player_elo;
        static Regex namecolors = new Regex(@"[\^]\d");


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

        public string clan
        {
            get
            {
                return Player.clan;
            }
            //set { Player.clan = value; NotifyOfPropertyChange(() => clan); }
        }
        public int sub_level
        {
            get
            {
                return Player.sub_level;
            }
            //set { Player.sub_level = value; NotifyOfPropertyChange(() => sub_level); }
        }
        public string name
        {
            get
            {
                return Player.name;
            }
            //set { Player.name = value; NotifyOfPropertyChange(() => name); }
        }
        public int bot
        {
            get
            {
                return Player.bot;
            }
            //set { Player.bot = value; NotifyOfPropertyChange(() => bot); }
        }
        public int rank
        {
            get
            {
                return Player.rank;
            }
            //set { Player.rank = value; NotifyOfPropertyChange(() => rank); }
        }
        public int score
        {
            get
            {
                return Player.score;
            }
            //set { Player.score = value; NotifyOfPropertyChange(() => score); }
        }
        public int team
        {
            get
            {
                return Player.team;
            }
            //set { Player.team = value; NotifyOfPropertyChange(() => team; }
        }
        public string model
        {
            get
            {
                return Player.model;
            }
            //set { Player.model = value; NotifyOfPropertyChange(() => model); }
        }
        // Custom UI properties
        public string team_name
        {
            get
            {
                switch (team)
                {
                    case 0:
                        _team_name = "None";
                        break;
                    case 1:
                        _team_name = "Red";
                        break;
                    case 2:
                        _team_name = "Blue";
                        break;
                    case 3:
                        _team_name = "Spec";
                        break;
                }
                return _team_name;
            }
            set
            {
                _team_name = value;
                NotifyOfPropertyChange(() => team_name);
            }

        }

        public string cleaned_clan
        {
            get
            {
                _cleaned_clan = namecolors.Replace(clan, "");
                return _cleaned_clan;
            }
            set
            {
                _cleaned_clan = value;
                NotifyOfPropertyChange(() => cleaned_clan);
            }
        }

        public ImageSource account_image
        {
            get
            {
                return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/accounttype/" + sub_level.ToString() + ".gif", UriKind.RelativeOrAbsolute));
            }
        }


        public int player_game_type
        {
            get;
            set;
        }

        public int player_elo
        {
            get
            {
                switch (player_game_type)
                {
                    case 0:
                        _player_elo = UQLTGlobals.playereloffa[name.ToLower()];
                        break;

                    case 4:
                        _player_elo = UQLTGlobals.playereloca[name.ToLower()];
                        break;

                    case 1:
                        _player_elo = UQLTGlobals.playereloduel[name.ToLower()];
                        break;

                    case 3:
                        _player_elo = UQLTGlobals.playerelotdm[name.ToLower()];
                        break;

                    case 5:
                        _player_elo = UQLTGlobals.playereloctf[name.ToLower()];
                        break;

                    default:
                        _player_elo = 0;
                        break;
                }
                return _player_elo;

            }
        }
    }
}
