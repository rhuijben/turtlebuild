using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Collections.ObjectModel;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	public class TBLogContent
	{
		[TokenGroup("Item")]
		public readonly Collection<TBLogItem> Items = new Collection<TBLogItem>();
	}
}
