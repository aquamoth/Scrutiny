﻿using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrutiny.Controllers
{
	public class Controller
	{
		public Controller()
		{
			ViewBag = new Dictionary<string, object>();
		}

		public string Name
		{
			get
			{
				var controllerName = this.GetType().Name;
				if (controllerName.EndsWith("Controller"))
					controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);
				return controllerName;
			}
		}

		protected IDictionary<string, object> ViewBag { get; private set; }

		#region View

		protected string View()
		{
			//TODO: Should automatically determine View name from callers function-name
			return View("Index", null);
		}

		protected string View(object model)
		{
			//TODO: Should automatically determine View name from callers function-name
			return View("Index", model);
		}

		protected string View(string viewName)
		{
			return View(viewName, null);
		}

		protected string View(string viewName, object model)
		{
			var resourceName = string.Format("Scrutiny.Views.{0}.{1}.cshtml", this.Name, viewName);
			var template = Resources.GetString(resourceName);
			var viewBag = new RazorEngine.Templating.DynamicViewBag(this.ViewBag);
			var result = RazorEngine.Engine.Razor.RunCompile(template, resourceName, null, model, viewBag);
			return result;
		}

		#endregion View

		#region Global Web

		public System.Web.HttpContext Context { get { return System.Web.HttpContext.Current; } }
		//public System.Web.HttpRequest Request { get { return System.Web.HttpContext.Current.Request; } }
		//public System.Web.HttpResponse Response { get { return System.Web.HttpContext.Current.Response; } }

		#endregion
	
	}
}
