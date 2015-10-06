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

		public static IEnumerable<PathConfigurationElement> PathsForTestrun(int testRun)
		{
			if (testRun == 0)
			{
				return Section.Paths.Select(p => p);
			}
			else
			{
				throw new NotImplementedException("Test groups are not yet supported.");
			}
		}

		[ConfigurationProperty("url", DefaultValue = "/Scrutiny")]
		public string Url { get { return ((string)this["url"]).Trim(); } }

		[ConfigurationProperty("Paths")]
		public GenericElementCollection<PathConfigurationElement> Paths
		{
			get { return (GenericElementCollection<PathConfigurationElement>)this["Paths"]; }
		}
	}
}
