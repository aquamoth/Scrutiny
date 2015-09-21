using RazorEngine.Templating;
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
			throw new NotImplementedException();
		}

		protected string View(object model)
		{
			throw new NotImplementedException();
		}

		protected string View(string viewName)
		{
			return View(viewName, null);
		}

		protected string View(string viewName, object model)
		{
			var resourceName = string.Format("Scrutiny.Views.{0}.{1}.cshtml", this.Name, viewName);
			var template = Resources.GetString(resourceName);

			var cacheName = string.Format("{0}/{1}", this.Name, viewName);
			var viewBag = new RazorEngine.Templating.DynamicViewBag(this.ViewBag);
			//var modelType = model == null ? null : model.GetType();
			var templatingService = new RazorEngine.Templating.TemplateService();
			var result = templatingService.Parse(template, model, viewBag, cacheName);
			return result;
		}

		#endregion View

	}
}
