using System.ComponentModel.Composition;
using Caliburn.Micro;
using UQLT.Helpers;
using UQLT.Models.Chat;

namespace UQLT.ViewModels
{
    [Export(typeof(RosterGroupViewModel))]

    /// <summary>
    /// Individual roster group viewmodel. This class wraps the RosterGroup class and exposes
    /// additional properties specific to the View (in this case, ChatlistView)
    /// </summary>

    public class RosterGroupViewModel : PropertyChangedBase
    {
        private ObservableDictionary<string, FriendViewModel> _friends;

        /// <summary>
        /// Initializes a new instance of the <see cref="RosterGroupViewModel" /> class.
        /// </summary>
        /// <param name="rostergroup">The roster group.</param>
        /// <param name="isautoexpanded">
        /// if set to <c>true</c> then this roster group is automatically expanded in the view,
        /// otherwise, <c>false</c> means not automatically expanded in the view.
        /// </param>
        [ImportingConstructor]
        public RosterGroupViewModel(RosterGroup rostergroup, bool isautoexpanded)
        {
            RostGroup = rostergroup;
            IsAutoExpanded = isautoexpanded;
            _friends = new ObservableDictionary<string, FriendViewModel>();
        }

        /// <summary>
        /// Gets or sets the friends of this roster group.
        /// </summary>
        /// <value>The friends of this roster group.</value>
        public ObservableDictionary<string, FriendViewModel> Friends
        {
            get
            {
                return _friends;
            }

            set
            {
                _friends = value;
                // Not really needed, ObservableDictionary class handles INPC
                NotifyOfPropertyChange(() => Friends);
            }
        }

        /// <summary>
        /// Gets the name of this roster group.
        /// </summary>
        /// <value>The name of the group.</value>
        public string GroupName
        {
            get
            {
                return RostGroup.GroupName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this roster group is automatically expanded.
        /// </summary>
        /// <value><c>true</c> if this roster group is automatically expanded; otherwise, <c>false</c>.</value>
        public bool IsAutoExpanded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the roster group that this viewmodel wraps.
        /// </summary>
        /// <value>The roster group that this viewmodel wraps.</value>
        public RosterGroup RostGroup
        {
            get;
            private set;
        }
    }
}