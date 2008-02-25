using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtleUtils.IO;
using NUnit.Framework.SyntaxHelpers;
using System.IO;
using QQn.TurtleBuildUtils;

namespace TurtleTests
{
	[TestFixture]
	public class PathTests
	{
		[Test]
		public void ExtensionEquals()
		{
			Assert.That(QQnPath.ExtensionEquals("c:/test.txt", ".txt"), Is.True);
			Assert.That(QQnPath.ExtensionEquals("c:/test.txt", ".TXT"), Is.True);
			Assert.That(QQnPath.ExtensionEquals("test.tXt", ".txt"), Is.True);
		}

		[Test]
		public void EnsureRelativePath()
		{
			Assert.That(QQnPath.EnsureRelativePath("c:\\tools", "c:/tools/t.txt"), Is.EqualTo("t.txt"));
			Assert.That(QQnPath.EnsureRelativePath("c:\\tools", "c:/t.txt"), Is.EqualTo("..\\t.txt"));

			Assert.That(QQnPath.EnsureRelativePath("c:\\tools", "t.txt"), Is.EqualTo("t.txt"));
			Assert.That(QQnPath.EnsureRelativePath("c:\\tools", "\\banaan\\t.txt"), Is.EqualTo("..\\banaan\\t.txt"));

			string currentDisk = Path.GetPathRoot(Environment.CurrentDirectory);
			string otherDisk;

			if(string.Equals(currentDisk, "z:\\", StringComparison.OrdinalIgnoreCase))
				otherDisk = "z:\\";
			else
				otherDisk = "c:\\";

			Assert.That(Path.Combine(otherDisk + "tools", "\\t.txt"), Is.EqualTo("\\t.txt"), "Path.Combine works like .Net 2.0");
			Assert.That(Path.GetFullPath(Path.Combine(otherDisk + "tools", "\\t.txt")), Is.EqualTo(currentDisk + "t.txt"), "Path.Combine works like .Net 2.0");
			Assert.That(QQnPath.Combine(otherDisk + "tools", "\\t.txt"), Is.EqualTo( otherDisk + "t.txt"), "QQnPath combines always relative");


			Assert.That(QQnPath.NormalizePath("c:\\", false), Is.EqualTo("c:\\"));
		}

		[Test]
		public void RuntimePaths()
		{
			DirectoryInfo dir11 = BuildTools.GetFrameworkDirectory(new Version("1.1"));
			DirectoryInfo dir20 = BuildTools.GetFrameworkDirectory(new Version("2.0"));
			DirectoryInfo dir30 = BuildTools.GetFrameworkDirectory(new Version("3.0"));
			DirectoryInfo dir35 = BuildTools.GetFrameworkDirectory(new Version("3.5"));

			Assert.That(dir11.Exists);
			Assert.That(dir20.Exists);
			Assert.That(dir35.Exists);

		}

		[Test]
		public void CheckSolutionVersions()
		{
			DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
			while (di.Parent.FullName != di.Root.FullName)
			{
				bool breakOut = false;
				foreach (FileInfo f in di.GetFiles("*.sln", SearchOption.TopDirectoryOnly))
				{
					Version v = BuildTools.GetSolutionVersion(f.FullName);

					Assert.That(v, Is.Not.Null);

					Assert.That(v, Is.EqualTo(f.Name.Contains("2008") ? new Version("10.0") : new Version("9.0")));
					breakOut = true;
				}
				if (breakOut)
					break;
				di = di.Parent;
			}
		}

		[Test]
		public void VerifyTrueName()
		{
			foreach(Environment.SpecialFolder sv in Enum.GetValues(typeof(Environment.SpecialFolder)))
			{
				string path = Environment.GetFolderPath(sv);

				if (string.IsNullOrEmpty(path))
					continue;

				Assert.That(QQnPath.GetTruePath(path.ToUpperInvariant()), Is.Not.Null);
			}
		}

	}
}
