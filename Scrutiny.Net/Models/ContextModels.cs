﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Models
{
	public class ContextModels
	{
		public class Index
		{
            public IEnumerable<string> Stylesheets { get; set; }
            public IEnumerable<string> Scripts { get; set; }
        }
    }
}
