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
			return View("Index");
		}
	}
}
