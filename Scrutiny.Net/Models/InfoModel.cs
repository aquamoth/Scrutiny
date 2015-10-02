using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Models
{
	class InfoModel
	{
		public string Log { get; set; }
		public string type { get; set; }

		public static InfoModel From(System.Collections.Specialized.NameValueCollection form)
		{
			return new InfoModel
			{
				Log = form["args[Log]"],
				type = form["args[type]"]
			};
		}
	}
}
