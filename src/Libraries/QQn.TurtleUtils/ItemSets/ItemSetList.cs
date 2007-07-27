using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace QQn.TurtleUtils.ItemSets
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TLeaf">The type of the leaf.</typeparam>
	public interface ILeafEnumerable<TLeaf>
	{
		/// <summary>
		/// Gets all leaves.
		/// </summary>
		/// <value>All leaves.</value>
		IEnumerable<TLeaf> AllLeaves { get; }
	}
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TList">The type of the list.</typeparam>
	/// <typeparam name="TRoot">The type of the root.</typeparam>
	/// <typeparam name="TLeaf">The type of the leaf.</typeparam>
	public abstract class ItemSetList<T, TList, TLeaf, TRoot> : NamedItemSetBase<TRoot>, /* IList<T>, System.Collections.IList, IDictionary<string, T>,*/ ILeafEnumerable<TLeaf>
		where T : NamedItemSetBase<TRoot>, ILeafEnumerable<TLeaf>, new()
		where TRoot : ItemSetRoot<TRoot>
		where TList : ItemSetList<T, TList, TLeaf, TRoot>, new()
		where TLeaf : NamedItemSetBase<TRoot>
	{
		ItemSetCollection _innerList;


		/// <summary>
		/// Adds an item with the specified name
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public T AddItem(string name)
		{
			if (Items.Contains(name))
				throw new ArgumentOutOfRangeException("name", name, "Name already exists in itemset");

			T t = new T();
			t.Name = name;

			Items.Add(t);

			return t;
		}

		/// <summary>
		/// Gets or sets the package.
		/// </summary>
		/// <value>The package.</value>
		public override TRoot Package
		{
			get
			{
				return base.Package;
			}
			set
			{
				base.Package = value;
				foreach (T i in Items)
				{
					i.Package = value;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public sealed override bool IsReadOnly
		{
			get { return (Package != null) ? Package.IsReadOnly : false; }
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		/// <exception cref="InvalidOperationException">When setting the name</exception>
		[XmlIgnore]
		public override string Name
		{
			get { return null; }
			set { throw new InvalidOperationException(); }
		}		

		/// <summary>
		/// Gets all leaves.
		/// </summary>
		/// <value>All leaves.</value>
		public IEnumerable<TLeaf> AllLeaves
		{
			get
			{
				foreach (T item in Items)
				{
					TLeaf leaf = item as TLeaf;
					if (leaf != null)
					{
						yield return leaf;
						continue;
					}

					foreach (TLeaf sLeaf in item.AllLeaves)
					{
						yield return sLeaf;
					}
				}
			}
		}

		/// <summary>
		/// The contained items
		/// </summary>
		public ItemSetCollection Items
		{
			get
			{
				if (_innerList == null)
					_innerList = new ItemSetCollection(this);

				return _innerList;
			}
			set
			{
				EnsureWritable();
				if ((_innerList != null) || (value == null))
					throw new InvalidOperationException();

				Items.AddRange(value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public sealed class ItemSetCollection : Collection<T>
		{
			ItemSetList<T, TList, TLeaf, TRoot> _list;
			readonly SortedList<string, T> _byName;

			/// <summary>
			/// Initializes a new instance of the <see cref="ItemSetList&lt;T, TList, TLeaf, TRoot&gt;.ItemSetCollection"/> class.
			/// </summary>
			public ItemSetCollection()
			{
				_byName = new SortedList<string, T>(StringComparer.InvariantCultureIgnoreCase);
			}

			internal ItemSetCollection(ItemSetList<T, TList, TLeaf, TRoot> list)
				: this()
			{
				_list = list;
			}

			/// <summary>
			/// Adds the range.
			/// </summary>
			/// <param name="items">The items.</param>
			public void AddRange(IEnumerable<T> items)
			{
				if (items == null)
					throw new ArgumentNullException("items");

				foreach (T t in items)
				{
					Add(t);
				}
			}

			/// <summary>
			/// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1"></see> at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index at which item should be inserted.</param>
			/// <param name="item">The object to insert. The value can be null for reference types.</param>
			/// <exception cref="T:System.ArgumentOutOfRangeException">index is less than zero.-or-index is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"></see>.</exception>
			protected override void InsertItem(int index, T item)
			{
				if((index < 0) || (index > Count))
					throw new ArgumentOutOfRangeException("index", index, "Index out of range");

				EnsureWritable();
				_byName.Add(item.Name, item);
				base.InsertItem(index, item);
			}

			/// <summary>
			/// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1"></see>.
			/// </summary>
			/// <param name="index">The zero-based index of the element to remove.</param>
			/// <exception cref="T:System.ArgumentOutOfRangeException">index is less than zero.-or-index is equal to or greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"></see>.</exception>
			protected override void RemoveItem(int index)
			{
				if((index < 0) || (index >= Count))
					throw new ArgumentOutOfRangeException("index", index, "Index out of range");

				EnsureWritable();
				_byName.Remove(this[index].Name);
				base.RemoveItem(index);
			}

			/// <summary>
			/// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1"></see>.
			/// </summary>
			protected override void ClearItems()
			{
				EnsureWritable();
				_byName.Clear();
				base.ClearItems();
			}

			/// <summary>
			/// Replaces the element at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index of the element to replace.</param>
			/// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
			/// <exception cref="T:System.ArgumentOutOfRangeException">index is less than zero.-or-index is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"></see>.</exception>
			protected override void SetItem(int index, T item)
			{
				throw new InvalidOperationException();
				//base.SetItem(index, item);
			}

			/// <summary>
			/// Ensures the writable.
			/// </summary>
			private void EnsureWritable()
			{
				if (_list != null)
					_list.EnsureWritable();
			}

			/// <summary>
			/// Gets the item with the specified name.
			/// </summary>
			/// <value></value>
			public T this[string name]
			{
				get { return _byName[name]; }
			}

			/// <summary>
			/// Tries to get the value with the specified name.
			/// </summary>
			/// <param name="name">The name.</param>
			/// <param name="value">The value.</param>
			/// <returns></returns>
			public bool TryGetValue(string name, out T value)
			{
				return _byName.TryGetValue(name, out value);
			}

			/// <summary>
			/// Determines whether the list contains an item with the specified name.
			/// </summary>
			/// <param name="name">The name.</param>
			/// <returns>
			/// 	<c>true</c> if [contains] [the specified name]; otherwise, <c>false</c>.
			/// </returns>
			public bool Contains(string name)
			{
				return _byName.ContainsKey(name);
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TList">The type of the list.</typeparam>
	/// <typeparam name="TRoot">The type of the root.</typeparam>
	/// <typeparam name="TLeaf">The type of the leaf.</typeparam>
	public abstract class NamedItemSetList<T, TList, TLeaf, TRoot> : ItemSetList<T, TList, TLeaf, TRoot>
		where T : NamedItemSetBase<TRoot>, ILeafEnumerable<TLeaf>, new()
		where TRoot : ItemSetRoot<TRoot>
		where TList : NamedItemSetList<T, TList, TLeaf, TRoot>, new()
		where TLeaf : NamedItemSetBase<TRoot>
	{
		string _name;

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		/// <exception cref="InvalidOperationException">When setting the name if readonly</exception>
		[XmlAttribute("name")]
		public override string Name
		{
			get { return _name; }
			set 
			{
				if (value == _name)
					return;
				else if (_name != null)
					throw new InvalidOperationException();

				EnsureWritable(); 
				_name = value; 
			}
		}
	}
}
