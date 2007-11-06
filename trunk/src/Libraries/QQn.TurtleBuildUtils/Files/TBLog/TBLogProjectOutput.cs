using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Collections.ObjectModel;
using System.Diagnostics;
using QQn.TurtleUtils.Items;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	public class TBLogProjectOutput : TBLogContainer
	{
		readonly TBLogItemCollection<TBLogItem> _items;

		/// <summary>
		/// Initializes a new instance of the <see cref="TBLogProjectOutput"/> class.
		/// </summary>
		public TBLogProjectOutput()
		{
			_items = new TBLogItemCollection<TBLogItem>(this);
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
}
