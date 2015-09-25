using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web;

namespace Scrutiny
{
	public class Module : AsyncHttpModule
    {
		readonly Scrutiny.Routers.Router _router;
		readonly string _moduleUrl;

		public Module()
		{
			_moduleUrl = ConfigurationManager.AppSettings["Scrutiny:Url"] ?? "/Scrutiny";
			_router = new Routers.Router();
			registerRoutes(_router);
			registerIOServer();
		}

		protected override async Task DoAsyncWork(HttpContext context)
		{
			if (context.Request.Path.StartsWith(_moduleUrl, StringComparison.OrdinalIgnoreCase))
			{
				var url = urlFrom(context.Request.Path);
				var result = await _router.Route(url, context.Request.Params);
				if (result == null)
					throw new NotSupportedException("Scrutiny.Net does not support the requested path: " + context.Request.Path);

				writeTo(context.Response, result);
			}
		}

		private static void writeTo(System.Web.HttpResponse response, string result)
		{
			response.Write(result);
			response.End();
		}

		private void registerRoutes(Scrutiny.Routers.Router router)
		{
			router.Register<Routers.HomeRouter>("home");
			router.Register<Routers.RpcRouter>("rpc");
		}

		private void registerIOServer()
		{
			//TODO: Abstract the IOServer with events into its own stateful class
			var server = new WebIO.Net.IOServer();
			server.ClientConnected += server_ClientConnected;

			var cacheKey = "WebIO.Net.Server";
			System.Web.HttpContext.Current.Cache.Add(cacheKey, server,
				null,
				Cache.NoAbsoluteExpiration,
				Cache.NoSlidingExpiration,
				CacheItemPriority.NotRemovable,
				registerIOServer_CacheItemRemoved);
		}

		void server_ClientConnected(object sender, WebIO.Net.ClientConnectedEventArgs e)
		{
			//This is business logic and should be moved elsewhere
			var server = sender as WebIO.Net.IOServer;
			e.Client.Browser = System.Web.HttpContext.Current.Request.Browser.Browser;

			var browsers = server.Clients.Select(c => c.Browser).ToArray();
			server.SendToAll("Clients", browsers);
			//TODO: Consider setting a session cookie on first request if not already set or timed out on the server
		}

		private void registerIOServer_CacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
		{
#warning Do we need to handle IO Server bunked from cache?
			throw new NotImplementedException("The entire IO Server was bunked from cache!");
		}




		private string urlFrom(string path)
		{
			var url = path.Substring(_moduleUrl.Length);
			if (url.StartsWith("/"))
			{
				url = url.Substring(1);
			}
			return url;
		}
	}
}
