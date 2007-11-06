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
	public class TBLogScripts : TBLogContainer
	{
		readonly TBLogItemCollection<TBLogItem> _items;
		/// <summary>
		/// Initializes a new instance of the <see cref="TBLogScripts"/> class.
		/// </summary>
		public TBLogScripts()
		{
			_items = new TBLogItemCollection<TBLogItem>(this);
		}

		/// <summary>
		/// 
		/// </summary>
		[TokenGroup("Item")]
		public TBLogItemCollection<TBLogItem> Items
		{
			[DebuggerStepThrough]
			get { return _items; }
		}
	}
}
