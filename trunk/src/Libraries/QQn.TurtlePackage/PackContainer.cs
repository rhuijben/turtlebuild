using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using QQn.TurtleUtils.ItemSets;


namespace QQn.TurtlePackage
{
	[XmlRoot("Container")]
	public class PackContainer : ItemSetList<PackFile, PackContainer, PackFile, Pack>
	{
		

	}
}
