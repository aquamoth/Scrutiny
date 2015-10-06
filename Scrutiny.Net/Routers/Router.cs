using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Routers
{
	public class Router
	{
		private Dictionary<string, IRouter> routers = new Dictionary<string, IRouter>(StringComparer.OrdinalIgnoreCase);

		public void Register<T>(string controllerName)
			where T : IRouter, new()
		{
			routers.Add(controllerName, new T());
		}

		public virtual async Task<string> Route(string url, NameValueCollection parameters)
		{
#warning Execute() should return an ActionResult instead, like Mvc.Net does
			var parts = ControllerActionParts.FromPath(url);
			var router = selectRouter(parts.Controller);
			return await router.Route(parts, parameters);
		}

		private IRouter selectRouter(string controllerName)
		{
			if (routers.ContainsKey(controllerName))
				return routers[controllerName];
			return new FallbackRouter();
		}

		//public static Router Default
		//{
		//	get
		//	{
		//		if (_default == null)
		//			_default = new Router();
		//		return _default;
		//	}
		//	set { _default = value; }
		//}
		//static Router _default = null;
	}
}
