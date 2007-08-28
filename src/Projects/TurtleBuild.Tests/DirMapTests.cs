using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using QQn.TurtleUtils.IO;
using NUnit.Framework.SyntaxHelpers;

namespace TurtleTests
{
	[TestFixture]
	public class DirMapTests
	{
		string _dirMapPath;
		public string DirMapPath
		{
			get
			{
				if (_dirMapPath == null)
					_dirMapPath = Path.Combine(Path.GetTempPath(), "DirMap");

				return _dirMapPath;
			}
		}

		[TestFixtureSetUp]
		public void Setup()
		{
			ClearMap();
		}

		public void ClearMap()
		{
			if (Directory.Exists(DirMapPath))
				Directory.Delete(DirMapPath, true);

			Directory.CreateDirectory(DirMapPath);
		}

		[Test]
		public void TestCombine()
		{
			string result = QQnPath.Combine("d:\\buildenv\\project", "bin\\release\\", "file.tmp");

			Assert.That(result, Is.EqualTo("d:\\buildenv\\project\\bin\\release\\file.tmp"));


			string r = QQnPath.EnsureRelativePath(@"d:\buildenv\TcgTools\tb-src\Libraries\QQn.TurtleUtils", @"d:\buildenv\TcgTools\tb-src\Libraries\QQn.TurtleUtils\obj\Release\QQn.TurtleUtils.pdb");

			Assert.That(r, Is.EqualTo(@"obj\Release\QQn.TurtleUtils.pdb"));

		}

		[Test]
		public void TestFileWrite()
		{
			ClearMap();
			using (DirectoryMap dm = DirectoryMap.Get(DirMapPath))
			{
				using (Stream fs = dm.CreateFile("test.txt"))
				{
					using (StreamWriter sw = new StreamWriter(fs))
					{
						sw.WriteLine("46357865-EFF5-454B-9B8F-845C6C17F9D5");
					}
				}

				using (Stream fs = dm.CreateFile("sd/test.txt"))
				{
					using (StreamWriter sw = new StreamWriter(fs))
					{
						sw.WriteLine("46357865-EFF5-454B-9B8F-845C6C17F9D5");
					}
				}

				using (Stream fs = dm.CreateFile("q/r/s/test.txt"))
				{
					using (StreamWriter sw = new StreamWriter(fs))
					{
						sw.WriteLine("46357865-EFF5-454B-9B8F-845C6C17F9D5");
					}
				}

				DirectoryMapFile mf = dm.GetFile("test.txt");

				Assert.That(mf, Is.Not.Null);
			}

			using (DirectoryMap dm = DirectoryMap.Get(DirMapPath))
			{
				Assert.That(dm.GetFile("test.txt"), Is.Not.Null);
				Assert.That(dm.GetFile("-not-existing"), Is.Null);

				Assert.That(dm.GetFile("sD/TeSt.TXT"), Is.Not.Null);

				Assert.That(dm.GetFile("q/r/s/TEST.txt"), Is.Not.Null);
				Assert.That(dm.GetFile("q\\r/s/./test.txt"), Is.Not.Null, "non-normalized file exists");

				Assert.That(dm.GetFile("test.txT").FileSize, Is.EqualTo(38L));
				Assert.That(dm.GetFile("test.txT").FileHash, Is.EqualTo("b2cf3a852a1793273d1029ea777099523da1f734,type=SHA1"));

				DirectoryMapFile dmf = dm.GetFile("test.txt");

				Assert.That(VerifyUtils.VerifyFile(DirMapPath, dmf, VerifyMode.Full), "File is the same in full");

				Assert.That(VerifyUtils.VerifyFile(DirMapPath, dmf, VerifyMode.Time), Is.True, "File is the same in time");
				Assert.That(VerifyUtils.VerifyFile(DirMapPath, dmf, VerifyMode.TimeSize), Is.True, "File is the same in size");
				Assert.That(VerifyUtils.VerifyFile(DirMapPath, dmf, VerifyMode.FileHash), Is.True, "File is the same in filehash");
								
				File.SetLastWriteTime(dm.GetFile("test.txt").FullName, DateTime.Now);
				Assert.That(VerifyUtils.VerifyFile(DirMapPath, dmf, VerifyMode.Time), Is.False, "File time changed");
				Assert.That(VerifyUtils.VerifyFile(DirMapPath, dmf, VerifyMode.TimeSize), Is.False, "FileTime+Size changed");
				Assert.That(VerifyUtils.VerifyFile(DirMapPath, dmf, VerifyMode.FileHash), Is.True, "File is the same in filehash");
				Assert.That(VerifyUtils.VerifyFile(DirMapPath, dmf, VerifyMode.Full), Is.True, "File has not changed");

				dmf.Update();

				Assert.That(VerifyUtils.VerifyFile(DirMapPath, dmf, VerifyMode.TimeSize), "File is the same in timesize after update");
				Assert.That(VerifyUtils.VerifyFile(DirMapPath, dmf, VerifyMode.Full), "File is the same in full");
			}
		}
	}
}
