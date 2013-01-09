using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtleUtils.Tags;


namespace TurtleTests.Tags
{
	[TestFixture]
	public class TagCollectionTests
	{
		[Test]
		public void PropertyTests()
		{
			TagPropertyCollection tpc = new TagPropertyCollection();
			tpc.LoadEnvironmentVariables();

			Assert.That(tpc, Is.All.Not.Null);

			Assert.That(tpc["PATH"], Is.Not.Null);

			Assert.That(tpc.ExpandProperties("$(Path)"), Is.EqualTo(Environment.GetEnvironmentVariable("PATH")));

			Assert.That(TagBatchDefinition.IsTagItemType(typeof(Microsoft.Build.Framework.ITaskItem)), Is.True);
			Assert.That(TagBatchDefinition.IsTagItemType(typeof(string)), Is.True);
			Assert.That(TagBatchDefinition.IsTagItemType(typeof(ITagItem)), Is.True);
			Assert.That(TagBatchDefinition.IsTagItemType(typeof(int)), Is.False);
		}
	}
}
