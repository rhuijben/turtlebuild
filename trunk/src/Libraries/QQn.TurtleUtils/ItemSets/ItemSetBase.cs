using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace QQn.TurtleUtils.ItemSets
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class ItemSetBase
	{
		/// <summary>
		/// 
		/// </summary>
		public abstract bool IsReadOnly
		{
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		protected void EnsureWritable()
		{
			if (IsReadOnly)
				throw new InvalidOperationException(ItemSetStrings.ReadOnly);
		}		
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TPackage"></typeparam>
	/// <typeparam name="TContainer"></typeparam>
	/// <typeparam name="TItem"></typeparam>
	public abstract class ItemSetBase<TPackage, TContainer, TItem> : ItemSetBase
		where TPackage : Package<TPackage, TContainer, TItem>
		where TContainer : Container<TPackage, TContainer, TItem>, new()
		where TItem : Item<TPackage, TContainer, TItem>, new()
	{
		TPackage _package;

		/// <summary>
		/// 
		/// </summary>
		[XmlIgnore]
		public virtual TPackage Package
		{
			get { return _package ?? (this as TPackage); }
			internal set { _package = value; }
		}
	}
}
