using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace QQn.TurtleUtils.ItemSets
{
	public interface ILeafEnumerable<TLeaf>
	{
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
		readonly SortedList<string, T> _sortedList = new SortedList<string, T>();
		readonly List<T> _innerList = new List<T>();

		/// <summary>
		/// Gets the inner list.
		/// </summary>
		/// <value>The inner list.</value>
		protected List<T> InnerList
		{
			get { return _innerList; }
		}

		/// <summary>
		/// Gets the inner list.
		/// </summary>
		/// <value>The inner list.</value>
		protected SortedList<string, T> InnerSortedList 
		{
			get { return _sortedList; }
		}

		#region IList<T> Members

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf(T item)
		{
			return InnerList.IndexOf(item);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void Insert(int index, T item)
		{
			if (IsReadOnly)
				throw new InvalidOperationException(ItemSetStrings.ReadOnly);
			else if (item.Package != null)
				throw new InvalidOperationException();

			using (new Inserter(this, item))
			{
				InnerSortedList.Add(item.Name, item);
				InnerList.Insert(index, item);				
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{
			EnsureWritable();

			T item = this[index];
			InnerList.RemoveAt(index);
			InnerSortedList.Remove(item.Name);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public T this[int index]
		{
			get { return InnerList[index]; }			
		}

		/*T IList<T>.this[int index]
		{
			get { return this[index]; }
			set { throw new InvalidOperationException(); }
		}*/

		#endregion

		#region ICollection<T> Members

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		public void Add(T item)
		{
			if (IsReadOnly)
				throw new InvalidOperationException(ItemSetStrings.ReadOnly);
			else if (item == null)
				throw new ArgumentNullException("item");
			else if (item.Package != null)
				throw new InvalidOperationException();

			using (new Inserter(this, item))
			{
				InnerList.Add(item);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			if (IsReadOnly)
				throw new InvalidOperationException(ItemSetStrings.ReadOnly);

			InnerList.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(T item)
		{
			return InnerList.Contains(item);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo(T[] array, int arrayIndex)
		{
			InnerList.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// 
		/// </summary>
		[XmlIgnore]
		public int Count
		{
			get { return InnerList.Count; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove(T item)
		{
			if (IsReadOnly)
				throw new InvalidOperationException(ItemSetStrings.ReadOnly);

			return InnerList.Remove(item);
		}

		#endregion

		#region IEnumerable<T> Members

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			return InnerList.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		/*System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}*/

		#endregion

		#region IList Members

		/*int System.Collections.IList.Add(object value)
		{
			T tValue = (T)value;

			Add(tValue);
			return IndexOf(tValue);
		}

		bool System.Collections.IList.Contains(object value)
		{
			return Contains((T)value);
		}

		int System.Collections.IList.IndexOf(object value)
		{
			return IndexOf((T)value);
		}

		void System.Collections.IList.Insert(int index, object value)
		{
			Insert(index, (T)value);
		}

		bool System.Collections.IList.IsFixedSize
		{
			get { return false; }
		}

		void System.Collections.IList.Remove(object value)
		{
			Remove((T)value);
		}

		object System.Collections.IList.this[int index]
		{
			get { return this[index]; }
			set { throw new InvalidOperationException(); }
		}

		#endregion

		#region ICollection Members

		void System.Collections.ICollection.CopyTo(Array array, int index)
		{
			((System.Collections.ICollection)InnerList).CopyTo(array, index);
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			get { return false; }
		}

		object System.Collections.ICollection.SyncRoot
		{
			get { return _innerList; }
		}*/

		#endregion

		sealed class Inserter : IDisposable
		{
			readonly ItemSetList<T, TList, TLeaf, TRoot> _owner;
			readonly T _item;
			public Inserter(ItemSetList<T, TList, TLeaf, TRoot> owner, T item)
			{
				_owner = owner;
				_item = item;

				if(item.Package != null && (owner.Package != item.Package))
					throw new ArgumentException(ItemSetStrings.CantMoveBetweenPackages);
			}

			#region IDisposable Members

			public void Dispose()
			{
				if (_item.Package == null)
					_item.Package = _owner.Package;
				else if (_item.Package != _owner.Package)
					throw new InvalidOperationException();
			}

			#endregion
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
				foreach (T i in this)
				{
					i.Package = value;
				}
			}
		}

		#region IDictionary<string,T> Members

		/*void IDictionary<string, T>.Add(string key, T value)
		{
			if (ContainsKey(key) || value.Name != key)
				throw new ArgumentException("Invalid key", "key");

			Add(value);
		}*/

		public bool ContainsKey(string key)
		{
			return InnerSortedList.ContainsKey(key);
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
		/// </summary>
		/// <value></value>
		/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
		public ICollection<string> Keys
		{
			get { return InnerSortedList.Keys; }
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		/// true if the element is successfully removed; otherwise, false.  This method also returns false if key was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
		/// <exception cref="T:System.ArgumentNullException">key is null.</exception>
		public bool Remove(string key)
		{
			T value;
			if (InnerSortedList.TryGetValue((string)key, out value))
			{
				Remove(value);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Tries the get value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public bool TryGetValue(string key, out T value)
		{
			return InnerSortedList.TryGetValue(key, out value);
		}

		public ICollection<T> Values
		{
			get { return InnerList.AsReadOnly(); }
		}

		/// <summary>
		/// Gets the <see cref="T"/> with the specified key.
		/// </summary>
		/// <value></value>
		public T this[string key]
		{
			get { return InnerSortedList[key]; }
		}

		/*T IDictionary<string, T>.this[string key]
		{
			get { return this[key]; }			
			set
			{
				throw new InvalidOperationException();
			}
		}*/

		#endregion

		#region ICollection<KeyValuePair<string,T>> Members

		/*void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item)
		{
			Add(item.Value);
		}

		bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> item)
		{
			return ((ICollection<KeyValuePair<string, T>>)InnerSortedList).Contains(item);
		}

		void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, T>>)InnerSortedList).CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
		{
			return Remove(item.Value);
		}*/

		#endregion

		#region IEnumerable<KeyValuePair<string,T>> Members

		/*IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
		{
			return InnerSortedList.GetEnumerator();
		}*/

		#endregion

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
		/// Adds the item.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The added item</returns>
		/// <exception cref="ArgumentException">An item with the specified name exists</exception>
		public T AddItem(string name)
		{
			if (ContainsKey(name))
				throw new ArgumentException("Key exists", "key");

			T item = new T();
			item.InitializeAndSetName(name);

			Add(item);
			return item;
		}

		/// <summary>
		/// Gets all leaves.
		/// </summary>
		/// <value>All leaves.</value>
		public IEnumerable<TLeaf> AllLeaves
		{
			get
			{
				foreach (T item in this)
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
		/// [Xml Serialization helper property]
		/// </summary>
		[XmlElement("item"), Browsable(false), DebuggerNonUserCode]
		public List<T> Items
		{
			get 
			{ 
				return new List<T>(InnerList);; 
			}
			set
			{
				EnsureWritable();
				if (value != null)
				{
					InnerList.AddRange(value);
					foreach (T item in value)
					{
						InnerSortedList.Add(item.Name, item);
					}
				}
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
