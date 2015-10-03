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

			Context.Response.Buffer = false;
			Context.Response.Write(string.Format("<h1>[{0:yyyy-MM-dd HH:mm:ss.fffff}] [DEBUG] config - <small>Loading config (hardcoded for now...)</small></h1>", DateTime.Now));

			if (!server.Clients.Any())
			{
				throw new ApplicationException("There are no connected clients!");
			}

			var isStarted = server.Execute();
			if (!isStarted)
			{
				throw new ApplicationException("Failed to start! Is another test run already in progress?");
			}



			//Context.Response.Write("<p>");
			while (server.Clients.Any(x => x.IsRunRequested || !x.IsReady))
			{
				//Context.Response.Write(".");
				await Task.Delay(200);

	//server.Clients.Select(x=>x.)
			
			
			}
			//Context.Response.Write("</p>");



			Context.Response.Write("<p>Test results:</p>");



			//Context.ApplicationInstance.CompleteRequest();
			return "";
		}
	}
}
