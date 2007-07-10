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
	public abstract class ItemSetItem<TRoot, TList, TLeaf> : ItemSetBase<TRoot, TLeaf>
		where TRoot : ItemSetRoot<TRoot>
		where TList : ItemSetList<TLeaf, TList, TRoot, TLeaf>, new()
		where TLeaf : ItemSetItem<TRoot, TList, TLeaf>, new()
	{
		string _name;

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[XmlAttribute("name")]
		public override string Name
		{
			get { return _name; }
			set { EnsureWritable(); _name = value; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		[XmlIgnore]
		public sealed override bool IsReadOnly
		{
			get { return (Package != null) ? Package.IsReadOnly : false; }
		}
	}
}
