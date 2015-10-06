using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Config
{
	public interface IConfig
	{
		string[] PreTestFiles { get; }
		//string[] TestFiles { get; }
		string[] PostTestFiles { get; }
	}

	public class Config : IConfig
	{
		public string[] PreTestFiles
		{
			get
			{
				return new string[]{
					"/Scrutiny/Scripts/expect.js/index.js",
 					"/Scrutiny/Scripts/mocha/mocha.js",
					"/Scrutiny/Scripts/karma_mocha/lib/adapter.js",
					"/Scrutiny/Scripts/require.js"
				};
			}
		}

		public string[] PostTestFiles
		{
			get
			{
				return new string[] { 
					"/Scrutiny/Scripts/karma_commonjs/client/commonjs_bridge.js"
				};
			}
		}





		public static IConfig Default
		{
			get
			{
				if (_default == null)
					_default = new Config();
				return _default;
			}
		}
		static IConfig _default = null;
	}
}
