using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtleUtils.IO;
using NUnit.Framework.SyntaxHelpers;
using System.IO;

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

	}
}
