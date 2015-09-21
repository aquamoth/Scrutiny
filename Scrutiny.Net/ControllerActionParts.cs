using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny
{
	class ControllerActionParts
	{
		public string Controller { get; set; }
		public string Action { get; set; }
		public string[] Value { get; set; }

		internal static ControllerActionParts FromPath(string path)
		{
			if (path.StartsWith("/"))
				path = path.Substring(1);
			var pathParts = path.Split('/');

			var parts = new ControllerActionParts();
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
