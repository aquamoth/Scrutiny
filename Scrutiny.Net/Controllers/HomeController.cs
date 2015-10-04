using Scrutiny.State;
using System;
using System.Collections.Concurrent;
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
			Context.Response.Write("<style>body {font-size:14px;font-family:arial} h1 {font-size:1.2em} h2 {font-size: 1.1em} .browser,.description{color:red} .description{font-weight:bold} .error{}</style>");

			Context.Response.Write(string.Format("<h1>[{0:yyyy-MM-dd HH:mm:ss.fffff}] [DEBUG] config - <small>Loading config (hardcoded for now...)</small></h1>", DateTime.Now));

			if (!server.Clients.Any())
			{
				throw new ApplicationException("There are no connected clients!");
			}


			var messageQueue = new ConcurrentQueue<string>();
			foreach (var client in server.Clients)
			{
				var collection = client.Results as System.Collections.ObjectModel.ObservableCollection<TestResult>;
				collection.CollectionChanged += (s, e) =>
				{
					if (e.NewItems != null)
					{
						foreach (TestResult item in e.NewItems)
						{
							if (!item.success)
							{
								string template;
								template = "<p><span class='browser'>{0}</span> <span class='description'>{1}</span></p>";
								messageQueue.Enqueue(string.Format(template, client.Browser, item.description));

								if (item.log.Length > 0)
								{
									template = "<pre class='error'>{0}</pre>";
									foreach (var logRow in item.log)
									{
										messageQueue.Enqueue(string.Format(template, logRow));
									}
								}
							}
						}
					}
				};
			}

	
			try
			{
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
					string message;
					while (messageQueue.TryDequeue(out message))
					{
						Context.Response.Write(message);
					}
				}
				//Context.Response.Write("</p>");
			}
			finally
			{
				//foreach (var client in server.Clients)
				//{
				//	var collection = client.Results as System.Collections.ObjectModel.ObservableCollection<TestResult>;
				//	collection.CollectionChanged -= collection_CollectionChanged;
				//}
			}



			Context.Response.Write("<h2>Test results:</h2>");



			//Context.ApplicationInstance.CompleteRequest();
			return "";
		}
	}
}
