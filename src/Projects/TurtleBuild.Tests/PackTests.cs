using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtlePackage;
using System.Xml;
using QQn.TurtleUtils.Tokens;
using System.Diagnostics;


namespace TurtleTests
{
	[TestFixture]
	public class PackTests
	{

		[Test]
		public void CreatePack()
		{
			Pack pack = new Pack();
			PackContainer po = pack.Containers.AddItem("#ProjectOutput");
			po.Files.AddItem("bin/release/QQnUtils.dll").FileHash = "qwqq";
			po.Files.AddItem("bin/release/QQnUtils.pdb").FileSize = long.MaxValue;
			po.Files.AddItem("bin/release/QQnUtils.xml").LastWriteTimeUtc = DateTime.Now;

			PackContainer pc = pack.Containers.AddItem("#ProjectContent");
			po.Files.AddItem("QQnUtils.doc").FileHash = "q";
			po.Files.AddItem("QQnDef.xsd").FileSize = 123456;

			PackContainer ps = pack.Containers.AddItem("#Scripts");
			ps.Files.AddItem("Setup/QQnUtils.wxs").LastWriteTimeUtc = DateTime.Now;

			XmlDocument doc = new XmlDocument();

			using (XmlWriter xw = doc.CreateNavigator().AppendChild())
			{
				xw.WriteStartElement("Pack", ""); // Place al elements in default namespace for easy quering

				Tokenizer.TryWriteXml(xw, pack);
			}

			Assert.That(doc.SelectSingleNode("/*"), Is.Not.Null);
			Assert.That(doc.SelectNodes("//Item").Count, Is.EqualTo(6));

			Pack pack2;

			Tokenizer.TryParseXml(doc.DocumentElement, out pack2);

			foreach (PackContainer ipc in pack.Containers)
			{
				PackContainer ipc2 = pack2.Containers[ipc.Name];
				Assert.That(ipc2, Is.Not.Null);

				Assert.That(ipc2.Name, Is.EqualTo(ipc.Name));


				foreach (PackFile ipf in ipc.Files)
				{
					PackFile ipf2 = ipc2.Files[ipf.Name];

					Assert.That(ipf2.Name, Is.EqualTo(ipf.Name));
					Assert.That(ipf2.FileHash, Is.EqualTo(ipf.FileHash));
					Assert.That(ipf2.FileSize, Is.EqualTo(ipf.FileSize));
					Assert.That(ipf2.LastWriteTimeUtc, Is.EqualTo(ipf.LastWriteTimeUtc)); // This also checks time normalization
				}

			}


		}
	}
}
