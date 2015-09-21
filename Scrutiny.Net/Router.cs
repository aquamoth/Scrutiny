using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Scrutiny
{
	class Router
	{
		internal string Route(string path, NameValueCollection parameters)
		{
#warning Execute() should return an ActionResult instead, like Mvc.Net does
			var parts = ControllerActionParts.FromPath(path);

			switch (parts.Controller.ToLowerInvariant())
			{
				case "home":
					return routeHome(parts, parameters);
				case "rpc":
					return routeRpc(parts, parameters);
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

		private string routeHome(ControllerActionParts parts, NameValueCollection parameters)
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

		private string routeRpc(ControllerActionParts parts, NameValueCollection parameters)
		{
			var controller = new Scrutiny.Controllers.RpcController();
			switch (parts.Action.ToLower())
			{
				case "register":
					return controller.Register();
				case "poll":
					//TODO: Verify and set SessionId
					//TODO: Consider setting a session cookie on first request if not already set or timed out on the server
					//var id = parts.Value[0];
					var id = parameters.Get("id");
					return controller.Poll(id);
				default:
					throw new NotSupportedException();
			}
		}

	}
}
