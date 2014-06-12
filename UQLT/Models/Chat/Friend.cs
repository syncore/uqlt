using System.Collections.Generic;
using System.Configuration;
using UQLT.Core.Chat;

namespace UQLT.Models.Chat
{
    /// <summary>
    /// Model for an individual friend on the buddy list
    /// </summary>
    public class Friend
    {

        private HashSet<string> _xmppResources;
        
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
            _xmppResources = new HashSet<string>();
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
        /// <remarks>StatusType can be: 0: nothing, 1: demo, 2: practice, or 3: in-game</remarks>
        public TypeOfStatus StatusType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets this friend's active XMPP resource.
        /// </summary>
        /// <value>
        /// The friend's active XMPP resource.
        /// </value>
        public string ActiveXMPPResource
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the set of this friend's XMPP resources.
        /// </summary>
        /// <value>
        /// The set of this friend's XMPP resources.
        /// </value>
        public HashSet<string> XMPPResources
        {
            get { return _xmppResources; }
            set { _xmppResources = value; }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether this friend has multiple XMPP clients.
        /// </summary>
        /// <value>
        /// <c>true</c> if this friend has multiple XMPP clients; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This will occur when the friend is signed into both QuakeLive and UQLT
        /// </remarks>
        public bool HasMultipleXMPPClients
        {
            get;
            set;
        }

    }
}