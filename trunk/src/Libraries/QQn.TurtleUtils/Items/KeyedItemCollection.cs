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
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public abstract class KeyedItemCollection<TKey, TValue> : KeyedCollection<TKey, TValue>
		where TValue : ICollectionItem<TValue>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyedItemCollection&lt;TKey, TValue&gt;"/> class.
		/// </summary>
		public KeyedItemCollection()
			: base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyedItemCollection&lt;TKey, TValue&gt;"/> class.
		/// </summary>
		/// <param name="comparer">The comparer.</param>
		public KeyedItemCollection(IEqualityComparer<TKey> comparer)
			: base(comparer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyedItemCollection&lt;TKey, TValue&gt;"/> class.
		/// </summary>
		/// <param name="comparer">The comparer.</param>
		/// <param name="dictionaryCreationTreshold">The dictionary creation treshold.</param>
		public KeyedItemCollection(IEqualityComparer<TKey> comparer, int dictionaryCreationTreshold)
			: base(comparer, dictionaryCreationTreshold)
		{
		}

		/// <summary>
		/// Inserts an element into the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is less than 0.-or-<paramref name="index"/> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"/>.</exception>
		protected override void InsertItem(int index, TValue item)
		{
			base.InsertItem(index, item);
			item.Collection = this;
		}

		/// <summary>
		/// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is less than zero.-or-<paramref name="index"/> is equal to or greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"/>.</exception>
		protected override void RemoveItem(int index)
		{
			this[index].Collection = null;
			base.RemoveItem(index);
		}

		/// <summary>
		/// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// </summary>
		protected override void ClearItems()
		{
			foreach (TValue t in this)
				t.Collection = null;

			base.ClearItems();
		}

		/// <summary>
		/// Replaces the item at the specified index with the specified item.
		/// </summary>
		/// <param name="index">The zero-based index of the item to be replaced.</param>
		/// <param name="item">The new item.</param>
		protected override void SetItem(int index, TValue item)
		{
			this[index].Collection = null;
			base.SetItem(index, item);
			item.Collection = this;
		}
	}
}
