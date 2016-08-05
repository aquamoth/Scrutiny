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

		public void Register(string id, SocketIORouterModels.RegisterModel model)
		{
			var client = FindClient(id);
			if (!string.IsNullOrEmpty(client.Browser))
				throw new ApplicationException("Client tried to register itself multiple times.");
			if (string.IsNullOrWhiteSpace(model.name))
				throw new ArgumentException("Client tried to register as an undefined browser.", "name");

			var request = System.Web.HttpContext.Current.Request;
			client.Browser = string.Format("{0} {1} ({2})", request.Browser.Browser, request.Browser.Version, request.Browser.Platform);
			client.IsReady = true;
	
			broadcastClientsList();
		}

		internal void Start(string id, SocketIORouterModels.StartModel model)
		{
			var client = FindClient(id);
			client.IsReady = false;
			client.IsRunRequested = false;
			client.TotalCount = model.total;
			client.TestsStartTime = DateTime.Now;
			client.TestsEndTime = DateTime.MaxValue;

			broadcastClientsList();
		}

		internal void Info(string id, SocketIORouterModels.InfoModel model)
		{
            var client = FindClient(id);
            client.Dumps.Add(model.Log);
        }

        internal void Result(string id, SocketIORouterModels.ResultModel model)
		{
			var client = FindClient(id);
			foreach (var item in model.Items)
			{
				client.Results.Add(item);
			}
		}

		internal void Complete(string id, SocketIORouterModels.CompleteModel model)
		{
			var client = FindClient(id);

#warning ScrutinyServer.Complete() not implemented correctly
            System.Diagnostics.Trace.TraceWarning("Complete: Coverage " + model.coverage);

            client.TestsEndTime = DateTime.Now;
			client.IsReady = true;

            broadcastClientsList();
		}

		internal void Error(string id, NameValueCollection arguments)
		{
			var client = FindClient(id);

#warning ScrutinyServer.Error() not implemented correctly
            client.Dumps.Add($"{arguments["message"]}: {arguments["args"]}");

            client.TestsEndTime = DateTime.Now;
            client.IsReady = true;

			broadcastClientsList();
		}

		#endregion Client emits

		#region Server commands


		internal bool Execute(ClientConfiguration cfg)
		{
			if (this.Clients.Any(x => x.IsRunRequested || !x.IsReady))
				return false;

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
				.Select(c => new SocketIORouterModels.RegisterResponse { isReady = true, name = c.Browser })
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
