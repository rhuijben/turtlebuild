using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using QQn.TurtleUtils.ItemSets;


namespace QQn.TurtlePackage
{
	[XmlRoot("TurtlePackage", Namespace=Pack.Ns)]
	public class Pack : ItemSetRoot<Pack>
	{
		internal const string Ns = "http://schemas.qqn.nl/2007/TurtlePackage";
		PackContainerList _containerList;

		[XmlArray("Containers")]
		public PackContainerList Containers
		{
			get { return _containerList; }
			set { EnsureWritable(); _containerList = value; value.Package = this;  }
		}
	}
}
