using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using QQn.TurtleUtils.ItemSets;
using System.Collections.ObjectModel;

namespace QQn.TurtlePackage
{
	public class PackContainerCollection : PackCollection<PackContainer>
	{
		protected internal PackContainerCollection(Pack pack)
			: base(pack)
		{
		}
	}
}
