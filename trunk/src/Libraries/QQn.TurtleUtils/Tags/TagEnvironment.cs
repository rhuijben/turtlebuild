using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	public class TagEnvironment : TagContext
	{
		readonly TagPropertyCollection _properties;
		readonly TagItemCollection _items;

		/// <summary>
		/// Initializes a new instance of the <see cref="TagEnvironment"/> class.
		/// </summary>
		public TagEnvironment()
		{
			_properties = new TagPropertyCollection(this);
			_items = new TagItemCollection(this);
		}

		/// <summary>
		/// Gets the properties collection
		/// </summary>
		/// <value>The properties collection.</value>
		[TokenGroup("property")]
		public TagPropertyCollection Properties
		{
			get { return _properties; }
		}

		/// <summary>
		/// Gets the items collection
		/// </summary>
		/// <value>The items collection.</value>
		[TokenGroup("item")]
		public TagItemCollection Items
		{
			get { return _items; }
		}
	}
}
