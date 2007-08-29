using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace QQn.TurtlePackage
{
	/// <summary>
	/// 
	/// </summary>
	public class PackContainerCollection : PackCollection<PackContainer>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PackContainerCollection"/> class.
		/// </summary>
		/// <param name="pack">The pack.</param>
		protected internal PackContainerCollection(Pack pack)
			: base(pack)
		{
		}
	}
}
