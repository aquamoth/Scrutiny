using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcKarmaDemo.Tests.WebApi
{
    [Scrutiny.Net.Api.ControllerName("Test")]
    public class ApiTestController : Scrutiny.Net.Api.ApiController
    {
        public object GET(string id)
        {
            return new { id = id };
        }
    }
}
