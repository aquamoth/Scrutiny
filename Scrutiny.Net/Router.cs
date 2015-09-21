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

			switch (parts.Controller.ToLowerInvariant())
			{
				case "home":
					return routeHome(parts);
				case "signalr":
					return routeSignalR(parts);
				default:
					return File(path);
			}
		}

		private string File(string path)
		{
			var resourceName = string.Format("Scrutiny.{0}", path.Replace("/", "."));
			var content = Resources.GetString(resourceName);
			return content;
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

		private string routeSignalR(ControllerActionParts parts)
		{
			switch (parts.Action.ToLower())
			{
				case "hubs":
					throw new NotImplementedException();
				default:
					throw new NotSupportedException();
			}
		}

	}
}
