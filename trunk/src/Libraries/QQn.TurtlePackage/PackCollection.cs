using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace QQn.TurtlePackage
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class PackCollection<T> : KeyedCollection<string, T>
		where T : PackItem, new()
	{
		readonly PackItem _parent;

		/// <summary>
		/// Initializes a new instance of the <see cref="PackCollection&lt;T&gt;"/> class.
		/// </summary>
		protected PackCollection()
			: base(StringComparer.InvariantCultureIgnoreCase, 16)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PackCollection&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="parent">The parent.</param>
		protected PackCollection(PackItem parent)
			: this()
		{
			_parent = parent;
			if (parent != null)
			{
				parent.PackChanged += new EventHandler(OnParentPackChanged);
				OnParentPackChanged(this, EventArgs.Empty);
			}
		}

		void OnParentPackChanged(object sender, EventArgs e)
		{
 			foreach(T i in this)
				i.Pack = Pack;
		}

		/// <summary>
		/// Inserts an element into the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is less than 0.-or-<paramref name="index"/> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"/>.</exception>
		protected override void InsertItem(int index, T item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			EnsureWritable();
			base.InsertItem(index, item);
			item.Pack = Pack;
			item.Parent = Parent;
		}

		/// <summary>
		/// Removes all elements from the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/>.
		/// </summary>
		protected override void ClearItems()
		{
			EnsureWritable();
			foreach (T item in this)
			{
				item.Pack = null;
				item.Parent = null;
			}
			base.ClearItems();
		}

		/// <summary>
		/// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.KeyedCollection`2"/>.
		/// </summary>
		/// <param name="index">The index of the element to remove.</param>
		protected override void RemoveItem(int index)
		{
			EnsureWritable();

			T item = this[index];
			item.Pack = null;
			item.Parent = null;

			base.RemoveItem(index);
		}

		/// <summary>
		/// Replaces the item at the specified index with the specified item.
		/// </summary>
		/// <param name="index">The zero-based index of the item to be replaced.</param>
		/// <param name="item">The new item.</param>
		protected override void SetItem(int index, T item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			EnsureWritable();
			T oldItem = this[index];
			oldItem.Pack = null;
			oldItem.Parent = null;
			base.SetItem(index, item);
			item.Pack = Pack;
			item.Parent = Parent;
		}

		/// <summary>
		/// Ensures the writable.
		/// </summary>
		protected void EnsureWritable()
		{
			if (_parent != null)
				_parent.EnsureWritable();
		}

		/// <summary>
		/// Gets the pack.
		/// </summary>
		/// <value>The pack.</value>
		public Pack Pack
		{
			get { return (_parent != null) ? _parent.Pack : null; }
		}

		/// <summary>
		/// Gets the parent.
		/// </summary>
		/// <value>The parent.</value>
		public PackItem Parent
		{
			get { return _parent; }
		}

		/// <summary>
		/// When implemented in a derived class, extracts the key from the specified element.
		/// </summary>
		/// <param name="item">The element from which to extract the key.</param>
		/// <returns>The key for the specified element.</returns>
		protected override string GetKeyForItem(T item)
		{
			return item.Name;
		}

		/// <summary>
		/// Adds the item.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public T AddItem(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			else if (Contains(name))
				throw new ArgumentException("Name already exixts", "name");

			T item = new T();
			item.Name = name;
			Add(item);

			return item;
		}
	}
}
