using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.State
{
	class ScrutinyServer : WebIO.Net.IOServer
	{
		protected override void OnClientConnected(WebIO.Net.ClientConnectedEventArgs e)
		{
			base.OnClientConnected(e);

			//This is business logic and should be moved elsewhere
			e.Client.Browser = System.Web.HttpContext.Current.Request.Browser.Browser;

			var browsers = this.Clients.Select(c => c.Browser).ToArray();
			this.SendToAll("Clients", browsers);
			//TODO: Consider setting a session cookie on first request if not already set or timed out on the server
		}

		protected override void OnClientDisconnected(WebIO.Net.ClientDisconnectedEventArgs e)
		{
			base.OnClientDisconnected(e);

			var browsers = this.Clients.Select(c => c.Browser).ToArray();
			this.SendToAll("Clients", browsers);
		}
	}
}
