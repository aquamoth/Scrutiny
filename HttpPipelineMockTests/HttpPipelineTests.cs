using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HttpPipelineMock;

namespace HttpPipelineMockTests
{
	[TestClass]
	public class HttpPipelineTests
	{
		[TestMethod]
		public void Pipeline_instanciates()
		{
			var pipeline = new HttpPipeline();
			//pipeline.HttpApplication.
			pipeline.Dispose();
		}
	}
}
