using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebIO.Net
{
	public class Command
	{
		public string Name { get; private set; }
		public object Data { get; private set; }

		public Command(string name, object data)
		{
			this.Name = name;
			this.Data = data;
		}
	}
}
