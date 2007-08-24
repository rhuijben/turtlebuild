using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Diagnostics;
using System.IO;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	[DebuggerDisplay("src={Src}, fromSrc={FromSrc}")]
	public abstract class TBLogReferenceItem
	{
		IHasFullPath _parent;

		/// <summary>
		/// 
		/// </summary>
		[Token("src")]
		public string Src;

		/// <summary>
		/// 
		/// </summary>
		[Token("fromSrc")]
		public string FromSrc;

		/// <summary>
		/// Gets a value indicating whether this instance is shared.
		/// </summary>
		/// <value><c>true</c> if this instance is shared; otherwise, <c>false</c>.</value>
		public abstract bool IsShared
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is copy.
		/// </summary>
		/// <value><c>true</c> if this instance is copy; otherwise, <c>false</c>.</value>
		public abstract bool IsCopy
		{
			get;
		}

		/// <summary>
		/// Gets the full SRC.
		/// </summary>
		/// <value>The full SRC.</value>
		public string FullSrc
		{
			get { return _parent != null ? Path.Combine(_parent.FullPath, Src) : null; }
		}

		/// <summary>
		/// Gets the full from SRC.
		/// </summary>
		/// <value>The full from SRC.</value>
		public string FullFromSrc
		{
			get { return _parent != null ? Path.Combine(_parent.FullPath, Src) : null; }
		}

		/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		/// <value>The parent.</value>
		internal IHasFullPath Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class TBLogItem : TBLogReferenceItem
	{
		/// <summary>
		/// Gets a value indicating whether this instance is shared.
		/// </summary>
		/// <value><c>true</c> if this instance is shared; otherwise, <c>false</c>.</value>
		public override bool IsShared
		{
			get { return false; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is copy.
		/// </summary>
		/// <value><c>true</c> if this instance is copy; otherwise, <c>false</c>.</value>
		public override bool IsCopy
		{
			get { return false; }
		}
	}
	/// <summary>
	/// 
	/// </summary>
	public class TBLogSharedItem : TBLogItem
	{
		/// <summary>
		/// Gets a value indicating whether this instance is shared.
		/// </summary>
		/// <value><c>true</c> if this instance is shared; otherwise, <c>false</c>.</value>
		public override bool IsShared
		{
			get { return true; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is copy.
		/// </summary>
		/// <value><c>true</c> if this instance is copy; otherwise, <c>false</c>.</value>
		public override bool IsCopy
		{
			get { return false; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class TBLogCopyItem : TBLogItem
	{
		/// <summary>
		/// Gets a value indicating whether this instance is shared.
		/// </summary>
		/// <value><c>true</c> if this instance is shared; otherwise, <c>false</c>.</value>
		public override bool IsShared
		{
			get { return false; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is copy.
		/// </summary>
		/// <value><c>true</c> if this instance is copy; otherwise, <c>false</c>.</value>
		public override bool IsCopy
		{
			get { return true; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class TBLogSharedCopyItem : TBLogItem
	{
		/// <summary>
		/// Gets a value indicating whether this instance is shared.
		/// </summary>
		/// <value><c>true</c> if this instance is shared; otherwise, <c>false</c>.</value>
		public override bool IsShared
		{
			get { return true; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is copy.
		/// </summary>
		/// <value><c>true</c> if this instance is copy; otherwise, <c>false</c>.</value>
		public override bool IsCopy
		{
			get { return true; }
		}
	}
}
