using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.ComponentModel;
using System.Web;

namespace Scrutiny.Net.Test
{
	[TestClass]
	public class ModuleTests : IDisposable
	{
		readonly HttpApplication application;
		Scrutiny.Module module;

		public ModuleTests()
		{
			application = new HttpApplication();
			module = new Scrutiny.Module();
			module.Init(application);
		}

		public void Dispose()
		{
			module.Dispose();
		}

		[TestMethod]
		public void Module_is_an_HttpModule()
		{
			Assert.IsInstanceOfType(module, typeof(System.Web.IHttpModule));
		}

		[TestMethod]
		public void Module_listens_to_BeginRequest()
		{
			var request = new HttpRequest("", "http://localhost/Test/", "");
			var responseWriter = new StringWriter();
			var response = new HttpResponse(responseWriter);
			var context = new HttpContext(request, response);
			HttpContext.Current = context;

			FireHttpApplication_BeginRequest();


			var x = 0;
		}

		//[TestMethod]
		//public void Module_listens_to_EndRequest()
		//{
		//	FireHttpApplication_EndRequest();
		//}

		//[TestMethod]
		//public void Module_initializes_and_disposes()
		//{
		//	//var request = new System.Web.HttpRequest("", "", "");
		//	//var responseWriter = new StringWriter();
		//	//var response = new System.Web.HttpResponse(responseWriter);
		//	//var context = new System.Web.HttpContext(request, response);

		//	var application = new System.Web.HttpApplication();
		//	var module = new Scrutiny.Module();
		//	module.Init(application);

	
		//	module.Dispose();
		//}







		private void FireHttpApplication_BeginRequest()
		{
			FireHttpApplicationEvent(application, "EventBeginRequest", this, EventArgs.Empty);
		}

		private void FireHttpApplication_EndRequest()
		{
			FireHttpApplicationEvent(application, "EventEndRequest", this, EventArgs.Empty);
		}

		private static void FireHttpApplicationEvent(object onMe, string invokeMe, params object[] args)
		{
			var objectType = onMe.GetType();

			object eventIndex = (object)objectType.GetField(invokeMe, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(onMe);
			EventHandlerList events = (EventHandlerList)objectType.GetField("_events", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(onMe);

			EventHandler handler = (EventHandler)events[eventIndex];

			Delegate[] delegates = handler.GetInvocationList();

			foreach (Delegate dlg in delegates)
			{
				dlg.Method.Invoke(dlg.Target, args);
			}
		}
	}

}
