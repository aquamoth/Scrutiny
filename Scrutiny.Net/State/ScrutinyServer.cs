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

			//TODO: Format as: Chrome 45.0.2454 (Windows 8.1)
			var request = System.Web.HttpContext.Current.Request;
			e.Client.Browser =
				string.Format("{0} {1} ({2})",
					request.Browser.Browser,
					request.Browser.Version,
					"TODO: Unknown"
				);

			var browsers = this.Clients.Select(c => c.Browser).ToArray();
			this.SendToAll("Clients", browsers);



#warning Just testing code!
			this.SendTo(e.Client, "Load", "/Scrutiny/Context");
		}

		protected override void OnClientDisconnected(WebIO.Net.ClientDisconnectedEventArgs e)
		{
			base.OnClientDisconnected(e);

			var browsers = this.Clients.Select(c => c.Browser).ToArray();
			this.SendToAll("Clients", browsers);
		}
	}
}
