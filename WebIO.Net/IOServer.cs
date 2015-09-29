using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;

namespace WebIO.Net
{
	public class IOServer
	{
		public string Connect()
		{
			var client = RegisterNewClient();
			return Json.Encode(new { id = client.Id });
		}

		public async Task<string> Poll(string id)
		{
			var client = FindClient(id);

			var commands = await FlushCommandQueue(client);

			if (commands.Any())
				return Json.Encode(new { id = client.Id, commands = commands });
			else
				return Json.Encode(new { id = client.Id });
		}

		protected virtual async Task<Command[]> FlushCommandQueue(Client client)
		{
			var response = HttpContext.Current.Response;
			while (response.IsClientConnected && client.CommandQueue.IsEmpty)
			{
				await Task.Delay(200);
			}

			if (!response.IsClientConnected)
			{
				DisconnectClient(client);
				throw new ApplicationException("Client has disconnected!");
			}

			var commands = new List<Command>();
			Command command;
			while(client.CommandQueue.TryDequeue(out command))
			{
				commands.Add(command);
			}

			return commands.ToArray();
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

			//TODO: Check if we can upgrade to WebSockets
			RegisterClientAsLongPollingQuery(client);

			var clientConnectedEventArgs = new ClientConnectedEventArgs(client);
			OnClientConnected(clientConnectedEventArgs);

			return client;
		}

		protected virtual void DisconnectClient(Client client)
		{
			//TODO: If this can release the polling in FlushCommandQueue(), this command can also be used by the server to bunk clients
			Client removedClient;
			if (_clients.TryRemove(client.Id, out removedClient))
			{
				var clientDisconnectedEventArgs = new ClientDisconnectedEventArgs(client);
				OnClientDisconnected(clientDisconnectedEventArgs);
			}
		}

		public virtual Client FindClient(string id)
		{
			if (!_clients.ContainsKey(id))
				throw new ArgumentException("Not a valid connection id!");
			return _clients[id];
		}

		public IEnumerable<Client> Clients { get { return _clients.Values; } }
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
			bool clientIdIsUnique;
			do
			{
				client.Id = Guid.NewGuid().ToString().Replace("-", "");
				clientIdIsUnique = _clients.TryAdd(client.Id, client);
			} while (!clientIdIsUnique);
		}

		#region Public Events

		public event EventHandler<ClientConnectingEventArgs> ClientConnecting;
		protected virtual void OnClientConnecting(ClientConnectingEventArgs e)
		{
			if (ClientConnecting != null)
			{
				ClientConnecting(this, e);
			}
		}

		public event EventHandler<ClientConnectedEventArgs> ClientConnected;
		protected virtual void OnClientConnected(ClientConnectedEventArgs e)
		{
			if (ClientConnected != null)
			{
				ClientConnected(this, e);
			}
		}

		public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
		protected virtual void OnClientDisconnected(ClientDisconnectedEventArgs e)
		{
			if (ClientDisconnected != null)
			{
				ClientDisconnected(this, e);
			}
		}

		#endregion Public Events

		public void SendTo(Client client, string name, object args)
		{
			if (!Clients.Contains(client))
				throw new ArgumentException("Client is not registered.");

			var command = new Command(name, args);
			client.CommandQueue.Enqueue(command);
		}

		public void SendToAll(string name, object args)
		{
			var command = new Command(name, args);
			foreach (var client in Clients)
			{
				client.CommandQueue.Enqueue(command);
			}
		}

		protected string JsonEncode(object value)
		{
			return System.Web.Helpers.Json.Encode(value);
		}
		//protected object JsonDecode(string value)
		//{
		//	return System.Web.Helpers.Json.Decode(value);
		//}
		protected T JsonDecode<T>(string value) where T : class
		{
			return System.Web.Helpers.Json.Decode<T>(value);
		}

	}
}
