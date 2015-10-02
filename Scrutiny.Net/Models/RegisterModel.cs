using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrutiny.Models
{
	class RegisterModel
	{
		public string name { get; set; }

		internal static RegisterModel From(System.Collections.Specialized.NameValueCollection form)
		{
			return new RegisterModel
			{
				name = form["args[name]"]
			};
		}
	}
}
