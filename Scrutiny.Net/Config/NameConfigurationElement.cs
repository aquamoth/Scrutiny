using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Config
{
    public class NameConfigurationElement : ConfigurationElement, IGenericConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = true)]
		public string Name
		{
			get
			{
				return (string)this["name"];
			}
		}

		object IGenericConfigurationElement.Key
		{
			get { return this.Name; }
		}
	}
}
