using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Config
{
	public class StringValueElement : ConfigurationElement, IGenericConfigurationElement
	{
		public StringValueElement()
		{
		}

		public StringValueElement(string value)
		{
			this["value"] = value;
		}

		public StringValueElement(string value, string description)
			: this(value)
		{
			this["description"] = description;
		}

		[ConfigurationProperty("value", IsRequired = true, IsKey = true)]
		public string Value
		{
			get { return (string)this["value"]; }
		}

		[ConfigurationProperty("description")]
		public string Description
		{
			get { return (string)this["description"]; }
		}

		#region IGenericConfigurationElement Members

		object IGenericConfigurationElement.Key
		{
			get { return this.Value; }
		}

		#endregion
	}
}
