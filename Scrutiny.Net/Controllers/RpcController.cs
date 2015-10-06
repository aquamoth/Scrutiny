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
		public async Task<string> Poll(string id)
		{
			var server = (WebIO.Net.IOServer)Context.Cache.Get(Module.IOSERVER_CACHE_KEY);
			return await server.Poll(id);
		}
	}
}
