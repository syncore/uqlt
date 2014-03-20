using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Chat
{
    public class RosterGroup
    {

        public RosterGroup(string name)
        {
            Name = name;
            Friends = new ObservableCollection<Friend>();
        }

        public string Name { get; set; }

        public ObservableCollection<Friend> Friends { get; set; }
    }

}