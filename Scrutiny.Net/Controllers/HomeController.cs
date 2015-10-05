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
			Context.Response.Write("<style>body {font-size:14px;font-family:arial} h1 {font-size:1.2em} h2 {font-size: 1.1em} .error{color:red} .description{font-weight:bold}</style>");

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
								var message = new StringBuilder();

								string template;
								template = "<p class='error'><span class='browser'>{0}</span> <span class='description'>{1}</span></p>\n";
								message.AppendFormat(template, client.Browser, item.description);

								if (item.log.Length > 0)
								{
									template = "<pre class='log'>{0}</pre>\n";
									foreach (var logRow in item.log)
									{
										message.AppendFormat(template, logRow);
									}
								}

								messageQueue.Enqueue(message.ToString());
							}
						}
					}
				};
			}

	
			var isStarted = server.Execute();
			if (!isStarted)
			{
				throw new ApplicationException("Failed to start! Is another test run already in progress?");
			}


			while (server.Clients.Any(x => x.IsRunRequested || !x.IsReady))
			{
				await Task.Delay(200);
				string message;
				while (messageQueue.TryDequeue(out message))
				{
					Context.Response.Write(message);
				}
			}


			//Context.Response.Write("<h2>Test results:</h2>");
			foreach (var client in server.Clients)
			{
				Context.Response.Write(string.Format("<p class='result'>{0}: Executed {1} of {2}", client.Browser, client.Results.Count, client.TotalCount));
				var failedCount = client.Results.Count(x => !x.success);
				if (failedCount > 0)
				{
					Context.Response.Write(string.Format(" <span class='error'>({0} FAILED)</span>", failedCount));
				}
				var testTime = client.Results.Sum(x => x.time) / 1000.0;
				var totalTime = client.TestsEndTime.Subtract(client.TestsStartTime).TotalSeconds;
				Context.Response.Write(string.Format(" ({0:N3} secs / {1:N3} secs)</p>", testTime, totalTime));
			}

			var totalFailed = server.Clients.SelectMany(c => c.Results).Where(r => !r.success).Count();
			var totalSucceeded = server.Clients.SelectMany(c => c.Results).Count() - totalFailed;
			var totalClass = totalFailed > 0 ? "error" : "";
			Context.Response.Write(string.Format("<p class='{0}'>TOTAL: {1} FAILED, {2} SUCCESS</p>", totalClass, totalFailed, totalSucceeded));

			return "";
		}
	}
}
