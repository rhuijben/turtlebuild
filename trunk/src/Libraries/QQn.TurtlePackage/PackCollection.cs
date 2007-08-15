using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace QQn.TurtlePackage
{
	public abstract class PackCollection<T> : KeyedCollection<string, T>
		where T : PackItem, new()
	{
		readonly PackItem _parent;

		protected PackCollection()
		{
		}

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

		protected override void InsertItem(int index, T item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			EnsureWritable();
			base.InsertItem(index, item);
			item.Pack = Pack;
		}

		protected override void ClearItems()
		{
			EnsureWritable();
			foreach (T item in this)
			{
				item.Pack = null;
			}
			base.ClearItems();
		}

		protected override void RemoveItem(int index)
		{
			EnsureWritable();
			this[index].Pack = null;
			base.RemoveItem(index);
		}

		protected override void SetItem(int index, T item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			EnsureWritable();
			this[index].Pack = null;
			base.SetItem(index, item);
			this[index].Pack = Pack;
		}

		protected void EnsureWritable()
		{
			if (_parent != null)
				_parent.EnsureWritable();
		}

		public Pack Pack
		{
			get { return (_parent != null) ? _parent.Pack : null; }
		}

		protected override string GetKeyForItem(T item)
		{
			return item.Name;
		}

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
