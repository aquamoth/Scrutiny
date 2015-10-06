using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Scrutiny
{
	//See: http://brockallen.com/2013/07/27/implementing-async-http-modules-in-asp-net-using-tpls-task-api/
	public abstract class AsyncHttpModule : System.Web.IHttpModule
	{
		public void Init(System.Web.HttpApplication context)
		{
			context.AddOnBeginRequestAsync(onBeginRequestBegin, onBeginRequestEnd);
		}

		public void Dispose()
		{
		}

		IAsyncResult onBeginRequestBegin(object sender, EventArgs e, AsyncCallback cb, object extraData)
		{
			var tcs = new TaskCompletionSource<object>(extraData);
			OnBeginRequestAsync(HttpContext.Current).ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					tcs.SetException(t.Exception.InnerExceptions);
				}
				else
				{
					tcs.SetResult(null);
				}
				if (cb != null) cb(tcs.Task);
			});
			return tcs.Task;
		}

		void onBeginRequestEnd(IAsyncResult ar)
		{
			Task t = (Task)ar;
			t.Wait();
		}

		protected virtual async Task OnBeginRequestAsync(HttpContext contexts)
		{
			await Task.Yield();
		}

	}
}
