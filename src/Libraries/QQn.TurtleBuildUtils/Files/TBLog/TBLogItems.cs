using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Diagnostics;
using System.IO;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	[DebuggerDisplay("src={Src}, fromSrc={FromSrc}")]
	public abstract class TBLogReferenceItem
	{
		IHasFullPath _parent;

		[Token("src")]
		public string Src;

		[Token("fromSrc")]
		public string FromSrc;

		public abstract bool IsShared
		{
			get;
		}

		public abstract bool IsCopy
		{
			get;
		}

		public string FullSrc
		{
			get { return _parent != null ? Path.Combine(_parent.FullPath, Src) : null; }
		}

		public string FullFromSrc
		{
			get { return _parent != null ? Path.Combine(_parent.FullPath, Src) : null; }
		}

		internal IHasFullPath Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}
	}

	public class TBLogItem : TBLogReferenceItem
	{
		public override bool IsShared
		{
			get { return false; }
		}

		public override bool IsCopy
		{
			get { return false; }
		}
	}
	public class TBLogSharedItem : TBLogItem
	{
		public override bool IsShared
		{
			get { return true; }
		}

		public override bool IsCopy
		{
			get { return false; }
		}
	}

	public class TBLogCopyItem : TBLogItem
	{
		public override bool IsShared
		{
			get { return false; }
		}

		public override bool IsCopy
		{
			get { return true; }
		}
	}

	public class TBLogSharedCopyItem : TBLogItem
	{
		public override bool IsShared
		{
			get { return true; }
		}

		public override bool IsCopy
		{
			get { return true; }
		}
	}
}
