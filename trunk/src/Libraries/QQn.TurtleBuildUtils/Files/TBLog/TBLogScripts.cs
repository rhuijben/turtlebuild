using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Collections.ObjectModel;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	public class TBLogScripts : IHasFullPath
	{
		public TBLogScripts()
		{
			 Items = new TBReferenceCollection<TBLogItem>(this);
		}

		[TokenGroup("Item")]
		public readonly Collection<TBLogItem> Items;

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
}
