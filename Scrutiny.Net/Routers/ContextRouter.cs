using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Routers
{
	class ContextRouter : IRouter
	{
		public async Task<string> Route(ControllerActionParts parts)
		{
			var controller = new Controllers.ContextController();
			switch (parts.Action)
			{
				case "index":
					//TODO: Rewrite as async?
					return controller.Index();
				case "tests":
					return await controller.Tests(parts.Value.Single());
				default:
					throw new NotSupportedException();
			}
		}
	}
}
