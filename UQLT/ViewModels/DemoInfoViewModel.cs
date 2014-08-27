using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UQLT.Models.DemoPlayer;

namespace UQLT.ViewModels
{
    /// <summary>
    /// Individual demo viewmodel. This class wraps a <see cref="Demo"/> class and exposes additional
    /// properties specific to the View (in this case, DemoPlayerView).
    /// </summary>
    /// <remarks>This viewmodel does not have a separate view.</remarks>
    [Export(typeof(DemoInfoViewModel))]
    public class DemoInfoViewModel
    {
        private static readonly Regex NameColors = new Regex(@"[\^]\d");

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoInfoViewModel"/> class.
        /// </summary>
        /// <param name="demo">The demo.</param>
        [ImportingConstructor]
        public DemoInfoViewModel(Demo demo)
        {
            this.Demo = demo;
            FormattedDemoInfoPlayers = FormatPlayerCollection(GetAllPlayers());
        }

        /// <summary>
        /// Gets the demo's date.
        /// </summary>
        /// <value>
        /// The demo's date.
        /// </value>
        public string Date
        {
            get { return Demo.timestamp; }
        }

        /// <summary>
        /// Gets the demo associated with this viewmodel.
        /// </summary>
        /// <value>
        /// The demo associated with this viewmodel.
        /// </value>
        public Demo Demo
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the demo server information.
        /// </summary>
        /// <value>
        /// The demo server information.
        /// </value>
        public Srvinfo ServerInfo
        {
            get { return Demo.srvinfo; }
        }

        /// <summary>
        /// Gets the demo filename.
        /// </summary>
        /// <value>
        /// The demo filename.
        /// </value>
        public string Filename
        {
            get { return Demo.filename; }
        }

        /// <summary>
        /// Gets the formatted player list for the demo.
        /// </summary>
        /// <value>The formatted player list.</value>
        public List<DemoInfoPlayerViewModel> FormattedDemoInfoPlayers { get; set; }

        /// <summary>
        /// Gets the type of the game.
        /// </summary>
        /// <value>
        /// The type of the game.
        /// </value>
        public int GameType
        {
            get { return Demo.gametype; }
        }

        /// <summary>
        /// Gets the game type image.
        /// </summary>
        /// <value>The game type image.</value>
        /// <remarks>This is a custom UI setting.</remarks>
        public ImageSource GameTypeImage
        {
            get
            {
                try
                {
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/" + GameType + ".gif", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/gametypes/unknown_game_type.gif", UriKind.RelativeOrAbsolute));
                }
            }
        }

        /// <summary>
        /// Gets the game type title.
        /// </summary>
        /// <value>
        /// The game type title.
        /// </value>
        public string GameTypeTitle
        {
            get { return Demo.gametype_title; }
        }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public string Location
        {
            get { return Demo.srvinfo.sv_location; }
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public string Map
        {
            get { return Demo.map_name; }
        }

        /// <summary>
        /// Gets the map image.
        /// </summary>
        /// <value>The map image.</value>
        public ImageSource MapImage
        {
            get
            {
                try
                {
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/maps/" + Map + ".jpg", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex);
                    return new BitmapImage(new Uri("pack://application:,,,/QLImages;component/images/maps/unknown_map.jpg", UriKind.RelativeOrAbsolute));
                }
            }
        }

        /// <summary>
        /// Gets the players.
        /// </summary>
        /// <value>
        /// The players.
        /// </value>
        public List<Player> Players
        {
            get { return Demo.players; }
        }

        /// <summary>
        /// Gets the protocol.
        /// </summary>
        /// <value>
        /// The protocol.
        /// </value>
        public string Protocol
        {
            get { return Demo.protocol; }
        }

        /// <summary>
        /// Gets the name of the player who recorded the demo.
        /// </summary>
        /// <value>
        /// The name of the player who recorded the demo.
        /// </value>
        public string RecordedBy
        {
            get
            {
                return NameColors.Replace(Demo.recorded_by, string.Empty);
            }
        }

        /// <summary>
        /// Gets the filesize.
        /// </summary>
        /// <value>
        /// The filesize.
        /// </value>
        public string Filesize
        {
            get { return string.Format("{0} MB", Demo.size.ToString("F2")); }
        }

        /// <summary>
        /// Gets the spectators.
        /// </summary>
        /// <value>
        /// The spectators.
        /// </value>
        public List<Player> Spectators
        {
            get { return Demo.spectators; }
        }

        /// <summary>
        /// Gets all players. The demo dumper treats players and spectators as separate lists, so this combines them.
        /// </summary>
        private IEnumerable<Player> GetAllPlayers()
        {
            var allPlayers = Players.ToList();
            allPlayers.AddRange(Spectators);
            return allPlayers;
        }
        
        /// <summary>
        /// Adds the players to a list of players that will be cleanly wrapped by a DemoPlayerInfoViewModel, orders the list by player name, and groups it by team.
        /// </summary>
        /// <param name="players">The players.</param>
        /// <returns>A formatted player list.</returns>
        private List<DemoInfoPlayerViewModel> FormatPlayerCollection(IEnumerable<Player> players)
        {
            var sorted = players.Select(player => new DemoInfoPlayerViewModel(player)).ToList();
            return sorted.OrderByDescending(a => a.Name).GroupBy(a => a.Team).SelectMany(a => a.ToList()).ToList();
        }

    }
}