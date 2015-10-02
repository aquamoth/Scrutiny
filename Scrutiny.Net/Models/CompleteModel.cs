using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrutiny.Models
{
	class CompleteModel
	{
		public object coverage { get; set; }

		public static CompleteModel From(System.Collections.Specialized.NameValueCollection form)
		{
			return new CompleteModel { coverage = form["args[coverage]"] };
		}
	}
}
