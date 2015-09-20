using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrutiny
{
	class Router
	{
		internal string Route(string path, System.Collections.Specialized.NameValueCollection nameValueCollection)
		{
#warning Execute() should return an ActionResult instead, like Mvc.Net does
			var parts = ControllerActionParts.FromPath(path);

			switch (parts.Controller)
			{
				case "Home":
					return routeHome(parts);
				default:
					throw new NotSupportedException();
			}
		}

		private string routeHome(ControllerActionParts parts)
		{
			var controller = new Controllers.HomeController();
			switch (parts.Action)
			{
				case "Index":
					return controller.Index();
				default:
					throw new NotSupportedException();
			}
		}

	}
}
