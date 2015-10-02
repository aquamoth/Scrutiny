using Scrutiny.Models;
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
		public async Task<string> Route(ControllerActionParts parts)
		{
			var context = System.Web.HttpContext.Current;
			var server = (ScrutinyServer)context.Cache.Get(Module.IOSERVER_CACHE_KEY);

			var form = context.Request.Unvalidated.Form;
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
					case "register": server.Register(id, RegisterModel.From(form)); break;
					case "start": server.Start(id, StartModel.From(form)); break;
					case "info": server.Info(id, InfoModel.From(form)); break;
					case "result": server.Result(id, ResultModel.From(form)); break;
					case "complete": server.Complete(id, CompleteModel.From(form)); break;
					case "error": server.Error(id, form); break;
					default:
						throw new ArgumentException(string.Format("'{0}' is not a valid command", message), "message");
				}
				return "{}";
			}
		}

		//private IEnumerable<KeyValuePair<string, string>> enumerate(NameValueCollection collection)
		//{
		//	for (int i = 0; i < collection.Count; i++)
		//	{
		//		if (!string.IsNullOrWhiteSpace(collection[i]))
		//		{
		//			var key = collection.Keys[i];
		//			var value = collection[i];
		//			yield return new KeyValuePair<string, string>(key, value);
		//		}
		//	}
		//}
	}
}
