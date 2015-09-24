using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebIO.Net
{
	public class IOServer
	{
#warning Poll() should be async!
		public string Poll(string id)
		{
			Client client;
			if (!string.IsNullOrEmpty(id))
			{
				client = FindClient(id);
			}
			else
			{
				client = RegisterNewClient();
				
				//TODO: Check if we can upgrade to WebSockets
				RegisterClientAsLongPollingQuery(client);
				
				var clientConnectedEventArgs = new ClientConnectedEventArgs(client);
				OnClientConnected(clientConnectedEventArgs);
			}

			var response = FlushMessageQueue(client);


			throw new NotImplementedException();
		}

		protected virtual object FlushMessageQueue(Client client)
		{
			//TODO: Poll the message queue for the client
			//TODO: Wait for the request timeout
			//TODO: Return the id + messages from the queue, if any
			throw new NotImplementedException();
		}

		protected virtual Client RegisterNewClient()
		{
			var client = CreateClient();

			var clientConnectingEventArgs = new ClientConnectingEventArgs(client);
			OnClientConnecting(clientConnectingEventArgs);
			if (clientConnectingEventArgs.Cancel)
			{
				var message = new StringBuilder();
				message.AppendLine("Connection was aborted.");
				if (!string.IsNullOrWhiteSpace(clientConnectingEventArgs.CancelReason))
				{
					message.AppendFormat("Reason: {0}\n", clientConnectingEventArgs.CancelReason);
				}
				throw new ApplicationException(message.ToString());
			}
			return client;
		}

		public virtual Client FindClient(string id)
		{
			if (!_clients.ContainsKey(id))
				throw new ArgumentException("Not a valid connection id!");
			return _clients[id];
		}
		readonly ConcurrentDictionary<string, Client> _clients = new ConcurrentDictionary<string, Client>();
#warning _clients should probably be replaced by Cache-items, since that automatically would support disconnection cleanup
#warning Let sliding timeouts == client disconnected

		/// <summary>
		/// Creates a new Client object. 
		/// Should be overloaded if a custom Client class is required.
		/// </summary>
		/// <returns></returns>
		protected virtual Client CreateClient()
		{
			var client = new Client();
			return client;
		}

		protected virtual void RegisterClientAsLongPollingQuery(Client client)
		{
			client.Id = Guid.NewGuid().ToString().Replace("-", "");
			//TODO: Do we need to store changes somehow?
		}

		public event EventHandler ClientConnected;
		protected virtual void OnClientConnected(ClientConnectedEventArgs e)
		{
			if (ClientConnected != null)
			{
				ClientConnected(this, e);
			}
		}

		public event EventHandler ClientConnecting;
		private void OnClientConnecting(EventArgs e)
		{
			if (ClientConnecting != null)
			{
				ClientConnecting(this, e);
			}
		}
	}
}
