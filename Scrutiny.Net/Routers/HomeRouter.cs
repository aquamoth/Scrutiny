using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Routers
{
	class HomeRouter : IRouter
	{
		public async Task<string> Route(ControllerActionParts parts)
		{
			var controller = new Controllers.HomeController();
			switch (parts.Action)
			{
				case "index":
					//TODO: Rewrite as async?
					return controller.Index();

				case "debug":
					//TODO: Rewrite as async?
					return controller.Debug();

				default:
					throw new NotSupportedException();
			}
		}
	}
}
