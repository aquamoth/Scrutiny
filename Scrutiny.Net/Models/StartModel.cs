using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Models
{
	class StartModel
	{
		public int total { get; set; }

		public static StartModel From(System.Collections.Specialized.NameValueCollection form)
		{
			return new StartModel { total = int.Parse(form["args[total]"]) };
		}
	}
}
