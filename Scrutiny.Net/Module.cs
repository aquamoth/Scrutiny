﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Web.Caching;

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
			context.BeginRequest += context_BeginRequest;
		}

		public void Dispose()
		{
		}

		void context_BeginRequest(object sender, EventArgs e)
		{
			var context = System.Web.HttpContext.Current;
			if (context.Request.Path.StartsWith(_moduleUrl, StringComparison.OrdinalIgnoreCase))
			{
				var url = urlFrom(context.Request.Path);
				var result = _router.Route(url, context.Request.Params);
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

		void server_ClientConnected(object sender, EventArgs e)
		{
			//TODO: Verify and set SessionId
			//TODO: Consider setting a session cookie on first request if not already set or timed out on the server
			throw new NotImplementedException();
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
