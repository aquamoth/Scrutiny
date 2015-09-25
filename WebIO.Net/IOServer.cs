﻿using System;
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
		public async Task<string> Poll(string id)
		{
			var client = string.IsNullOrEmpty(id)
				? RegisterNewClient()
				: FindClient(id);

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

			//TODO: Is the IsClientConnected test even useful here?
			if (!response.IsClientConnected)
				throw new ApplicationException("Client is disconnected!");

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

		public event EventHandler<ClientConnectedEventArgs> ClientConnected;
		protected virtual void OnClientConnected(ClientConnectedEventArgs e)
		{
			if (ClientConnected != null)
			{
				ClientConnected(this, e);
			}
		}

		public event EventHandler<ClientConnectingEventArgs> ClientConnecting;
		private void OnClientConnecting(ClientConnectingEventArgs e)
		{
			if (ClientConnecting != null)
			{
				ClientConnecting(this, e);
			}
		}

		public void SendToAll(string p, string[] browsers)
		{
			var command = new Command(p, browsers);
			foreach (var client in Clients)
			{
				client.CommandQueue.Enqueue(command);
			}
		}
	}
}