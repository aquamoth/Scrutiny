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

		//[ConfigurationProperty("Tests")]
		//public GenericElementCollection<StringValueElement> Tests
		//{
		//	get { return (GenericElementCollection<StringValueElement>)this["Tests"]; }
		//}

		//[ConfigurationProperty("Tests")]
		//public ConfigurationElementCollection Tests
		[ConfigurationProperty("Tests")]
		public GenericElementCollection<TestsConfigurationElement> Tests
		{
			get { return (GenericElementCollection<TestsConfigurationElement>)this["Tests"]; }
		}

		[ConfigurationProperty("url", DefaultValue = "/Scrutiny")]
		public string Url { get { return ((string)this["url"]).Trim(); } }
	}

	public class TestsConfigurationElement : ConfigurationElement, IGenericConfigurationElement
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
