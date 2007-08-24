using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Collections.ObjectModel;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	public class TBLogProjectOutput : IHasFullPath
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TBLogProjectOutput"/> class.
		/// </summary>
		public TBLogProjectOutput()
		{
			Items = new TBReferenceCollection<TBLogReferenceItem>(this);
		}

		/// <summary>
		/// 
		/// </summary>
		[TokenGroup("Item", typeof(TBLogItem))]
		[TokenGroup("SharedItem", typeof(TBLogSharedItem))]
		[TokenGroup("CopyItem", typeof(TBLogCopyItem))]
		[TokenGroup("SharedCopyItem", typeof(TBLogSharedCopyItem))]
		public readonly Collection<TBLogReferenceItem> Items;

		IHasFullPath _parent;
		string IHasFullPath.FullPath
		{
			get { return (_parent != null) ? _parent.FullPath : null; }
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

	class TBReferenceCollection<T> : Collection<T>
		where T : TBLogReferenceItem
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
