using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebIO.Net
{
	public class ClientConnectingEventArgs : System.ComponentModel.CancelEventArgs
	{
		public Client Client { get; private set; }
		public string CancelReason { get; set; }

		public ClientConnectingEventArgs(Client client)
		{
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}
			this.Client = client;
		}
	}
}
