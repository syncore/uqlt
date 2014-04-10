using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Chat
{
    // Individual friend

    public class Friend
    {
        public Friend(string name, bool isfavorite)
        {
            FriendName = name;
            IsFavorite = isfavorite;
        }

        public string FriendName { get; set; }

        public bool IsFavorite { get; set; }

        public string Status { get; set; }

    }
}