using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using QQn.TurtleUtils.IO;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	[DebuggerDisplay("src={Src}, fromSrc={FromSrc}")]
	public class TBLogItem
	{
		TBLogContainer _container;
		string _src;
		string _fromSrc;

		/// <summary>
		/// 
		/// </summary>
		[Token("src")]
		public string Src
		{
			get { return _src; }
			set { _src = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		[Token("fromSrc")]
		public string FromSrc
		{
			get { return _fromSrc; }
			set { _fromSrc = value; }
		}

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
			get { return (_container != null && !string.IsNullOrEmpty(Src)) ? QQnPath.Combine(_container.BasePath, Src) : null; }
		}

		/// <summary>
		/// Gets the full from SRC.
		/// </summary>
		/// <value>The full from SRC.</value>
		public string FullFromSrc
		{
			get { return (_container != null && !string.IsNullOrEmpty(FromSrc)) ? QQnPath.Combine(_container.BasePath, FromSrc) : null; }
		}

		/// <summary>
		/// Gets or sets the container.
		/// </summary>
		/// <value>The container.</value>
		public TBLogContainer Container
		{
			get { return _container; }
			internal set { _container = value; }
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
			: base(StringComparer.OrdinalIgnoreCase, 16)
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
