using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace Scrutiny.Controllers
{
	public class RpcController : Controller
	{
		public string Poll(string id)
		{
			var cacheKey = "WebIO.Net.Server";
			var server = (WebIO.Net.IOServer)Context.Cache.Get(cacheKey);
			return server.Poll(id);
		}
	}
}
