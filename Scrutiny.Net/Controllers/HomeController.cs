using Scrutiny.State;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Controllers
{
	class HomeController : Controller
	{
		internal string Index()
		{
			ViewBag.Add("RootUrl", Scrutiny.Config.Scrutiny.Section.Url);
			return View();
		}

		/// <summary>
		/// This action is virtually the same as ContextController/Index
		/// </summary>
		/// <returns></returns>
		internal string Debug(int testRun)
		{
			try
			{
				ViewBag.Add("RootUrl", Scrutiny.Config.Scrutiny.Section.Url);

				var paths = Config.Scrutiny.PathsForTestrun(testRun);
				var model = new Models.ContextModels.Index
				{
					Scripts = Filesystem.ExpandMinimatchUrls(paths)
									.Select(x => string.Format("{0}/{1}", testRun, x))
									.ToArray()
				};

				return View("Debug", model);
			}
			catch (Exception ex)
			{
#warning Write exception about dir not found to user
				throw;
			}
		}
	}
}
