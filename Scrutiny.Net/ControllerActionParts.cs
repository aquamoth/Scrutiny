using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny
{
	public class ControllerActionParts
	{
		public string Controller { get; set; }
		public string Action { get; set; }
		public string[] Value { get; set; }

		public string OriginalPath { get; private set; }

		internal static ControllerActionParts FromPath(string path)
		{
			var parts = new ControllerActionParts { OriginalPath = path };
	
			if (path.StartsWith("/"))
				path = path.Substring(1);
			var pathParts = path.Split('/');

			parts.Controller = defaultIfEmpty(pathParts, 0, "Home");
			parts.Action = defaultIfEmpty(pathParts, 1, "Index");
			parts.Value = pathParts.Skip(2).ToArray();

			return parts;
		}

		private static string defaultIfEmpty(string[] pathParts, int index, string defaultValue)
		{
			if (pathParts.Length <= index)
				return defaultValue;
	
			var value = pathParts[index];

			if (string.IsNullOrWhiteSpace(value))
				return defaultValue;

			return value;
		}
	}
}
