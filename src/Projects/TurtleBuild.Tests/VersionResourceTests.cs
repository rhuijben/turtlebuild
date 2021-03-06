using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtleMSBuild;
using QQn.TurtleBuildUtils;
using System.IO;
using QQn.TurtleUtils.IO;

namespace TurtleTests
{
	[TestFixture]
	public class VersionResourceTests
	{
		[Test]
		public void CopyResources()
		{
			Uri uri = new Uri(typeof(AssemblyUtils).Assembly.CodeBase);
			string file = uri.LocalPath;
			string tmpFile = QQnPath.Combine(Path.GetTempPath(), Path.GetFileName(file));

			File.Copy(file, tmpFile, true);
			Assert.That(AssemblyUtils.RefreshVersionInfoFromAttributes(tmpFile, Path.GetFullPath("..\\..\\..\\..\\Libraries\\QQn.TurtleBuildUtils\\QQn.TurtleBuildUtils.snk"), null));
		}
	}
}
