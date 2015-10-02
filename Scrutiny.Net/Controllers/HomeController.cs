using Scrutiny.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Controllers
{
	class HomeController : Controller
	{
		internal string Index()
		{
			return View("Index");
		}

		internal async Task<string> Run()
		{
			var server = (ScrutinyServer)Context.Cache.Get(Module.IOSERVER_CACHE_KEY);

			//TODO: Check if Run() is already in progress before starting another thread!

			Context.Response.Buffer = false;

			Context.Response.Write("<p>Starting testing</p>");

			foreach (var client in server.Clients)
				client.IsRunRequested = true;

			var cfg = new
			{
				frameworks = new[] { "mocha", "commonjs", "expect" },
				preprocessors = new string[] { },
				reporters = new[] { "dots" },
			};
			server.SendToAll("execute", cfg);

			Context.Response.Write("<p>");
			while (server.Clients.Any(x => x.IsRunRequested || !x.IsReady))
			{
				Context.Response.Write(".");
				await Task.Delay(200);
			}
			Context.Response.Write("</p>");
			Context.Response.Write("<p>Test results:</p>");



			//Context.ApplicationInstance.CompleteRequest();
			return "";
		}
	}
}
