﻿using Scrutiny.State;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Controllers
{
	class RunController : Controller
	{
		internal async Task<string> Index(int testRun)
        {
            if (testRun != 0)
            {
                throw new NotImplementedException("Test groups are not yet supported.");
            }

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
                client.Dumps.CollectionChanged += (s, e) => OnDumpsAdded(e, messageQueue, client);
                client.Results.CollectionChanged += (s, e) => OnResultsAdded(e, messageQueue, client);
            }

			var cfg = loadClientConfig();
            var isStarted = server.Execute(cfg);
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

        private static Models.ClientConfiguration loadClientConfig()
        {
            var cfg = new Models.ClientConfiguration
            {
                frameworks = Config.Scrutiny.Section.ClientConfig.Frameworks.Select(x => x.Name).ToArray(),
                preprocessors = Config.Scrutiny.Section.ClientConfig.Preprocessors.Select(x => x.Name).ToArray(),
                reporters = Config.Scrutiny.Section.ClientConfig.Reporters.Select(x => x.Name).ToArray(),
                captureConsole = Config.Scrutiny.Section.ClientConfig.CaptureConsole
            };
            return cfg;
        }

        private static void OnDumpsAdded(System.Collections.Specialized.NotifyCollectionChangedEventArgs e, ConcurrentQueue<string> messageQueue, ScrutinyTestClient client)
        {
            if (e.NewItems == null)
                return;

            foreach (string dump in e.NewItems)
            {
                OnDumpAdded(messageQueue, client, dump);
            }
        }

        private static void OnDumpAdded(ConcurrentQueue<string> messageQueue, ScrutinyTestClient client, string dump)
        {
            messageQueue.Enqueue($"<p class='info'><span class='browser'>{client.Browser}</span> <span class='description'>{dump}</span></p>\n");
        }

        private static void OnResultsAdded(System.Collections.Specialized.NotifyCollectionChangedEventArgs e, ConcurrentQueue<string> messageQueue, ScrutinyTestClient client)
        {
            if (e.NewItems == null)
                return;

            foreach (TestResult item in e.NewItems)
            {
                OnResultAdded(messageQueue, client, item);
            }
        }

        private static void OnResultAdded(ConcurrentQueue<string> messageQueue, ScrutinyTestClient client, TestResult item)
        {
            if (item.success)
                return; //Suppressing all success messages

            var message = new StringBuilder();

            message.AppendLine($"<p class='error'><span class='browser'>{client.Browser}</span> <span class='description'>{item.description}</span></p>");

            foreach (var logRow in item.log)
            {
                message.AppendLine($"<pre class='log'>{logRow}</pre>");
            }

            messageQueue.Enqueue(message.ToString());
        }
    }
}
