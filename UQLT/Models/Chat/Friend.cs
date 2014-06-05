namespace UQLT.Models.Chat
{
    /// <summary>
    /// Model for an individual friend on the buddy list
    /// </summary>
    public class Friend
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Friend" /> class.
        /// </summary>
        /// <param name="name">The name of the friend.</param>
        /// <param name="isfavorite">
        /// if set to <c>true</c> then friend is a favorite friend. otherwise, not a favorite friend
        /// if set to <c>false</c>
        /// </param>
        public Friend(string name, bool isfavorite)
        {
            FriendName = name;
            IsFavorite = isfavorite;
        }

        /// <summary>
        /// Gets or sets the name of the friend.
        /// </summary>
        /// <value>The name of the friend.</value>
        public string FriendName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this friend has a XMPP status.
        /// </summary>
        /// <value><c>true</c> if this friend has a XMPP status; otherwise, <c>false</c>.</value>
        public bool HasXMPPStatus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the friend is a favorite
        /// </summary>
        /// <value><c>true</c> if this friend is a favorite; otherwise, <c>false</c>.</value>
        public bool IsFavorite
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this friend is in game.
        /// </summary>
        /// <value><c>true</c> if this friend is in game; otherwise, <c>false</c>.</value>
        public bool IsInGame
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this friend is online.
        /// </summary>
        /// <value><c>true</c> if this friend is online; otherwise, <c>false</c>.</value>
        public bool IsOnline
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the status.
        /// </summary>
        /// <value>The type of the status.</value>
        /// <remarks>StatusType can be: 1: demo, 2: practice, or 3: in-game</remarks>
        public int StatusType
        {
            get;
            set;
        }
    }
}