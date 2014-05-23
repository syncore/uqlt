using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Events
{
    //-----------------------------------------------------------------------------------------------------
    /// <summary>
    /// Event that is fired whenever we receive a new default filter, either through the "make new default" button or "reset filters" button in the filterviewmodel
    /// </summary>
    public class ServerRequestEvent
    {
        //-----------------------------------------------------------------------------------------------------
        public ServerRequestEvent(string filterURL)
        {
            ServerRequestURL = filterURL;
        }

        public string ServerRequestURL { get; set; }
    }
}