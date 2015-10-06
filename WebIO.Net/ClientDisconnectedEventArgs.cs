using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebIO.Net
{
	public class ClientDisconnectedEventArgs : System.EventArgs
	{
		public Client Client { get; private set; }

		public ClientDisconnectedEventArgs(Client client)
		{
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}
			this.Client = client;
		}
	}
}
