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
					var value = parts.Value.SingleOrDefault();
					var testRun = string.IsNullOrEmpty(value) 
						? 0 
						: int.Parse(value);
					return controller.Index(testRun);

				case "tests":
					return await controller.Tests(parts.Value);
	
				default:
					throw new NotSupportedException();
			}
		}
	}
}
