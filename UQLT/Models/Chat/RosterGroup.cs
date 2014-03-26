using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace UQLT.Models.Chat
{
    public class RosterGroup
    {

        public RosterGroup(string name)
        {
            GroupName = name;
            Friends = new BindableCollection<Friend>();
        }

        public string GroupName { get; set; }

        public BindableCollection<Friend> Friends { get; set; }
    }

}