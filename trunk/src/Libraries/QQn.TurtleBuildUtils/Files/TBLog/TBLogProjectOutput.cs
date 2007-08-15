using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokenizer;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	public class TBLogProjectOutput
	{
		[TokenGroup("Item", typeof(TBLogItem))]
		[TokenGroup("SharedItem", typeof(TBLogSharedItem))]
		[TokenGroup("CopyItem", typeof(TBLogCopyItem))]
		[TokenGroup("SharedCopyItem", typeof(TBLogSharedCopyItem))]
		public readonly List<TBLogReferenceItem> Items = new List<TBLogReferenceItem>();
	}
}
