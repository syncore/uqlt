using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.ComponentModel.Composition;
using UQLT.Models.Chat;
using System.Collections.ObjectModel;

namespace UQLT.ViewModels
{
    [Export(typeof(ChatListDetailsViewModel))]
    
        // Individual buddy list information, no associated View
        public class ChatListDetailsViewModel : PropertyChangedBase
    {

        public ChatGroup ChatGroup
        {
            get;
            private set;
        }

        // Wrapped properties
        public string GroupName
        {
            get
            {
                return ChatGroup.Name;
            }
        }

        public ObservableCollection<Friend> GroupFriends
        {
            get
            {
                return ChatGroup.Friends;
            }
        }

        public string Name { get; set; }

        [ImportingConstructor]
        public ChatListDetailsViewModel(ChatGroup chatgroup)
        {
            ChatGroup = chatgroup;
        }



    }
}
