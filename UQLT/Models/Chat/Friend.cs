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
        public Friend(string name)
        {
            Name = name;
        }
        
        public string Name { get; set; }

        public string Status { get; set; }

    }
}
