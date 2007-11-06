using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace QQn.TurtleUtils.Items
{
	/// <summary>
	/// Generic collection contained item
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ICollectionItem<T>
	{
		/// <summary>
		/// Gets or sets the list.
		/// </summary>
		/// <value>The list.</value>
		Collection<T> Collection { get; set; }
	}
}
