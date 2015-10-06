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
					TestFiles = Filesystem.ExpandMinimatchUrls(paths)
									.Select(x => string.Format("{0}/{1}", testRun, x))
									.ToArray()
				};

				return View(model);
			}
			catch (Exception ex)
			{
				//C:\Users\maas\Workspace\Git\MvcKarmaDemo\MvcKarmaDemo.Tests\Scripts\TestRunner
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
			var basePath = Filesystem.DirectoryOf(pathFor(testRun, pathIndex));
			var subPath = string.Join(@"\", urlParts.Skip(2));
			var relativePath = Path.Combine(basePath, subPath);
			var absolutePath = Filesystem.MakeRooted(relativePath, Filesystem.AssemblyDirectory);

			using (var reader = File.OpenText(absolutePath))
			{
				var content = await reader.ReadToEndAsync();
				return content;
			}
		}

		private static string pathFor(int testRun, int pathIndex)
		{
			var path = Config.Scrutiny.PathsForTestrun(testRun).Skip(pathIndex - 1).First();
			return path;
		}

		IDictionary<string, string> testsDictionary
		{
			get
			{
				var dictionary = (IDictionary<string,string>)this.Context.Cache.Get(Module.TEST_FILES_CACHE_KEY);
				if (dictionary == null)
				{
#warning Paths to test scripts should be in config
					//TODO: Monitor for changes => Throw cache, possibly force tests to run?
					var paths = new[]{
						@"C:\Users\maas\Workspace\Git\MvcKarmaDemo\MvcKarmaDemo.Tests\Scripts"
					};

					//TODO: Cache with content?
					dictionary = paths
						.SelectMany(path => Directory
												.EnumerateFiles(path, "*.*")
												.Select(file =>
													new
													{
														url = file.Substring(path.Length + 1).Replace('\\', '/'),
														path = file
													})
						).ToDictionary(x => x.url, x => x.path);

					this.Context.Cache.Add(
						Module.TEST_FILES_CACHE_KEY,
						dictionary,
						null,	//TODO: Depend on changes in file paths?
						System.Web.Caching.Cache.NoAbsoluteExpiration,
						new TimeSpan(0, 0, 5), // 5s sliding expiration is enough to cache thoughout one test run, but not between them
						System.Web.Caching.CacheItemPriority.Low,
						null
					);
				}
				return dictionary;
			}
		}
	}
}
