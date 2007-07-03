using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace QQn.TurtleUtils.ItemSets
{
	[XmlRoot("Container", IsNullable=false)]
	public class Container<TPackage, TContainer, TItem> : ItemSetList<TItem, TPackage, TContainer, TItem>
		where TPackage : Package<TPackage, TContainer, TItem>
		where TContainer : Container<TPackage, TContainer, TItem>, new()
		where TItem : Item<TPackage, TContainer, TItem>, new()
	{
		string _name;
		

		[XmlAttribute("name")]
		public string Name
		{
			get { return _name; }
			set
			{
				if (IsReadOnly)
					throw new InvalidOperationException(ItemSetStrings.ReadOnly);

				_name = value;
			}
		}

		public TItem AddItem(string include, string type)
		{
			if (string.IsNullOrEmpty(include))
				throw new ArgumentNullException("include");
			else if (string.IsNullOrEmpty(type))
				throw new ArgumentNullException("type");

			if (IsReadOnly)
				throw new InvalidOperationException(ItemSetStrings.ReadOnly);

			TItem t = new TItem();
			t.Include = include;
			t.Type = type;
			Add(t);
			return t;
		}

		public TItem AddItem(string include)
		{
			return AddItem(include, "item");
		}

		[XmlIgnore]
		public sealed override bool IsReadOnly
		{
			get { return (Package != null) ? Package.IsReadOnly : false; }
		}
	}

}
