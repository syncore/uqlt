using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UQLT.Models.DemoPlayer;

namespace UQLT.ViewModels
{
    /// <summary>
    /// Individual viewmodel for players contained in a demo. This class wraps a <see cref="Player"/> class and exposes additional
    /// properties specific to the View (in this case, DemoPlayerView).
    /// </summary>
    /// <remarks>This viewmodel does not have a separate view.</remarks>
    [Export(typeof(DemoInfoPlayerViewModel))]
    public class DemoInfoPlayerViewModel
    {
        private static readonly Regex NameColors = new Regex(@"[\^]\d");
        private string _teamName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoInfoPlayerViewModel" /> class.
        /// </summary>
        /// <param name="player">The player.</param>
        [ImportingConstructor]
        public DemoInfoPlayerViewModel(Player player)
        {
            Player = player;
        }

        /// <summary>
        /// Gets the account type image.
        /// </summary>
        /// <value>The account type image.</value>
        /// <remarks>This is either 0 (no subscription) or 1 (subscription, no distinction between pro/prem).
        /// It will not exist for demos prior to 2009, so just return a blank image (0.gif)
        /// </remarks>
        public ImageSource AccountImage
        {
            get
            {
                if (string.Equals(Subscription, "1"))
                {
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/accounttype/" + Subscription + ".gif", UriKind.RelativeOrAbsolute));
                }
                if (string.Equals(Subscription, "0"))
                {
                    return
                        new BitmapImage(
                            new Uri(
                                "pack://application:,,,/QLImages;component/images/accounttype/" + Subscription + ".gif",
                                UriKind.RelativeOrAbsolute));
                }
                return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/accounttype/0.gif", UriKind.RelativeOrAbsolute));
            }
        }

        /// <summary>
        /// Gets the clan tag.
        /// </summary>
        /// <value>
        /// The clan tag.
        /// </value>
        /// <remarks>It looks like this does not exist for demos prior to sometime in 2010.</remarks>
        public string ClanTag
        {
            get
            {
                return !string.IsNullOrEmpty(Player.clan) ? NameColors.Replace(Player.clan, string.Empty) : string.Empty;
            }
        }

        /// <summary>
        /// Gets the player's country.
        /// </summary>
        /// <value>
        /// The player's country.
        /// </value>
        /// <remarks>It looks like this does not exist for demos prior to 2011.</remarks>
        public string Country
        {
            get { return Player.country; }
        }

        /// <summary>
        /// Gets the extended (full) clan name.
        /// </summary>
        /// <value>
        /// The extended clan name.
        /// </value>
        /// <remarks>This does not exist for demos prior to 2011.</remarks>
        public string ExtendedClan
        {
            get
            {
                return !string.IsNullOrEmpty(Player.xclan) ? string.Format("({0})", Player.xclan) : Player.xclan;
            }
        }

        /// <summary>
        /// Gets the player's country flag image.
        /// </summary>
        /// <value>
        /// The player's country flag image.
        /// </value>
        public ImageSource FlagImage
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(Country))
                    {
                        return
                            new BitmapImage(
                                new Uri(
                                    "pack://application:,,,/QLImages;component/images/playerflags/" + Country + ".gif",
                                    UriKind.RelativeOrAbsolute));
                    }
                    return
                        new BitmapImage(
                            new Uri("pack://application:,,,/QLImages;component/images/playerflags/unknown_flag.gif",
                                UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                    return
                        new BitmapImage(
                            new Uri("pack://application:,,,/QLImages;component/images/playerflags/unknown_flag.gif",
                                UriKind.RelativeOrAbsolute));
                }
            }
        }

        /// <summary>
        /// Gets the player name.
        /// </summary>
        /// <value>
        /// The player name.
        /// </value>
        public string Name
        {
            get
            {
                return !string.IsNullOrEmpty(Player.name) ? NameColors.Replace(Player.name, string.Empty) : string.Empty;
            }
        }

        /// <summary>
        /// Gets the demo associated with this viewmodel.
        /// </summary>
        /// <value>
        /// The demo associated with this viewmodel.
        /// </value>
        public Player Player
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the subscription.
        /// </summary>
        /// <value>
        /// The subscription.
        /// </value>
        /// <remarks>This does not exist for demos prior to 2009.</remarks>
        public string Subscription
        {
            get { return Player.subscription; }
        }

        /// <summary>
        /// Gets the team.
        /// </summary>
        /// <value>
        /// The team.
        /// </value>
        public string Team
        {
            get { return Player.team; }
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
                    case "0":
                        _teamName = "None";
                        break;

                    case "1":
                        _teamName = "Red";
                        break;

                    case "2":
                        _teamName = "Blue";
                        break;

                    case "3":
                        _teamName = "Spec";
                        break;
                }
                return _teamName;
            }
            set
            {
                _teamName = value;
            }
        }
    }
}