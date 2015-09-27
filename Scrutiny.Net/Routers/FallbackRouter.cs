using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Routers
{
	class FallbackRouter : IRouter
	{
		public async Task<string> Route(ControllerActionParts parts, System.Collections.Specialized.NameValueCollection parameters)
		{
			var path = parts.OriginalPath;
			path = path.Replace("/", ".");
			//path = path.Replace("-", "_");
			var resourceName = string.Format("Scrutiny.{0}", path);
			var content = await Resources.GetStringAsync(resourceName);
			return content;
		}
	}
}
