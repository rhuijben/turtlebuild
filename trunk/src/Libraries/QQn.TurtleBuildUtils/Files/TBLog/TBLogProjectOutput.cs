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
	public class TBLogProjectOutput : TBLogContainer
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
	}

	class TBReferenceCollection<T> : TBLogItemCollection<T>
		where T : TBLogItem
	{
		readonly TBLogContainer _parent;

		public TBReferenceCollection(TBLogContainer parent)
		{
			_parent = parent;
		}

		protected override void InsertItem(int index, T item)
		{
			base.InsertItem(index, item);
			item.Container = _parent;
		}

		protected override void SetItem(int index, T item)
		{
			this[index].Container = null;
			base.SetItem(index, item);
			item.Container = _parent;
		}

		protected override void RemoveItem(int index)
		{
			this[index].Container = null;
			base.RemoveItem(index);
		}
	}
}
