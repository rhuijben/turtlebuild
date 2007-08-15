using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokenizer;
using System.Diagnostics;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	[DebuggerDisplay("src={Src}, fromSrc={FromSrc}")]
	public abstract class TBLogReferenceItem
	{
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
	public class TBLogSharedItem : TBLogReferenceItem
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

	public class TBLogCopyItem : TBLogReferenceItem
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

	public class TBLogSharedCopyItem : TBLogReferenceItem
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
