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
		public const string TEST_FILES_CACHE_KEY = "Scrutiny.Test-files";

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
				try
				{
					var url = urlFrom(context.Request.Path);
					var result = await _router.Route(url);
					if (result == null)
						throw new NotSupportedException("Scrutiny.Net does not support the requested path: " + context.Request.Path);
					endResponse(context.Response, result);
				}
				catch (Exception ex)
				{
					endResponse(context.Response, ex.ToString(), System.Net.HttpStatusCode.InternalServerError);
				}
			}
		}

		private static void endResponse(System.Web.HttpResponse response, string responseText, System.Net.HttpStatusCode statusCode = System.Net.HttpStatusCode.OK)
		{
#warning Unbuffered responses can probably set status code if NO content has been sent yet?!
			if (response.Buffer)
				response.StatusCode = (int)statusCode;
			response.Write(responseText);
			response.End();
		}

		private void registerRoutes(Scrutiny.Routers.Router router)
		{
			router.Register<Routers.HomeRouter>("home");
			router.Register<Routers.ContextRouter>("context");
			router.Register<Routers.RunRouter>("run");
			router.Register<Routers.SocketIORouter>("socket.io");
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
