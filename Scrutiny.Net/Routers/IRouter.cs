using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Routers
{
	public interface IRouter
	{
		string Route(ControllerActionParts parts, NameValueCollection parameters);
	}
}
