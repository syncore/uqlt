﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Models.Filters.User
{
    public class Arena
    {
        public string display_name { get; set; }
        public string arena_type { get; set; }
        public string arena { get; set; }

        public override string ToString()
        {
            return display_name;
        }
    }
}