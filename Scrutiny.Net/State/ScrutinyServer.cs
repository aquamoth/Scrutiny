using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.State
{
	class RegisterModel
	{
		public string Browser { get; set; }
	}

	class ScrutinyServer : WebIO.Net.IOServer
	{
		protected override void OnClientConnected(WebIO.Net.ClientConnectedEventArgs e)
		{
			base.OnClientConnected(e);

//			//TODO: Format as: Chrome 45.0.2454 (Windows 8.1)
//			var request = System.Web.HttpContext.Current.Request;
//			e.Client.Browser =
//				string.Format("{0} {1} ({2})",
//					request.Browser.Browser,
//					request.Browser.Version,
//					"TODO: Unknown"
//				);

//			broadcastClientsList();



//#warning Just testing code!
//			this.SendTo(e.Client, "execute", "/Scrutiny/Context");
		}

		protected override void OnClientDisconnected(WebIO.Net.ClientDisconnectedEventArgs e)
		{
			base.OnClientDisconnected(e);
			broadcastClientsList();
		}

		public void Register(string id, NameValueCollection arguments)
		{
			var client = FindClient(id);
			client.Browser = arguments["args[name]"];
			broadcastClientsList();




#warning Just testing code!
			this.SendTo(client, "execute", "{}");
		}


		private void broadcastClientsList()
		{
			var browsers = this.Clients
				.Select(c => new RegisterResponse { isReady = true, name = c.Browser })
				.ToArray();
			this.SendToAll("info", browsers);
		}

	}

	class RegisterResponse
	{
		public bool isReady { get; set; }
		public string name { get; set; }
	}
}
