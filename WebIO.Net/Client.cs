﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebIO.Net
{
	public class Client
	{
		public string Id { get; set; }
		public System.Collections.Concurrent.ConcurrentQueue<Command> CommandQueue { get; private set; }

		public Client()
		{
			this.CommandQueue = new System.Collections.Concurrent.ConcurrentQueue<Command>();
		}
	}
}
