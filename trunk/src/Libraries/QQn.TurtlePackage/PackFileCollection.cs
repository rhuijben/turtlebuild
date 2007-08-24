using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace QQn.TurtlePackage
{
	/// <summary>
	/// 
	/// </summary>
	public class PackFileCollection : PackCollection<PackFile>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PackFileCollection"/> class.
		/// </summary>
		/// <param name="parent">The parent.</param>
		public PackFileCollection(PackContainer parent)
			: base(parent)
		{
		}
	}
}
