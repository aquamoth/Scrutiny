using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebIO.Net
{
	public class ClientConnectedEventArgs : EventArgs
	{
		public Client Client { get; private set; }

		public ClientConnectedEventArgs(Client client)
		{
			if (client==null)
			{
				throw new ArgumentNullException("client");
			}
			this.Client = client;
		}
	}
}
