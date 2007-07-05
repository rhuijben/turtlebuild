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

			MyContainer container = pkg.AddContainer("bin");
			container.AddItem("bin/release/MyApp.dll");
			container.AddItem("bin/release/MyApp.xml");
			container.AddItem("bin/release/MyApp.pdb");
			XmlDocument doc = new XmlDocument();

			using (XmlWriter xw = doc.CreateNavigator().AppendChild())
			{
				pkg.WriteTo(xw);
			}

			System.Diagnostics.Debug.WriteLine(doc.DocumentElement.ToString());
		}
	}

	[XmlRoot("container")]
	public class MyContainer : Container<MyPackage, MyContainer, MyItem>
	{
		
	}

	[XmlRoot("item")]
	public class MyItem : Item<MyPackage, MyContainer, MyItem>
	{
	}

	[XmlRoot("package")]
	public class MyPackage : Package<MyPackage, MyContainer, MyItem>
	{
	}
}
