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
				case "Index":
					//TODO: Rewrite as async?
					return controller.Index();

				case "Run":
					return await controller.Run();

				default:
					throw new NotSupportedException();
			}
		}
	}
}
