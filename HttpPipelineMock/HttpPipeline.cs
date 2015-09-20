using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HttpPipelineMock
{
    public class HttpPipeline : IDisposable
    {
		public HttpApplication HttpApplication { get; private set; }

		public HttpPipeline()
		{

		}

		public void Dispose()
		{
		}
	}
}
