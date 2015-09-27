using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Controllers
{
	class ContextController : Controller
	{
		public string Index()
		{
			return View();
		}

		public string Debug()
		{
			return View("Debug");
		}
	}
}
