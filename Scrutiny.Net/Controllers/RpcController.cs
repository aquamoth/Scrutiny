using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Controllers
{
	public class RpcController : Controller
	{
		public string Register()
		{
			return Guid.NewGuid().ToString().Replace("-", "");
		}

		public string Poll(string id)
		{
			//System.Web.HttpContext.Current.Server.ScriptTimeout = 5;
			System.Threading.Thread.Sleep(new TimeSpan(0, 0, 30));
			//throw new ApplicationException("Polling not fully implemented!");
			return "Poll returned.";
		}
	}
}
