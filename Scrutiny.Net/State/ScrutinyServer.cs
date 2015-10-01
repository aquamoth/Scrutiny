using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.State
{
	//class ts : System.Web.ModelBinding.IModelBinder
	//{

	//	public bool BindModel(System.Web.ModelBinding.ModelBindingExecutionContext modelBindingExecutionContext, System.Web.ModelBinding.ModelBindingContext bindingContext)
	//	{
	//		bindingContext.ModelMetadata.
	//		return true;
	//	}
	//}
//SAMPLE USAGE:
			//var modelState = new System.Web.ModelBinding.ModelStateDictionary();
			//var contextBase = new System.Web.HttpContextWrapper(System.Web.HttpContext.Current);
			//var bindingExecutionContext = new System.Web.ModelBinding.ModelBindingExecutionContext(contextBase, modelState);
			//var bindingContext = new System.Web.ModelBinding.ModelBindingContext();
			//var binder = new ts();
			//var success = binder.BindModel(bindingExecutionContext, bindingContext);
			//var model = bindingContext.Model;



	class RegisterModel
	{
		public string Browser { get; set; }
	}

	class ScrutinyServer : WebIO.Net.IOServer
	{
		protected override void OnClientConnected(WebIO.Net.ClientConnectedEventArgs e)
		{
			base.OnClientConnected(e);

		}



		protected override void OnClientDisconnected(WebIO.Net.ClientDisconnectedEventArgs e)
		{
			base.OnClientDisconnected(e);
			broadcastClientsList();
		}

		public void Register(string id, string name)
		{
			var client = FindClient(id);
			if (!string.IsNullOrEmpty(client.Browser))
				throw new ApplicationException("Client tried to register itself multiple times.");

			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Client tried to register as an undefined browser.", "name");

			client.Browser = name;
			client.IsReady = true;
	
			broadcastClientsList();




#warning Just testing code! Tests should be started from external server-calls
			var cfg = new {
				frameworks = new[] { "mocha", "commonjs", "expect" },
				preprocessors = new string[] { },
				reporters = new[] { "dots"},
			};
			this.SendTo(client, "execute", cfg);
		}

		internal void Start(string id, int total)
		{
			var client = FindClient(id);
			client.IsReady = false;
			client.TotalCount = total;
			broadcastClientsList();
		}

		internal void Result(string id, ResultModel model)
		{
#warning ScrutinyServer.Result() not implemented correctly
		}

		internal void Complete(string id)
		{
			var client = FindClient(id);
			client.IsReady = true;

#warning ScrutinyServer.Complete() not implemented correctly
			broadcastClientsList();
		}

		internal void Error(string id, NameValueCollection arguments)
		{
			var client = FindClient(id);
			client.IsReady = true;

#warning ScrutinyServer.Error() not implemented correctly
			System.Diagnostics.Trace.TraceError("Client error: " + arguments["args"]);

			broadcastClientsList();
		}


		private void broadcastClientsList()
		{
			var browsers = this.Clients
				.Select(c => new RegisterResponse { isReady = true, name = c.Browser })
				.ToArray();
			this.SendToAll("info", browsers);
		}
	}

	class RegisterResponse
	{
		public bool isReady { get; set; }
		public string name { get; set; }
	}
}
