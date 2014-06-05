using System.Collections.Generic;

namespace UQLT.Models.Chat
{
    /// <summary>
    /// Model for roster groups on the friend list (i.e.: "Online Friends" and "Offline Friends")
    /// </summary>
    public class RosterGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RosterGroup"/> class.
        /// </summary>
        /// <param name="name">The name of the roster group.</param>
        public RosterGroup(string name)
        {
            GroupName = name;
            Friends = new List<Friend>();
        }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        public string GroupName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the friends.
        /// </summary>
        /// <value>
        /// The friends.
        /// </value>
        public List<Friend> Friends
        {
            get;
            set;
        }
    }
}