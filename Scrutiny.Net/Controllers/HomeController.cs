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
			return View();
		}

		internal string Debug()
		{
			var model = new Models.ContextModels.Index
			{
				PreTestFiles = new string[]{
					"/Scrutiny/Scripts/expect.js/index.js",
 					"/Scrutiny/Scripts/mocha/mocha.js",
					"/Scrutiny/Scripts/karma_mocha/lib/adapter.js",
					"/Scrutiny/Scripts/require.js"
				},
				TestFiles = new string[] { 
					"/Scrutiny/Context/Tests/_fail_fast_test.js", //TODO: Load files according to model
				},
				PostTestFiles = new string[] { 
					"/Scrutiny/Scripts/karma_commonjs/client/commonjs_bridge.js"
				}
			};

			//TODO: Run all plugins to modify the model

			return View("Debug", model);
		}
	}
}
