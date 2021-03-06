﻿using System;
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

        public static IEnumerable<PathConfigurationElement> AssembliesToSearchForApisIn()
        {
            return Section.ApiAssemblies.Select(a => a);
        }

		public static IEnumerable<PathConfigurationElement> ScriptsForTestrun(int testRun)
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

        public static IEnumerable<PathConfigurationElement> StylesheetsForTestrun(int testRun)
        {
            if (testRun == 0)
            {
                return Section.Stylesheets.Select(p => p);
            }
            else
            {
                throw new NotImplementedException("Test groups are not yet supported.");
            }
        }

        [ConfigurationProperty("url", DefaultValue = "/Scrutiny")]
		public string Url { get { return ((string)this["url"]).Trim(); } }

        [ConfigurationProperty("ClientConfig")]
        public ClientConfigElement ClientConfig
        {
            get { return ((ClientConfigElement)this["ClientConfig"]); }
        }

        [ConfigurationProperty("ApiAssemblies")]
        public GenericElementCollection<PathConfigurationElement> ApiAssemblies
        {
            get { return (GenericElementCollection<PathConfigurationElement>)this["ApiAssemblies"]; }
        }

        [ConfigurationProperty("Paths")]
		public GenericElementCollection<PathConfigurationElement> Paths
		{
			get { return (GenericElementCollection<PathConfigurationElement>)this["Paths"]; }
		}

        [ConfigurationProperty("Stylesheets")]
        public GenericElementCollection<PathConfigurationElement> Stylesheets
        {
            get { return (GenericElementCollection<PathConfigurationElement>)this["Stylesheets"]; }
        }
    }
}
