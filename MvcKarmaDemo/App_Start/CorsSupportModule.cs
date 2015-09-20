using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcKarmaDemo.App_Start
{
	public class CorsSupportModule : IHttpModule
	{
		public void Init(HttpApplication context)
		{
			context.BeginRequest += context_BeginRequest;
			context.AuthenticateRequest += context_AuthenticateRequest;
			context.PostAuthenticateRequest += context_PostAuthenticateRequest;
			context.AuthorizeRequest += context_AuthorizeRequest;
			context.PostAuthorizeRequest += context_PostAuthorizeRequest;
			context.ResolveRequestCache += context_ResolveRequestCache;
			context.MapRequestHandler += context_MapRequestHandler;
			context.AcquireRequestState += context_AcquireRequestState;
			context.PostAcquireRequestState += context_PostAcquireRequestState;

			

			context.EndRequest += new EventHandler(context_EndRequest);
			context.Disposed += context_Disposed;
			context.Error += context_Error;
			context.LogRequest += context_LogRequest;
			context.PostLogRequest += context_PostLogRequest;
			context.PostMapRequestHandler += context_PostMapRequestHandler;
			context.PostReleaseRequestState += context_PostReleaseRequestState;
			context.PostRequestHandlerExecute += context_PostRequestHandlerExecute;
			context.PostResolveRequestCache += context_PostResolveRequestCache;
			context.PostUpdateRequestCache += context_PostUpdateRequestCache;
			context.PreRequestHandlerExecute += context_PreRequestHandlerExecute;
			context.PreSendRequestContent += context_PreSendRequestContent;
			context.PreSendRequestHeaders += context_PreSendRequestHeaders;
			context.ReleaseRequestState += context_ReleaseRequestState;
			context.RequestCompleted += context_RequestCompleted;
			context.UpdateRequestCache += context_UpdateRequestCache;
		}

		void context_BeginRequest(object sender, EventArgs e) { log(); sendResponse(sender); }

		void context_AuthenticateRequest(object sender, EventArgs e) { log(); }
		void context_PostAuthenticateRequest(object sender, EventArgs e) { log(); }

		void context_AuthorizeRequest(object sender, EventArgs e) { log(); }
		void context_PostAuthorizeRequest(object sender, EventArgs e) { log(); }

		void context_ResolveRequestCache(object sender, EventArgs e) { log(); }
		void context_PostResolveRequestCache(object sender, EventArgs e) { log(); }

		void context_MapRequestHandler(object sender, EventArgs e) { log(); }
		void context_PostMapRequestHandler(object sender, EventArgs e) { log(); }

		void context_AcquireRequestState(object sender, EventArgs e) { log(); }
		void context_PostAcquireRequestState(object sender, EventArgs e) { log(); }

		void context_PreRequestHandlerExecute(object sender, EventArgs e)
		{
			log();
			sendResponse(sender);
		}

		private static void sendResponse(object sender)
		{
			var application = sender as HttpApplication;
			HttpResponse response = HttpContext.Current.Response;
			if (application.Request.Url.LocalPath == "/Test/")
			{
				application.Response.Write("<html><body><h1>Response from CorsSupportModule</h1></body></html>");
				application.Response.End();
			}
		}
		//Takes some time here
		void context_PostRequestHandlerExecute(object sender, EventArgs e) { log(); }

		void context_ReleaseRequestState(object sender, EventArgs e) { log(); }
		void context_PostReleaseRequestState(object sender, EventArgs e) { log(); }

		void context_UpdateRequestCache(object sender, EventArgs e) { log(); }
		void context_PostUpdateRequestCache(object sender, EventArgs e) { log(); }

		void context_LogRequest(object sender, EventArgs e) { log(); }
		void context_PostLogRequest(object sender, EventArgs e) { log(); }
		
		void context_EndRequest(object sender, EventArgs e)
		{
			log(); 
			HttpResponse response = HttpContext.Current.Response;
			//response.AddHeader("Access-Control-Allow-Origin", "*");
		}

		void context_PreSendRequestContent(object sender, EventArgs e) { log(); }
		void context_PreSendRequestHeaders(object sender, EventArgs e) { log(); }
		void context_RequestCompleted(object sender, EventArgs e) { log(); }

		//Only called when server shuts down
		void context_Disposed(object sender, EventArgs e) { log(); }

		void context_Error(object sender, EventArgs e)
		{
			log();
		}


		void log()
		{
			var eventName = new System.Diagnostics.StackTrace().GetFrames().Skip(1).First().GetMethod().Name;
			var url = HttpContext.Current.Request.Url;
			System.Diagnostics.Debug.WriteLine("{2:HH:mm:ss.fffff} {0:30}: {1}", eventName, url.PathAndQuery, DateTime.Now);
		}

		public void Dispose()
		{
		}
	}
}