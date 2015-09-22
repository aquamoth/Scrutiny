using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

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
