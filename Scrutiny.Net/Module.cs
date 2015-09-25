﻿using System;
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
	public class Module : System.Web.IHttpModule
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

		public void Init(System.Web.HttpApplication context)
		{
			//See: http://brockallen.com/2013/07/27/implementing-async-http-modules-in-asp-net-using-tpls-task-api/
			context.AddOnBeginRequestAsync(onBegin, onEnd);
		}

		public void Dispose()
		{
		}

		IAsyncResult onBegin(object sender, EventArgs e, AsyncCallback cb, object extraData)
		{
			var tcs = new TaskCompletionSource<object>(extraData);
			//HttpContext.Current
			DoAsyncWork(sender as HttpContext).ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					tcs.SetException(t.Exception.InnerExceptions);
				}
				else
				{
					tcs.SetResult(null);
				}
				if (cb != null) cb(tcs.Task);
			});
			return tcs.Task;
		}

		void onEnd(IAsyncResult ar)
		{
			Task t = (Task)ar;
			t.Wait();
		}

		async Task DoAsyncWork(HttpContext sender)
		{
			var context = System.Web.HttpContext.Current;
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
