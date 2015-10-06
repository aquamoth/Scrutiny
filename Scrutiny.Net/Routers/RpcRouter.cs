using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Routers
{
	class RpcRouter : IRouter
	{
		public async Task<string> Route(ControllerActionParts parts, System.Collections.Specialized.NameValueCollection parameters)
		{
			var controller = new Scrutiny.Controllers.RpcController();
			switch (parts.Action.ToLower())
			{
				case "poll":
					var id = parameters.Get("id");
					return await controller.Poll(id);
				default:
					throw new NotSupportedException();
			}
		}
	}
}
