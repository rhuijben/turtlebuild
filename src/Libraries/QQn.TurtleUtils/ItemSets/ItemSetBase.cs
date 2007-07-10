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
	public abstract class ItemSetBase<TRoot>
		where TRoot : ItemSetRoot<TRoot>
	{
		TRoot _package;

		/// <summary>
		/// Gets or sets the package.
		/// </summary>
		/// <value>The package.</value>
		[XmlIgnore]
		public virtual TRoot Package
		{
			get { return _package ?? (this as TRoot); }
			internal set { _package = value; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public abstract bool IsReadOnly
		{
			get;
		}

		/// <summary>
		/// Ensures the set is writable.
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
	/// <typeparam name="TRoot">The type of the root.</typeparam>
	public abstract class NamedItemSetBase<TRoot> : ItemSetBase<TRoot>
		where TRoot : ItemSetRoot<TRoot>
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[XmlAttribute("name")]
		public abstract string Name
		{
			get;
			set;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TPackage"></typeparam>
	/// <typeparam name="TContainer"></typeparam>
	/// <typeparam name="TItem"></typeparam>
	public abstract class ItemSetBase<TRoot, TLeaf> : NamedItemSetBase<TRoot>
		where TRoot : ItemSetRoot<TRoot>
		where TLeaf : ItemSetBase<TRoot, TLeaf>, new()
	{
		
	}
}
