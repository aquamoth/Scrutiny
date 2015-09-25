using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Routers
{
	class HomeRouter : IRouter
	{
		public async Task<string> Route(ControllerActionParts parts, System.Collections.Specialized.NameValueCollection parameters)
		{
			var controller = new Controllers.HomeController();
			switch (parts.Action)
			{
				case "Index":
					//TODO: Write dummy await here?!
					return controller.Index();
				default:
					throw new NotSupportedException();
			}
		}
	}
}
