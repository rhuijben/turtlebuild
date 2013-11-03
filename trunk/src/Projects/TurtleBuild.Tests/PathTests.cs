using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtleUtils.IO;
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

            if (string.Equals(currentDisk, "z:\\", StringComparison.OrdinalIgnoreCase))
                otherDisk = "z:\\";
            else
                otherDisk = "c:\\";

            Assert.That(Path.Combine(otherDisk + "tools", "\\t.txt"), Is.EqualTo("\\t.txt"), "Path.Combine works like .Net 2.0");
            Assert.That(Path.GetFullPath(Path.Combine(otherDisk + "tools", "\\t.txt")), Is.EqualTo(currentDisk + "t.txt"), "Path.Combine works like .Net 2.0");
            Assert.That(QQnPath.Combine(otherDisk + "tools", "\\t.txt"), Is.EqualTo(otherDisk + "t.txt"), "QQnPath combines always relative");


            Assert.That(QQnPath.NormalizePath("c:\\", false), Is.EqualTo("c:\\"));
        }

        [Test]
        public void GetFrameworkDirectory_11_ReturnsDirectory()
        {
            DirectoryInfo dir = BuildTools.GetFrameworkDirectory(new Version("1.1"));

            Assert.That(dir, Is.Null);
        }

        [Test]
        public void GetFrameworkDirectory_20_ReturnsDirectory()
        {
            DirectoryInfo dir = BuildTools.GetFrameworkDirectory(new Version("2.0"));

            Assert.That(dir.Name, Is.EqualTo("v2.0.50727"));
        }

        [Test]
        public void GetFrameworkDirectory_30_ReturnsDirectory()
        {
            DirectoryInfo dir = BuildTools.GetFrameworkDirectory(new Version("3.0"));

            Assert.That(dir.Name, Is.EqualTo("v3.0"));
        }

        [Test]
        public void GetFrameworkDirectory_35_ReturnsDirectory()
        {
            DirectoryInfo dir = BuildTools.GetFrameworkDirectory(new Version("3.5"));

            Assert.That(dir.Name, Is.EqualTo("v3.5"));
        }

        [Test]
        public void GetFrameworkDirectory_40_ReturnsDirectory()
        {
            DirectoryInfo dir = BuildTools.GetFrameworkDirectory(new Version("4.0"));

            Assert.That(dir.Name, Is.EqualTo("v4.0.30319"));
        }

        [Test]
        public void GetFrameworkDirectory_45_ReturnsDirectory()
        {
            DirectoryInfo dir = BuildTools.GetFrameworkDirectory(new Version("4.5"));

            Assert.That(dir.Name, Is.EqualTo("v4.0.30319"));
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

                    Version expectedVersion;

                    if (f.Name.Contains("2005"))
                        expectedVersion = new Version(9, 0);
                    else if (f.Name.Contains("2008"))
                        expectedVersion = new Version(10, 0);
                    else
                        // VS 2012 / Portable format
                        expectedVersion = new Version(12, 0);

                    Assert.That(v, Is.EqualTo(expectedVersion));
                    breakOut = true;
                }
                if (breakOut)
                    break;
                di = di.Parent;
            }
        }

        [Test]
        public void CheckFullSolutionVersion()
        {
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            while (di.Parent.FullName != di.Root.FullName)
            {
                bool breakOut = false;
                foreach (FileInfo f in di.GetFiles("TurtleBuild.sln", SearchOption.TopDirectoryOnly))
                {
                    Version slnVersion;
                    Version vsVersion;
                    Version minVsVersion;
                    Assert.That(BuildTools.TryGetSolutionAndVisualStudioVersion(f.FullName, out slnVersion, out vsVersion, out minVsVersion));

                    Assert.That(slnVersion, Is.EqualTo(new Version(12, 0)));

                    Assert.That(minVsVersion, Is.GreaterThanOrEqualTo(new Version(10, 0)));
                    Assert.That(minVsVersion, Is.LessThan(new Version(11, 0)));

                    Assert.That(vsVersion, Is.GreaterThanOrEqualTo(new Version(10, 0)));
                    breakOut = true;
                    break;
                }
                if (breakOut)
                    break;
                di = di.Parent;
            }
        }

        [Test]
        public void VerifyTrueName()
        {
            foreach (Environment.SpecialFolder sv in Enum.GetValues(typeof(Environment.SpecialFolder)))
            {
                string path = Environment.GetFolderPath(sv);

                if (string.IsNullOrEmpty(path))
                    continue;

                Assert.That(QQnPath.GetTruePath(path.ToUpperInvariant()), Is.Not.Null);
            }
        }

    }
}
