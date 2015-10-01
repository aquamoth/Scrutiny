﻿using Scrutiny.State;
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

		public async Task<string> Route(ControllerActionParts parts)
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
						var name = form["args[name]"];
						server.Register(id, name);
						return "{}";

					case "start":
						var total = int.Parse(form["args[total]"]);
						server.Start(id, total);
						return "{}";

					case "result":
						var model = new ResultModel
						{
							//TODO:
						};
						server.Result(id, model);
						return "{}";

					case "complete":
						server.Complete(id);
						return "{}";

					case "error":
						server.Error(id, form);
						return "{}";

					default:
						throw new ArgumentException("Not a valid command", "message");
				}
			}
		}
	}
}
