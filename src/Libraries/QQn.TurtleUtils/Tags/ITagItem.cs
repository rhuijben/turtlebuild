using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	public interface ITagItem
	{		
		/// <summary>
		/// Gets or sets the item specification.
		/// </summary>
		/// <value>The item specification</value>
		string ItemSpec { get; set; }		
		/// <summary>
		/// Gets the number of metadata entries associated with the item.
		/// </summary>
		/// <value>The number of metadata entries associated with the item..</value>
		int MetadataCount { get; }
		
		/// <summary>
		/// Gets the name of the TagItemKeys
		/// </summary>
		ICollection<string> KeyNames { get; }

		/// <summary>
		/// Gets or sets the <see cref="System.String"/> with the specified key name.
		/// </summary>
		/// <value></value>
		string this[string keyName]
		{
			get;
			set;
		}

		/// <summary>
		/// Removes the key.
		/// </summary>
		/// <param name="keyName">Name of the key.</param>
		void RemoveKey(string keyName);		
	}
}
