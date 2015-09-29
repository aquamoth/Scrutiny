using Scrutiny.State;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Routers
{
	class SocketIORouter : IRouter
	{
		private IEnumerable<KeyValuePair<string,string>> enumerate(NameValueCollection collection)
		{
			for (int i = 0; i < collection.Count; i++)
			{
				if (!string.IsNullOrWhiteSpace(collection[i]))
				{
					var key = collection.Keys[i];
					var value = collection[i];
					yield return new KeyValuePair<string, string>(key, value);
				}
			}
		}

		public async Task<string> Route(ControllerActionParts parts, System.Collections.Specialized.NameValueCollection parameters)
		{
			var context = System.Web.HttpContext.Current;
			var server = (ScrutinyServer)context.Cache.Get(Module.IOSERVER_CACHE_KEY);

			var form = context.Request.Form;
			var id = form.Get("id");
			var message = form.Get("message");

			if (string.IsNullOrEmpty(id))
			{
				//Connecting
				if (!string.IsNullOrEmpty(message))
					throw new ArgumentException("Establish a connection first before emitting commands!");
				//TODO: Verify formArgs are empty?
				return server.Connect();
			}
			else if (string.IsNullOrEmpty(message))
			{
				//Polling thread
				//TODO: Verify formArgs are empty?
				return await server.Poll(id);
			}
			else
			{
				//Emitting command
				switch (message)
				{
					case "register":
						server.Register(id, form);
						return "{}";

					case "error":
#warning Client sending error has not been implemented properly
						System.Diagnostics.Trace.TraceError("Client error: " + form["args"]);
						return "{}";

					case "complete":
#warning Client sending complete has not been implemented properly
						System.Diagnostics.Trace.TraceError("Client complete: Not implemented");
						return "{}";

					default:
						throw new ArgumentException("Not a valid command", "message");
				}
			}
		}
	}
}
