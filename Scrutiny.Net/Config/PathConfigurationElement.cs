using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Config
{
	public class PathConfigurationElement : ConfigurationElement, IGenericConfigurationElement
	{
		[ConfigurationProperty("path")]
		public string Name
		{
			get
			{
				string value = (string)this["path"];
				return String.IsNullOrEmpty(value) ? null : value;
			}
		}

		[ConfigurationProperty("url")]
		public string Url
		{
			get
			{
				string value = (string)this["url"];
				return String.IsNullOrEmpty(value) ? null : value;
			}
		}

		object IGenericConfigurationElement.Key
		{
			get { return this.Name ?? this.Url; }
		}
	}
}
