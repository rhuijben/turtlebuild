using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Collections.ObjectModel;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	public class TBLogProjectOutput
	{
		[TokenGroup("Item", typeof(TBLogItem))]
		[TokenGroup("SharedItem", typeof(TBLogSharedItem))]
		[TokenGroup("CopyItem", typeof(TBLogCopyItem))]
		[TokenGroup("SharedCopyItem", typeof(TBLogSharedCopyItem))]
		public readonly Collection<TBLogReferenceItem> Items = new Collection<TBLogReferenceItem>();
	}
}
