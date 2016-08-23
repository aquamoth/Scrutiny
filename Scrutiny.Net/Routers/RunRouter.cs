using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Routers
{
	class RunRouter : IRouter
	{
		public async Task<string> Route(ControllerActionParts parts, Net.Api.RequestType requestType)
		{
			var controller = new Controllers.RunController();
			switch (parts.Action)
			{
				case "index":
					//TODO: Rewrite as async?
					var value = parts.Value.SingleOrDefault();
					var testRun = string.IsNullOrEmpty(value)
						? 0
						: int.Parse(value);
					return await controller.Index(testRun);

				default:
					throw new NotSupportedException();
			}
		}
	}
}
