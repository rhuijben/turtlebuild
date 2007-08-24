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
	public class TBLogContent : IHasFullPath
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TBLogContent"/> class.
		/// </summary>
		public TBLogContent()
		{
			 Items = new TBReferenceCollection<TBLogItem>(this);
		}

		/// <summary>
		/// 
		/// </summary>
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
