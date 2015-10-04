using Scrutiny.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebIO.Net;

namespace Scrutiny.State
{
	class ScrutinyServer : WebIO.Net.IOServer
	{
		protected override void OnClientDisconnected(WebIO.Net.ClientDisconnectedEventArgs e)
		{
			base.OnClientDisconnected(e);
			broadcastClientsList();
		}

		#region Client emits

		public void Register(string id, RegisterModel model)
		{
			var client = FindClient(id);
			if (!string.IsNullOrEmpty(client.Browser))
				throw new ApplicationException("Client tried to register itself multiple times.");
			if (string.IsNullOrWhiteSpace(model.name))
				throw new ArgumentException("Client tried to register as an undefined browser.", "name");

			client.Browser = model.name;
			client.IsReady = true;
	
			broadcastClientsList();
		}

		internal void Start(string id, StartModel model)
		{
			var client = FindClient(id);
			client.IsReady = false;
			client.IsRunRequested = false;
			client.TotalCount = model.total;

			broadcastClientsList();
		}

		internal void Info(string id, InfoModel model)
		{
#warning ScrutinyServer.Info() not implemented correctly
		}

		internal void Result(string id, ResultModel model)
		{
			var client = FindClient(id);
			foreach (var item in model.Items)
			{
				client.Results.Add(item);
			}
		}

		internal void Complete(string id, CompleteModel model)
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

		#endregion Client emits

		#region Server commands

		internal bool Execute(/* cfg? */)
		{
			if (this.Clients.Any(x => x.IsRunRequested || !x.IsReady))
				return false;

			var cfg = new
			{
				frameworks = new[] { "mocha", "commonjs", "expect" },
				preprocessors = new string[] { },
				reporters = new[] { "dots" },
			};

			foreach (var client in this.Clients)
			{
				client.IsRunRequested = true;
				client.Results.Clear();
			}
			this.SendToAll("execute", cfg);

			return true;
		}

		#endregion Server commands



		public new IEnumerable<ScrutinyTestClient> Clients
		{
			get
			{
				return base.Clients.Cast<ScrutinyTestClient>();
			}
		}
		private new ScrutinyTestClient FindClient(string id)
		{
			return base.FindClient(id) as ScrutinyTestClient;
		}
		
		protected override Client CreateClient()
		{
			var client = new ScrutinyTestClient();
			return client;
		}

		private void broadcastClientsList()
		{
			var browsers = this.Clients
				.Select(c => new RegisterResponse { isReady = true, name = c.Browser })
				.ToArray();
			this.SendToAll("info", browsers);
		}
	}


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
}
