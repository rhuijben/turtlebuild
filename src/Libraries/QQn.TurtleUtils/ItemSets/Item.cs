using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace QQn.TurtleUtils.ItemSets
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TPackage"></typeparam>
	/// <typeparam name="TContainer"></typeparam>
	/// <typeparam name="TItem"></typeparam>
	[XmlRoot("Item", IsNullable = false)]
	public class Item<TPackage, TContainer, TItem> : ItemSetBase<TPackage, TContainer, TItem>
		where TPackage : Package<TPackage, TContainer, TItem>
		where TContainer : Container<TPackage, TContainer, TItem>, new()
		where TItem : Item<TPackage, TContainer, TItem>, new()
	{
		string _include;
		string _type;

		/// <summary>
		/// 
		/// </summary>
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

		/// <summary>
		/// 
		/// </summary>
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

		/// <summary>
		/// 
		/// </summary>
		[XmlIgnore]
		public sealed override bool IsReadOnly
		{
			get { return (Package != null) ? Package.IsReadOnly : false; }
		}
	}
}
