using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace QQn.TurtleUtils.ItemSets
{
	public abstract class ItemSetList<T, TPackage, TContainer, TItem> : ItemSetBase<TPackage, TContainer, TItem>, IList<T>, System.Collections.IList
		where T : ItemSetBase<TPackage, TContainer, TItem>, new()
		where TPackage : Package<TPackage, TContainer, TItem>
		where TContainer : Container<TPackage, TContainer, TItem>, new()
		where TItem : Item<TPackage, TContainer, TItem>, new()
	{
		readonly List<T> _innerList = new List<T>();

		protected List<T> InnerList
		{
			get { return _innerList; }
		}

		#region IList<T> Members

		public int IndexOf(T item)
		{
			return InnerList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			if (IsReadOnly)
				throw new InvalidOperationException(ItemSetStrings.ReadOnly);
			else if (item.Package != null)
				throw new InvalidOperationException();

			using (new Inserter(this, item))
			{
				InnerList.Insert(index, item);
			}
		}

		public void RemoveAt(int index)
		{
			if (IsReadOnly)
				throw new InvalidOperationException(ItemSetStrings.ReadOnly);

			T item = this[index];
			InnerList.RemoveAt(index);
		}

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

		public void Clear()
		{
			if (IsReadOnly)
				throw new InvalidOperationException(ItemSetStrings.ReadOnly);

			InnerList.Clear();
		}

		public bool Contains(T item)
		{
			return InnerList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			InnerList.CopyTo(array, arrayIndex);
		}

		[XmlIgnore]
		public int Count
		{
			get { return InnerList.Count; }
		}

		public bool Remove(T item)
		{
			if (IsReadOnly)
				throw new InvalidOperationException(ItemSetStrings.ReadOnly);

			return InnerList.Remove(item);
		}

		#endregion

		#region IEnumerable<T> Members

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
			readonly ItemSetList<T, TPackage, TContainer, TItem> _owner;
			readonly T _item;
			public Inserter(ItemSetList<T, TPackage, TContainer, TItem> owner, T item)
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

		public override TPackage Package
		{
			get
			{
				return base.Package;
			}
			internal set
			{
				base.Package = value;
				foreach (T i in this)
				{
					i.Package = value;
				}
			}
		}
	}
}
