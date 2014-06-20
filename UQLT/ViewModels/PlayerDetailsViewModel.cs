using System;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using UQLT.Models.QuakeLiveAPI;

namespace UQLT.ViewModels
{
    /// <summary>
    /// Viewmodel wrapper for the Player class. This class wraps the Player class and exposes
    /// additional view-related properties for the View (in this case, ServerBrowserView).
    /// </summary>
    [Export(typeof(PlayerDetailsViewModel))]
    
    public class PlayerDetailsViewModel : PropertyChangedBase
    {
        private static Regex nameColors = new Regex(@"[\^]\d");

        private string _cleanedClan;

        // Custom UI properties
        private string _teamName;

        [ImportingConstructor]
        public PlayerDetailsViewModel(Player player)
        {
            Player = player;
        }

        /// <summary>
        /// Gets the account type image.
        /// </summary>
        /// <value>The account type image.</value>
        /// <remarks>This is a custom UI property.</remarks>
        public ImageSource AccountImage
        {
            get
            {
                return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/accounttype/" + SubLevel.ToString() + ".gif", UriKind.RelativeOrAbsolute));
            }
        }

        /// <summary>
        /// Gets the bot.
        /// </summary>
        /// <value>The bot.</value>
        public int Bot
        {
            get
            {
                return Player.bot;
            }
        }

        /// <summary>
        /// Gets the clan.
        /// </summary>
        /// <value>The clan.</value>
        public string Clan
        {
            get
            {
                return Player.clan;
            }
        }

        /// <summary>
        /// Gets or sets the clan name, with the color characters removed.
        /// </summary>
        /// <value>The clan name with the color characters removed clan.</value>
        /// <remarks>This is a custom UI property.</remarks>
        public string CleanedClan
        {
            get
            {
                _cleanedClan = nameColors.Replace(Clan, string.Empty);
                return _cleanedClan;
            }

            set
            {
                _cleanedClan = value;
                NotifyOfPropertyChange(() => CleanedClan);
            }
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        public string Model
        {
            get
            {
                return Player.model;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return Player.name;
            }
        }

        /// <summary>
        /// Gets the player that this viewmodel wraps.
        /// </summary>
        /// <value>The player that this viewmodel wraps.</value>
        public Player Player
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the player's CA Elo.
        /// </summary>
        /// <value>The player's CA Elo.</value>
        /// <remarks>This is a custom UI property.</remarks>
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
            }
        }

        /// <summary>
        /// Gets or sets the player's CTF Elo.
        /// </summary>
        /// <value>The player's CTF Elo.</value>
        /// <remarks>This is a custom UI property.</remarks>
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

        /// <summary>
        /// Gets or sets the player's Duel Elo.
        /// </summary>
        /// <value>The player's Duel Elo.</value>
        /// <remarks>This is a custom UI property.</remarks>
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

        /// <summary>
        /// Gets the player's Elo depending on the gametype.
        /// </summary>
        /// <value>The player's Elo.</value>
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

        /// <summary>
        /// Gets or sets the player's FFA Elo.
        /// </summary>
        /// <value>The player's FFA Elo.</value>
        /// <remarks>This is a custom UI property.</remarks>
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

        /// <summary>
        /// Gets the type of game the player is currently playing.
        /// </summary>
        /// <value>The player's gametype.</value>
        /// <remarks>This is a custom UI property.</remarks>
        public int PlayerGameType
        {
            get
            {
                return Player.player_game_type;
            }
        }

        /// <summary>
        /// Gets or sets the player's TDM Elo.
        /// </summary>
        /// <value>The player's TDM Elo.</value>
        /// <remarks>This is a custom UI property.</remarks>
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

        /// <summary>
        /// Gets the rank.
        /// </summary>
        /// <value>The rank.</value>
        public int Rank
        {
            get
            {
                return Player.rank;
            }
        }

        /// <summary>
        /// Gets the score.
        /// </summary>
        /// <value>The score.</value>
        public int Score
        {
            get
            {
                return Player.score;
            }
        }

        /// <summary>
        /// Gets the sub level.
        /// </summary>
        /// <value>The sub level.</value>
        public int SubLevel
        {
            get
            {
                return Player.sub_level;
            }
        }

        /// <summary>
        /// Gets the team.
        /// </summary>
        /// <value>The team.</value>
        public int Team
        {
            get
            {
                return Player.team;
            }
        }

        /// <summary>
        /// Gets or sets the name of the team.
        /// </summary>
        /// <value>The name of the team.</value>
        /// <remarks>This is a custom UI property.</remarks>
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
    }
}