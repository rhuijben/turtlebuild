using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace QQn.TurtleUtils.ItemSets
{
	public abstract class ItemSetBase
	{
		public abstract bool IsReadOnly
		{
			get;
		}

		protected void EnsureWritable()
		{
			if (IsReadOnly)
				throw new InvalidOperationException(ItemSetStrings.ReadOnly);
		}		
	}

	public abstract class ItemSetBase<TPackage, TContainer, TItem> : ItemSetBase
		where TPackage : Package<TPackage, TContainer, TItem>
		where TContainer : Container<TPackage, TContainer, TItem>, new()
		where TItem : Item<TPackage, TContainer, TItem>, new()
	{
		TPackage _package;

		[XmlIgnore]
		public virtual TPackage Package
		{
			get { return _package ?? (this as TPackage); }
			internal set { _package = value; }
		}
	}
}
