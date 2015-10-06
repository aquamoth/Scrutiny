using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Scrutiny.Config
{
	public interface IGenericConfigurationElement
	{
		object Key { get; }
	}
	
	public sealed class GenericElementCollection<T> : ConfigurationElementCollection, IEnumerable<T>
		where T : ConfigurationElement, IGenericConfigurationElement, new()
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new T();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((IGenericConfigurationElement)element).Key;
		}

		#region IEnumerable<T> Members

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			foreach (T item in this)
				yield return item;
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}

		#endregion
	}
}
