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
			get { return _package ?? (_package = (this as TRoot)); }
			set 
			{
				if (value == _package)
					return;
				else if ((_package != null) && (_package.Package != null))
					throw new InvalidOperationException();
				_package = value;
			}
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

		/// <summary>
		/// Initializes and sets the name of the item.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <remarks>Called from List.AddItem</remarks>
		protected internal virtual void InitializeAndSetName(string name)
		{
			Name = name;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TLeaf">The type of the leaf.</typeparam>
	/// <typeparam name="TRoot">The type of the root.</typeparam>
	public abstract class ItemSetBase<TLeaf, TRoot> : NamedItemSetBase<TRoot>
		where TRoot : ItemSetRoot<TRoot>
		where TLeaf : ItemSetBase<TLeaf, TRoot>, new()
	{
		
	}
}
