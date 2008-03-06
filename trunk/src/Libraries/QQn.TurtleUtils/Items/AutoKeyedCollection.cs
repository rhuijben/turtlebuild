using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace QQn.TurtleUtils.Items
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	public interface IKeyed<TKey>
	{
		/// <summary>
		/// Gets the key.
		/// </summary>
		/// <value>The key.</value>
		TKey Key
		{
			get;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class AutoKeyedCollection<TKey, TValue> : KeyedCollection<TKey, TValue>
		where TValue : IKeyed<TKey>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AutoKeyedCollection&lt;TKey, TValue&gt;"/> class.
		/// </summary>
		public AutoKeyedCollection()
			: base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoKeyedCollection&lt;TKey, TValue&gt;"/> class.
		/// </summary>
		/// <param name="comparer">The comparer.</param>
		public AutoKeyedCollection(IEqualityComparer<TKey> comparer)
			: base(comparer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoKeyedCollection&lt;TKey, TValue&gt;"/> class.
		/// </summary>
		/// <param name="comparer">The implementation of the <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> generic interface to use when comparing keys, or null to use the default equality comparer for the type of the key, obtained from <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.</param>
		/// <param name="dictionaryCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary (0 creates the lookup dictionary when the first item is added), or –1 to specify that a lookup dictionary is never created.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="dictionaryCreationThreshold"/> is less than –1.</exception>
		public AutoKeyedCollection(IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
			: base(comparer, dictionaryCreationThreshold)
		{
		}

		/// <summary>
		/// When implemented in a derived class, extracts the key from the specified element.
		/// </summary>
		/// <param name="item">The element from which to extract the key.</param>
		/// <returns>The key for the specified element.</returns>
		protected override TKey GetKeyForItem(TValue item)
		{
			return item.Key;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	public class AutoKeyedCollection<TKey> : KeyedCollection<TKey, TKey>
		where TKey : IKeyed<TKey>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AutoKeyedCollection&lt;TKey, TValue&gt;"/> class.
		/// </summary>
		public AutoKeyedCollection()
			: base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoKeyedCollection&lt;TKey, TValue&gt;"/> class.
		/// </summary>
		/// <param name="comparer">The comparer.</param>
		public AutoKeyedCollection(IEqualityComparer<TKey> comparer)
			: base(comparer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoKeyedCollection&lt;TKey, TValue&gt;"/> class.
		/// </summary>
		/// <param name="comparer">The implementation of the <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> generic interface to use when comparing keys, or null to use the default equality comparer for the type of the key, obtained from <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.</param>
		/// <param name="dictionaryCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary (0 creates the lookup dictionary when the first item is added), or –1 to specify that a lookup dictionary is never created.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="dictionaryCreationThreshold"/> is less than –1.</exception>
		public AutoKeyedCollection(IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
			: base(comparer, dictionaryCreationThreshold)
		{
		}

		/// <summary>
		/// When implemented in a derived class, extracts the key from the specified element.
		/// </summary>
		/// <param name="item">The element from which to extract the key.</param>
		/// <returns>The key for the specified element.</returns>
		protected override TKey GetKeyForItem(TKey item)
		{
			return item;
		}
	}
}
