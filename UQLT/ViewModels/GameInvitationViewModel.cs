using System.ComponentModel.Composition;

namespace UQLT.ViewModels
{
    /// <summary>
    /// Viewmodel responsible for game invitations
    /// </summary>
    [Export(typeof(GameInvitationViewModel))]
    public class GameInvitationViewModel
    {
        [ImportingConstructor]
        public GameInvitationViewModel()
        {
        }
    }
}