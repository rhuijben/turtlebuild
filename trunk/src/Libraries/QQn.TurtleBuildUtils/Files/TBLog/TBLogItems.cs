using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	[DebuggerDisplay("src={Src}, fromSrc={FromSrc}")]
	public class TBLogItem
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
		public virtual bool IsShared
		{
			get { return false; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is copy.
		/// </summary>
		/// <value><c>true</c> if this instance is copy; otherwise, <c>false</c>.</value>
		public virtual bool IsCopy
		{
			get { return false; }
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
	/// Collection of <see cref="TBLogItem"/> instances, indexed by <see cref="TBLogItem.Src"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TBLogItemCollection<T> : KeyedCollection<string, T>
		where T : TBLogItem
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TBLogItemCollection&lt;T&gt;"/> class.
		/// </summary>
		public TBLogItemCollection()
			: base(StringComparer.InvariantCultureIgnoreCase, 16)
		{
		}

		/// <summary>
		/// Extracts the key from the specified element.
		/// </summary>
		/// <param name="item">The element from which to extract the key.</param>
		/// <returns>The key for the specified element.</returns>
		protected override string GetKeyForItem(T item)
		{
			return item.Src;
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
