using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Routers
{
	class RpcRouter : IRouter
	{
		public string Route(ControllerActionParts parts, System.Collections.Specialized.NameValueCollection parameters)
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
