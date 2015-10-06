using System.IO;
using System.Threading.Tasks;

namespace Scrutiny
{
	internal class Resources
	{
		public static string GetString(string name)
		{
			using (var streamReader = createResourceStreamReader(name))
			{
				return streamReader.ReadToEnd();
			}
		}

		public static async Task<string> GetStringAsync(string name)
		{
			using (var streamReader = createResourceStreamReader(name))
			{
				return await streamReader.ReadToEndAsync();
			}
		}

		static StreamReader createResourceStreamReader(string name)
		{
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			var stream = assembly.GetManifestResourceStream(name);
			if (stream == null)
				throw new FileNotFoundException(string.Format("The resource '{0}' was not found on the server.", name), name);
			return new StreamReader(stream);
		}
	}
}
