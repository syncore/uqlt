using System.ComponentModel.Composition;
using System.Windows.Media;
using Caliburn.Micro;
using UQLT.ViewModels.ServerBrowser;

namespace UQLT.ViewModels.Chat
{
    /// <summary>
    /// Viewmodel responsible for game invitations
    /// </summary>
    [Export(typeof(GameInvitationViewModel))]
    public class GameInvitationViewModel : IHaveDisplayName
    {
        private string _displayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameInvitationViewModel" /> class.
        /// </summary>
        [ImportingConstructor]
        public GameInvitationViewModel(ServerDetailsViewModel sdvm, string fromUser)
        {
            Sdvm = sdvm;
            FromUser = fromUser;
            _displayName = "Game invitation from: " + fromUser;
        }

        /// <summary>
        /// Gets or Sets the window name.
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        /// <summary>
        /// Gets the flag image.
        /// </summary>
        /// <value>The flag image.</value>
        public ImageSource FlagImage
        {
            get { return Sdvm.FlagImage; }
        }

        /// <summary>
        /// Gets or sets the name of the user who sent the game invitation.
        /// </summary>
        /// <value>The user who sent the game invitation.</value>
        public string FromUser { get; set; }

        /// <summary>
        /// Gets the state of the game.
        /// </summary>
        /// <value>The state of the game.</value>
        public string GameState
        {
            get { return Sdvm.FormattedGameState; }
        }

        /// <summary>
        /// Gets the game type image.
        /// </summary>
        /// <value>The game type image.</value>
        public ImageSource GameTypeImage
        {
            get { return Sdvm.GameTypeImage; }
        }

        /// <summary>
        /// Gets the game type title.
        /// </summary>
        /// <value>The game type title.</value>
        public string GameTypeTitle
        {
            get { return Sdvm.GameTypeTitle; }
        }

        /// <summary>
        /// Gets the map image.
        /// </summary>
        /// <value>The map image.</value>
        public ImageSource MapImage
        {
            get { return Sdvm.MapImage; }
        }

        /// <summary>
        /// Gets the map title.
        /// </summary>
        /// <value>The map title.</value>
        public string MapTitle
        {
            get { return Sdvm.MapTitle; }
        }

        /// <summary>
        /// Gets or sets the Server Details View Model wrapper
        /// </summary>
        /// <value>The Server Details View Model wrapper class.</value>
        public ServerDetailsViewModel Sdvm { get; set; }

        /// <summary>
        /// Gets the short name of the location.
        /// </summary>
        /// <value>The short name of the location.</value>
        public string ShortLocationName
        {
            get { return Sdvm.ShortGameTypeName; }
        }

        /// <summary>
        /// Gets the time remaining.
        /// </summary>
        /// <value>The time remaining.</value>
        public string TimeRemaining
        {
            get { return Sdvm.TimeRemaining; }
        }

        /// <summary>
        /// Gets the total players.
        /// </summary>
        /// <value>The total players.</value>
        public string TotalPlayers
        {
            get { return Sdvm.TotalPlayers; }
        }
    }
}