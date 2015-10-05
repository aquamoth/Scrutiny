using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Routers
{
	class RunRouter : IRouter
	{
		public async Task<string> Route(ControllerActionParts parts)
		{
			var controller = new Controllers.RunController();
			switch (parts.Action)
			{
				case "index":
					return await controller.Index();

				default:
					throw new NotSupportedException();
			}
		}
	}
}
