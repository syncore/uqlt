using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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

        // 1: demo, 2: practice, 3: in-game
        public int StatusType { get; set; }

        public bool HasStatus { get; set; }

        public bool IsInGame { get; set; }

        public string StatusGameType { get; set; }

        public string StatusGameMap { get; set; }

        public string StatusGameLocation { get; set; }

        public BitmapImage StatusGameFlag { get; set; }

        public string StatusGamePlayerCount { get; set; }
    }
}