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
		readonly string _path;

		public Module()
		{
			_path = ConfigurationManager.AppSettings["Scrutiny:Url"] ?? "/Scrutiny";
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
			if (context.Request.Path.StartsWith(_path))
			{
				var action = context.Request.Path.Substring(_path.Length);
				if (action.StartsWith("/"))
				{
					action = action.Substring(1);
				}
				var router = new Router();
				var result = router.Route(action, context.Request.Params);
				if (result != null)
				{
					context.Response.Write(result);
					context.Response.End();
				}
			}
		}
	}
}
