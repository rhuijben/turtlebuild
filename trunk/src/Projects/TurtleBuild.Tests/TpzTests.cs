using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using QQn.TurtleUtils.IO;
using QQn.TurtleUtils.Cryptography;

namespace TurtleTests
{
	[TestFixture]
	public class TpzTests
	{
		string _snkFile;
		string SnkFile
		{
			get
			{
				if (_snkFile == null)
				{
					_snkFile = QQnPath.Combine(Environment.CurrentDirectory, "..\\..\\..\\..\\Libraries\\QQn.TurtleUtils\\QQn.TurtleUtils.snk");

					Assert.That(File.Exists(_snkFile), "Snk file exists");
				}
				return _snkFile;
			}
		}

		string _tmpPath;
		string TmpPath
		{
			get
			{
				if (_tmpPath == null)
				{
					_tmpPath = Path.GetFullPath(Path.GetTempFileName());
					File.Delete(_tmpPath);
					Directory.CreateDirectory(_tmpPath);
				}

				return _tmpPath;
			}
		}

		[TestFixtureTearDown]
		public void DeleteTmp()
		{
			if (Directory.Exists(TmpPath))
				Directory.Delete(TmpPath, true);
		}

		[TestFixtureSetUp]
		public void CreateZipAndTpz()
		{
			using (ZipFile zf = ZipFile.Create(QQnPath.Combine(TmpPath, "TestZip.zip")))
			{
				AddSomeFiles(zf);
			}

			AssuredStreamCreateArgs assuredArgs = new AssuredStreamCreateArgs();
			assuredArgs.StrongNameKey = StrongNameKey.LoadFrom(SnkFile);
			assuredArgs.FileType = "TPZ-Test";

			MultipleStreamCreateArgs mutlArgs = new MultipleStreamCreateArgs();
			mutlArgs.VerificationMode = VerificationMode.Full;
			
			using(FileStream fileStream = File.Create(QQnPath.Combine(TmpPath, "TestTpz.zip")))
			using (AssuredStream assuredStream = new AssuredStream(fileStream, assuredArgs))
			using(MultipleStreamWriter msw = new MultipleStreamWriter(assuredStream, mutlArgs))
			{
				using (Stream s = msw.CreateStream())
				{
					s.WriteByte(255);
				}

				using (Stream s = msw.CreateStream())
				using (ZipFile zf = ZipFile.Create(s))
				{
					AddSomeFiles(zf);
				}
			}
		}

		private void AddSomeFiles(ZipFile zf)
		{
			zf.BeginUpdate();
			zf.Add("QQn.TurtlePackage.dll");
			zf.Add("QQn.TurtleUtils.dll");
			zf.CommitUpdate();
		}

		[Test]
		public void ReadTpzAsZip()
		{
		}

		[Test]
		public void ReadTpzAsTpz()
		{
		}
	}
}
