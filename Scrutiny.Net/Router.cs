using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
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
			var resolver = Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver;
			switch (parts.Action.ToLower())
			{
				case "hubs":
					var _path = ConfigurationManager.AppSettings["Scrutiny:Url"] ?? "/Scrutiny";
					var proxyGenerator = new Microsoft.AspNet.SignalR.Hubs.DefaultJavaScriptProxyGenerator(resolver);
					return proxyGenerator.GenerateProxy(_path + "/signalr", true);

				case "negotiate":
					//new Microsoft.AspNet.SignalR.Infrastructure.ProtocolResolver().Resolve()
					//HubConfiguration configuration = null;
					//var dispatcher = new Microsoft.AspNet.SignalR.Hubs.HubDispatcher(configuration);
					//var context = new Microsoft.AspNet.SignalR.Hosting.HostContext(request, response);
					//dispatcher.ProcessRequest(context).Wait();
					throw new NotImplementedException();

				default:
					throw new NotSupportedException();
			}
		}

	}
}
