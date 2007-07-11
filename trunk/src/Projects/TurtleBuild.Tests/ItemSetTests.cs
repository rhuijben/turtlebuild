using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtleUtils.ItemSets;
using System.Xml;
using System.Xml.Serialization;

namespace TurtleTests
{
	[TestFixture]
	public class ItemSetTests
	{

		[Test]
		public void CreatePackage()
		{
			MyPackage pkg = new MyPackage();

			MyContainer container = pkg.Containers.AddItem("bin");
			container.AddItem("bin/release/MyApp.dll");
			container.AddItem("bin/release/MyApp.xml");
			container.AddItem("bin/release/MyApp.pdb");
			XmlDocument doc = new XmlDocument();

			using (XmlWriter xw = doc.CreateNavigator().AppendChild())
			{
				pkg.WriteTo(xw);
			}

			System.Diagnostics.Debug.WriteLine(doc.DocumentElement.OuterXml);
		}
	}

	[XmlRoot("container")]
	public class MyContainer : NamedItemSetList<MyItem, MyContainer, MyItem, MyPackage>
	{
		[XmlAttribute("q")]
		public string Q = "a";
	}

	[XmlRoot("item")]
	public class MyItem : ItemSetItem<MyContainer, MyItem, MyPackage>
	{
		[XmlAttribute("q")]
		public string Q = "b";
	}

	[XmlRoot("package", Namespace="qq")]
	public class MyPackage : ItemSetRoot<MyPackage>
	{
		[XmlAttribute("q")]
		public string Q = "c";

		//[XmlArrayItem("Container", typeof(MyContainer))]
		public MyContainerList Containers = new MyContainerList();
		
	}

	[XmlRoot("Containers")]
	public class MyContainerList : ItemSetList<MyContainer, MyContainerList, MyItem, MyPackage>
	{
		[XmlAttribute("q")]
		public string Q = "d";
	}
}
