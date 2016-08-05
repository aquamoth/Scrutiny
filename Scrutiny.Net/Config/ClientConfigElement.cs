using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Config
{
    public class ClientConfigElement : ConfigurationElement//, IGenericConfigurationElement
    {
        [ConfigurationProperty("captureConsole", DefaultValue = false)]
        public bool CaptureConsole
        {
            get
            {
                bool value = (bool)this["captureConsole"];
                return value;
            }
        }

        [ConfigurationProperty("Frameworks")]
        public GenericElementCollection<NameConfigurationElement> Frameworks
        {
            get { return (GenericElementCollection<NameConfigurationElement>)this["Frameworks"]; }
        }

        [ConfigurationProperty("Preprocessors")]
        public GenericElementCollection<NameConfigurationElement> Preprocessors
        {
            get { return (GenericElementCollection<NameConfigurationElement>)this["Preprocessors"]; }
        }

        [ConfigurationProperty("Reporters")]
        public GenericElementCollection<NameConfigurationElement> Reporters
        {
            get { return (GenericElementCollection<NameConfigurationElement>)this["Reporters"]; }
        }
    }
}
