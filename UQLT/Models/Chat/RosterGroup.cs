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

        public RosterGroup()
        {
            this.Members = new ObservableCollection<RosterMember>();
        }

        public string Name { get; set; }

        public ObservableCollection<RosterMember> Members { get; set; }
    }

}