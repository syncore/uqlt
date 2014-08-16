using System.Collections.Generic;
using UQLT.Core.Modules.Chat;

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
        /// <param name="isfavorite">if set to <c>true</c> then friend is a favorite friend. otherwise, not a favorite friend
        /// if set to <c>false</c></param>
        /// <param name="ispending">if set to <c>true</c> friend's acceptance is pending.</param>
        public Friend(string name, bool isfavorite, bool ispending)
        {
            FriendName = name;
            IsFavorite = isfavorite;
            IsPending = ispending;
            XmppResources = new HashSet<string>();
        }

        /// <summary>
        /// Gets or sets this friend's active XMPP resource.
        /// </summary>
        /// <value>The friend's active XMPP resource.</value>
        public string ActiveXmppResource
        {
            get;
            set;
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
        /// Gets or sets a value indicating whether this friend has multiple XMPP clients.
        /// </summary>
        /// <value><c>true</c> if this friend has multiple XMPP clients; otherwise, <c>false</c>.</value>
        /// <remarks>This will occur when the friend is signed into both QuakeLive and UQLT</remarks>
        public bool HasMultipleXmppClients
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this friend has a XMPP status.
        /// </summary>
        /// <value><c>true</c> if this friend has a XMPP status; otherwise, <c>false</c>.</value>
        public bool HasXmppStatus
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
        /// Gets or sets a value indicating whether acceptance of this friend's
        /// friend request is pending, that is, the friend has not accepted our
        /// friend request yet.
        /// </summary>
        /// <value>
        /// <c>true</c> if this friend's request acceptance is pending
        /// otherwise, <c>false</c>.
        /// </value>
        public bool IsPending
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the status.
        /// </summary>
        /// <value>The type of the status.</value>
        /// <remarks>StatusType can be: 0: nothing, 1: demo, 2: practice, or 3: in-game</remarks>
        public ChatStatusTypes StatusType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the set of this friend's XMPP resources.
        /// </summary>
        /// <value>The set of this friend's XMPP resources.</value>
        public HashSet<string> XmppResources { get; set; }
    }
}