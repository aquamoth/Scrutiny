using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace Scrutiny
{
	public class Module : AsyncHttpModule
    {
		public const string IOSERVER_CACHE_KEY = "WebIO.Net.Server";

		readonly Scrutiny.Routers.Router _router;
		readonly string _moduleUrl;

		public Module()
		{
			_moduleUrl = ConfigurationManager.AppSettings["Scrutiny:Url"] ?? "/Scrutiny";
			_router = new Routers.Router();
			registerRoutes(_router);
			registerIOServer();
		}

		protected override async Task OnBeginRequestAsync(HttpContext context)
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
			var server = new State.ScrutinyServer();
			System.Web.HttpContext.Current.Cache.Add(IOSERVER_CACHE_KEY, server,
				null,
				Cache.NoAbsoluteExpiration,
				Cache.NoSlidingExpiration,
				CacheItemPriority.NotRemovable,
				registerIOServer_CacheItemRemoved);
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
