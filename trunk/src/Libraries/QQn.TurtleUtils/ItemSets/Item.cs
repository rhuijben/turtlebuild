using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace QQn.TurtleUtils.ItemSets
{
	[XmlRoot("Item", IsNullable = false)]
	public class Item<TPackage, TContainer, TItem> : ItemSetBase<TPackage, TContainer, TItem>
		where TPackage : Package<TPackage, TContainer, TItem>
		where TContainer : Container<TPackage, TContainer, TItem>, new()
		where TItem : Item<TPackage, TContainer, TItem>, new()
	{
		string _include;
		string _type;

		[XmlAttribute("include")]
		public string Include
		{
			get { return _include; }
			set
			{
				if (IsReadOnly)
					throw new InvalidOperationException(ItemSetStrings.ReadOnly);
				_include = value;
			}
		}

		[XmlAttribute("type"), DefaultValue("Item")]
		public string Type
		{
			get { return _type; }
			set
			{
				if (IsReadOnly)
					throw new InvalidOperationException(ItemSetStrings.ReadOnly);
				_type = value;
			}
		}

		[XmlIgnore]
		public sealed override bool IsReadOnly
		{
			get { return (Package != null) ? Package.IsReadOnly : false; }
		}
	}
}
