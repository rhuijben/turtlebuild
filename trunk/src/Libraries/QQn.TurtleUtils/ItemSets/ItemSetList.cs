using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Collections;

namespace QQn.TurtleUtils.ItemSets
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TList">The type of the list.</typeparam>
	/// <typeparam name="TRoot">The type of the root.</typeparam>
	/// <typeparam name="TLeaf">The type of the leaf.</typeparam>
	public abstract class ItemSetList<T, TList, TRoot, TLeaf> : ItemSetBase<TRoot>, IList<T>, System.Collections.IList, IDictionary<string, T>, IDictionary
		where T : NamedItemSetBase<TRoot>, new()
		where TRoot : ItemSetRoot<TRoot>
		where TList : ItemSetList<T, TList, TRoot, TLeaf>, new()
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
			if (IsReadOnly)
				throw new InvalidOperationException(ItemSetStrings.ReadOnly);

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
			set
			{
				if (IsReadOnly)
					throw new InvalidOperationException(ItemSetStrings.ReadOnly);
				else if (value == null)
					throw new ArgumentNullException("value");
				else if (value.Package != null)
					throw new InvalidOperationException();

				T item = this[index];
				using (new Inserter(this, item))
				{
					InnerList[index] = value;
				}
			}
		}

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

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region IList Members

		int System.Collections.IList.Add(object value)
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
			set
			{
				this[index] = (T)value;
			}
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
		}

		#endregion

		sealed class Inserter : IDisposable
		{
			readonly ItemSetList<T, TList, TRoot, TLeaf> _owner;
			readonly T _item;
			public Inserter(ItemSetList<T, TList, TRoot, TLeaf> owner, T item)
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
			internal set
			{
				EnsureWritable();
				base.Package = value;
				foreach (T i in this)
				{
					i.Package = value;
				}
			}
		}

		#region IDictionary<string,T> Members

		void IDictionary<string, T>.Add(string key, T value)
		{
			if (ContainsKey(key) || value.Name != key)
				throw new ArgumentException("Invalid key", "key");

			Add(value);
		}

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

		T IDictionary<string, T>.this[string key]
		{
			get { return this[key]; }			
			set
			{
				throw new InvalidOperationException();
			}
		}

		#endregion

		#region ICollection<KeyValuePair<string,T>> Members

		void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item)
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
		}

		#endregion

		#region IEnumerable<KeyValuePair<string,T>> Members

		IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
		{
			return InnerSortedList.GetEnumerator();
		}

		#endregion

		#region IDictionary Members

		void IDictionary.Add(object key, object value)
		{
			Add((T)value);
		}

		bool IDictionary.Contains(object key)
		{
			return Contains(key as T);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return ((IDictionary)InnerSortedList).GetEnumerator();
		}

		bool IDictionary.IsFixedSize
		{
			get { return false; }
		}

		ICollection IDictionary.Keys
		{
			get { return (ICollection)Keys; }
		}

		void IDictionary.Remove(object key)
		{
			T value;
			if (InnerSortedList.TryGetValue((string)key, out value))
				Remove(value);
		}

		ICollection IDictionary.Values
		{
			get { return (ICollection)Values; }
		}

		object IDictionary.this[object key]
		{
			get
			{
				T value;
				if (InnerSortedList.TryGetValue((string)key, out value))
					return value;
				else
					return null;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		#endregion
	}
}
