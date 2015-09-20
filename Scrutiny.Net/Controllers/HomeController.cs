using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrutiny.Controllers
{
	class HomeController : Controller
	{
		internal string Index()
		{
			return View("Index");
		}
	}
}
