using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.State
{
	public class Filesystem
	{
		public static string[] ExpandMinimatchUrls(IEnumerable<Config.PathConfigurationElement> paths, string baseUrl)
		{
			var allUrls = new List<string>();

			var pathIndex = 0;
			foreach (Config.PathConfigurationElement pathElement in paths)
			{
				pathIndex++;

				if (pathElement.Url!=null)
				{
					allUrls.Add(pathElement.Url);
				}
				else
				{
					var relativePath = pathElement.Name;
					var directory = DirectoryOf(relativePath);
					var searchPattern = FilePatternOf(relativePath);
					var searchOption = SearchOptionsFor(relativePath);

					var path = MakeRooted(directory, AssemblyDirectory);
					var files = Directory.GetFiles(path, searchPattern, searchOption);

					var urls = files.Select(file => 
						string.Format("{0}/{1}{2}",
							baseUrl, 
							pathIndex, 
							file.Substring(path.Length).Replace(@"\", "/")
						)).ToArray();

					allUrls.AddRange(urls);
				}
			}

			return allUrls.ToArray();
		}

		public static SearchOption SearchOptionsFor(string relativePath)
		{
			SearchOption searchOptions;

			if (relativePath.Contains(@"\**"))
			{
				searchOptions = SearchOption.AllDirectories;
			}
			else
			{
				searchOptions = SearchOption.TopDirectoryOnly;
			}
			return searchOptions;
		}

		public static string FilePatternOf(string relativePath)
		{
			string filePattern;
			if (relativePath.EndsWith(@"\"))
			{
				filePattern = Path.GetFileName(relativePath);
			}
			else
			{
				filePattern = Path.GetFileName(relativePath);
			}
			return filePattern;
		}

		public static string DirectoryOf(string relativePath)
		{
			string directoryPart;
			if (relativePath.EndsWith(@"\"))
			{
				directoryPart = Path.GetDirectoryName(relativePath);
			}
			else
			{
				directoryPart = Path.GetDirectoryName(relativePath);
			}
			if (directoryPart.EndsWith(@"\**"))
			{
				directoryPart = directoryPart.Substring(0, directoryPart.Length - 3);
			}
			return directoryPart;
		}

		public static string MakeRooted(string relativePath, string rootedBasePath)
		{
			var absolutePath = Path.IsPathRooted(relativePath)
				? relativePath
				: Path.Combine(rootedBasePath, relativePath);
			absolutePath = Path.GetFullPath((new Uri(absolutePath)).LocalPath);
			return absolutePath;
		}

		public static string AssemblyDirectory
		{
			get
			{
				if (_assemblyDirectory == null)
				{
					var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
					if (!codeBase.StartsWith("file:///"))
						throw new ApplicationException("Expecting codebase to be a file:/// reference!");
					_assemblyDirectory = Path.GetDirectoryName(codeBase.Substring(8));
				}
				return _assemblyDirectory;
			}
		}
		private static string _assemblyDirectory = null;

	}
}
