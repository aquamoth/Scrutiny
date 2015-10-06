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
		internal string Debug(string id)
		{
			var model = new Models.ContextModels.Index
			{
				//PreTestFiles = Config.Config.Default.PreTestFiles,

				TestFiles = new string[] { 
					"/Scrutiny/Context/Tests/_fail_fast_test.js", //TODO: Load files according to model
				},

				//PostTestFiles = Config.Config.Default.PostTestFiles
			};

			//TODO: Run all plugins to modify the model
			
			//TODO: Implement just as in Context/Index

			return View("Debug", model);
		}
	}
}
