using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtlePackage;
using System.IO;
using System.Reflection;
using System.Xml;
using QQn.TurtlePackage.StreamFiles;
using NUnit.Framework.SyntaxHelpers;
using QQn.TurtleUtils.Streams;

namespace TurtleTests
{
	[TestFixture]
	public class StreamTests
	{
		[Test]
		public void TestStreams()
		{
			string fileName = Path.GetTempFileName()+"q";
			using (StreamFile sf = new StreamFile(fileName, 16))
			{
				for (int i = 0; i < 10; i++)
				{
					using (Stream s = sf.CreateNextStream())
					{
						using (StreamWriter sw = new StreamWriter(s))
						{
							sw.WriteLine("This is stream {0}", i);
						}
					}
				}
			}

			using (StreamFile sf = StreamFile.Open(fileName))
			{
				for (int i = 0; i < 10; i++)
				{
					using (Stream s = sf.GetNextStream())
					{
						using (StreamReader sw = new StreamReader(s))
						{
							string line = sw.ReadLine();
							string shouldBe = string.Format("This is stream {0}", i);

							Assert.That(line, Is.EqualTo(shouldBe));
						}
					}
				}
			}
		}

		[Test]
		public void TestMultiStreamWriter()
		{
			string fileName = Path.GetTempFileName() + "q";
			using (FileStream fs = File.Create(fileName))
			using (MultiStreamWriter writer = new MultiStreamWriter(fs))
			{
				for (int i = 0; i < 10; i++)
				{
					using (Stream s = writer.CreateStream())
					{
						using (StreamWriter sw = new StreamWriter(s))
						{
							sw.WriteLine("This is stream {0}", i);
						}
					}
				}
			}

			using (FileStream fs = File.OpenRead(fileName))
			using(MultiStreamReader reader = new MultiStreamReader(fs))
			{
				for (int i = 0; i < 10; i++)
				{
					using (Stream s = reader.GetNextStream())
					{
						using (StreamReader sw = new StreamReader(s))
						{
							string line = sw.ReadLine();
							string shouldBe = string.Format("This is stream {0}", i);

							Assert.That(line, Is.EqualTo(shouldBe));
						}
					}
				}
			}
		}


		[Test]
		public void CreatePackage()
		{
			XmlDocument minimanifest = new XmlDocument();
			minimanifest.LoadXml("<my-mini-manifest />");
			TurtlePackage pkg = TurtlePackage.CreateNew(Path.GetTempFileName(), minimanifest);

			TurtleContainer container = pkg.AddContainer("bin");
			Assembly a = Assembly.GetExecutingAssembly();

			Uri codeBase = new Uri(a.CodeBase);

			container.AddFile(codeBase.LocalPath, Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"../..")));

			XmlDocument doc = new XmlDocument();

			doc.LoadXml("<my-xml />");

			//pkg.SetManifest(doc);

		//	pkg.Commit();
		}
	}
}
