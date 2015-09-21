using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny
{
	internal class Resources
	{
		public static string GetString(string name)
		{
			string template;
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			using (var stream = assembly.GetManifestResourceStream(name))
			{
				if (stream == null)
					throw new System.IO.FileNotFoundException("The requested resource was not found on the server.", name);

				using (var streamReader = new System.IO.StreamReader(stream))
				{
					template = streamReader.ReadToEnd();
				}
			}
			return template;
		}
	}
}
