using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtlePackage;
using System.IO;
using System.Reflection;
using System.Xml;
using QQn.TurtleUtils.IO;
using System.Net;
using System.Net.Cache;
using NUnit.Framework.SyntaxHelpers;

namespace TurtleTests
{
	[TestFixture]
	public class StreamTests
	{
		[Test]
		public void TestMultiStreamWriter()
		{
			string fileName = Path.GetTempFileName() + "q";
			using (FileStream fs = File.Create(fileName))
			using (MultipleStreamWriter writer = new MultipleStreamWriter(fs))
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

				using (MultipleStreamWriter subWriter = new MultipleStreamWriter(writer.CreateStream()))
				{
					for (int i = 0; i < 10; i++)
					{
						using (Stream s = subWriter.CreateStream())
						{
							using (StreamWriter sw = new StreamWriter(s))
							{
								sw.WriteLine("This is stream x-{0}", fileName);
							}
						}
					}
				}
			}

			using (FileStream fs = File.OpenRead(fileName))
			using(MultipleStreamReader reader = new MultipleStreamReader(fs))
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


		/*[Test]
		public void CreatePackage()
		{
			XmlDocument minimanifest = new XmlDocument();
			minimanifest.LoadXml("<my-mini-manifest />");
			TurtlePackage pkg = TurtlePackage.CreateNew(Path.GetTempFileName(), minimanifest);

			TurtleContainer container = pkg.AddContainer("bin");
			Assembly a = Assembly.GetExecutingAssembly();

			Uri codeBase = new Uri(a.CodeBase);

			container.AddFile(codeBase.LocalPath, Path.GetFullPath(QQnPath.Combine(Directory.GetCurrentDirectory(),"../..")));

			XmlDocument doc = new XmlDocument();

			doc.LoadXml("<my-xml />");

			//pkg.SetManifest(doc);

		//	pkg.Commit();
		}*/

		[Test]
		public void ReadStatic()
		{
			for (int i = 0; i < 2; i++)
			{
				WebRequest request = WebRequest.Create(new Uri("http://qqn.nl/~bert/upnp.zip"));
				HttpWebRequest wr = request as HttpWebRequest;

				if (wr != null)
				{
					wr.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);
				}
				using (WebResponse response = request.GetResponse())
				{
					using (Stream s = response.GetResponseStream())
					{
						if (i > 1)
							Assert.That(s.CanSeek, Is.True, "Can seek on second request");
						byte[] buffer = new byte[8192];

						while (0 != s.Read(buffer, 0, buffer.Length))
						{

						}
						s.ReadByte();
					}
				}
			}
		}

		[Test]
		public void ReadStaticFile()
		{
			HttpWebRequest.DefaultCachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate);

			WebRequest request = WebRequest.Create(new Uri("file:///f:/fout.swf"));
			HttpWebRequest wr = request as HttpWebRequest;

			if (wr != null)
			{
				wr.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);
			}
			using (WebResponse response = request.GetResponse())
			{
				using (Stream s = response.GetResponseStream())
				{
					byte[] buffer = new byte[8192];

					while (0 != s.Read(buffer, 0, buffer.Length))
					{
						Assert.That(s.CanSeek, Is.True, "Can seek");
					}
					s.ReadByte();
				}
			}
		}
	}
}
