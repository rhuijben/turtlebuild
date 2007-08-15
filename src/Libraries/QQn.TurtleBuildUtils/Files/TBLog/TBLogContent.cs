using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokenizer;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	public class TBLogContent
	{
		[TokenGroup("Item")]
		List<TBLogItem> Items;
	}
}
