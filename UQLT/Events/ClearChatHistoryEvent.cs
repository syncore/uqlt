namespace UQLT.Events
{
    /// <summary>
    /// Event that is fired to an individual chat message window whenever the user clears the chat
    /// history from the buddy list context menu (ChatListView). Used because ChatListViewModel does
    /// not have direct access to the ChatMessage window.
    /// </summary>
    public class ClearChatHistoryEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClearChatHistoryEvent" /> class.
        /// </summary>

        public ClearChatHistoryEvent(string currentuser, string remoteuser)
        {
            CurrentUser = currentuser;
            RemoteUser = remoteuser;
        }

        /// <summary>
        /// Gets or sets the currently-logged in user.
        /// </summary>
        /// <value>
        /// The currently-logged in user.
        /// </value>
        public string CurrentUser
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the remote user whose history with the <see cref="CurrentUser"/> is to be cleared
        /// </summary>
        /// <value>
        /// The remote user whose history is to be cleared.
        /// </value>
        public string RemoteUser
        {
            get;
            set;
        }
    }
}