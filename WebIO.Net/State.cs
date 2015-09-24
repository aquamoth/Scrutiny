using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace WebIO.Net
{
	internal class State
	{
		private State() 
		{
			Clients = new System.Collections.Concurrent.ConcurrentDictionary<string, Client>();
		}

		public System.Collections.Concurrent.ConcurrentDictionary<string, Client> Clients { get; private set; }
	
		
		
		
		
		public static State Default
		{
			get
			{
				var cacheKey = "Scrutiny.CacheObject";
				var value = HttpContext.Current.Cache.Get(cacheKey) as State;
				if (value != null)
					return value;

				lock (HttpContext.Current)
				{
					var existingCacheObject = HttpContext.Current.Cache.Get(cacheKey);
					if (existingCacheObject != null)
					{
						value = existingCacheObject as State;
						if (value != null)
							return value;

						throw new ApplicationException("Scrutiny can't store its state! Another object already occupies Scrutinys key '{0}' in the Cache. To resolve, please specify another cache key name in web.config.");
					}
					else
					{
						value = new State();
						HttpContext.Current.Cache.Add(cacheKey, value, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, cacheItemRemovedCallback);
						return value;
					}
				}
			}
		}


		private static void cacheItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
		{
			throw new NotImplementedException();
		}

	}
}
