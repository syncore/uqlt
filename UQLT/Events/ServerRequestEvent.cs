using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Events
{
    public class ServerRequestEvent
    {
        public ServerRequestEvent(string FilterURL)
        {
            ServerRequestURL = FilterURL;
        }
        public string ServerRequestURL { get; set; }

    }
}
