using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Config
{
	public class Scrutiny : System.Configuration.ConfigurationSection
	{
		public static Scrutiny Section
		{
			get
			{
				return ConfigurationManager.GetSection("Scrutiny") as Scrutiny ?? new Scrutiny();
			}
		}

		public static IEnumerable<string> PathsForTestrun(int testRun)
		{
			IEnumerable<string> paths;
			if (testRun == 0)
			{
				paths = Section.Paths.Select(p => p.Name);
			}
			else
			{
				throw new NotImplementedException("Test groups are not yet supported.");
			}
			return paths;
		}

		[ConfigurationProperty("url", DefaultValue = "/Scrutiny")]
		public string Url { get { return ((string)this["url"]).Trim(); } }

		[ConfigurationProperty("Paths")]
		public GenericElementCollection<PathConfigurationElement> Paths
		{
			get { return (GenericElementCollection<PathConfigurationElement>)this["Paths"]; }
		}
	}

	public class PathConfigurationElement : ConfigurationElement, IGenericConfigurationElement
	{
		[ConfigurationProperty("name")]
		public string Name
		{
			get
			{
				string value = (string)this["name"];
				return String.IsNullOrEmpty(value) ? null : value;
			}
		}

		object IGenericConfigurationElement.Key
		{
			get { return this.Name; }
		}
	}

}
