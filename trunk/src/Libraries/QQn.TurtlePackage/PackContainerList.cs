using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using QQn.TurtleUtils.ItemSets;

namespace QQn.TurtlePackage
{
	[XmlRoot("Containers")]
	public class PackContainerList : ItemSetList<PackContainer, PackContainerList, PackFile, Pack>
	{
	}
}
