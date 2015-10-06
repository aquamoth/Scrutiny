using Scrutiny.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Controllers
{
	class ContextController : Controller
	{
		/// <summary>
		/// Also update HomeController/Debug when making changes here
		/// </summary>
		/// <returns></returns>
		public string Index(int testRun)
		{
			try
			{
				ViewBag.Add("RootUrl", Scrutiny.Config.Scrutiny.Section.Url);

				var paths = Config.Scrutiny.PathsForTestrun(testRun);
				var model = new Models.ContextModels.Index
				{
					Scripts = Filesystem.ExpandMinimatchUrls(paths)
									.Select(x => string.Format("{0}/{1}", testRun, x))
									.ToArray()
				};

				return View(model);
			}
			catch (Exception ex)
			{
#warning Write exception about dir not found to user
				throw;
			}
		}

		/// <summary>
		/// Returns the content of a test script found in one of the paths set in the config
		/// </summary>
		/// <param name="url">relative url to the file to return content for</param>
		/// <returns>Content of the file, as a string</returns>
		public async Task<string> Tests(string[] urlParts)
		{
			var testRun = int.Parse(urlParts[0]);
			var pathIndex = int.Parse(urlParts[1]);
			var subPath = string.Join(@"\", urlParts.Skip(2));

			var pathInConfig = Config.Scrutiny.PathsForTestrun(testRun).Skip(pathIndex - 1).First();
			var basePath = Filesystem.DirectoryOf(pathInConfig);
			var relativePath = Path.Combine(basePath, subPath);
			var absolutePath = Filesystem.MakeRooted(relativePath, Filesystem.AssemblyDirectory);

			using (var reader = File.OpenText(absolutePath))
			{
				var content = await reader.ReadToEndAsync();
				return content;
			}
		}

	}
}
