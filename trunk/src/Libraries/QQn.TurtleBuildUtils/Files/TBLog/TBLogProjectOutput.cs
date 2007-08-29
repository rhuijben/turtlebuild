using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	public class TBLogProjectOutput : IHasFullPath
	{
		readonly TBReferenceCollection<TBLogItem> _items;

		/// <summary>
		/// Initializes a new instance of the <see cref="TBLogProjectOutput"/> class.
		/// </summary>
		public TBLogProjectOutput()
		{
			_items = new TBReferenceCollection<TBLogItem>(this);
		}

		/// <summary>
		/// 
		/// </summary>
		[TokenGroup("Item", typeof(TBLogItem))]
		[TokenGroup("SharedItem", typeof(TBLogSharedItem))]
		[TokenGroup("CopyItem", typeof(TBLogCopyItem))]
		[TokenGroup("SharedCopyItem", typeof(TBLogSharedCopyItem))]
		public TBLogItemCollection<TBLogItem> Items
		{
			[DebuggerStepThrough]
			get { return _items; }
		}

		IHasFullPath _parent;
		string IHasFullPath.FullPath
		{
			get { return (Parent != null) ? Parent.FullPath : null; }
		}

		internal IHasFullPath Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}
	}

	interface IHasFullPath
	{
		string FullPath { get; }
	}

	class TBReferenceCollection<T> : TBLogItemCollection<T>
		where T : TBLogItem
	{
		readonly IHasFullPath _parent;

		public TBReferenceCollection(IHasFullPath parent)
		{
			_parent = parent;
		}

		protected override void InsertItem(int index, T item)
		{
			base.InsertItem(index, item);
			item.Parent = _parent;
		}

		protected override void SetItem(int index, T item)
		{
			this[index].Parent = null;
			base.SetItem(index, item);
			item.Parent = _parent;
		}

		protected override void RemoveItem(int index)
		{
			this[index].Parent = null;
			base.RemoveItem(index);
		}
	}
}
