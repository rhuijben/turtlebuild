using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtleUtils.Tags;
using NUnit.Framework.SyntaxHelpers;

namespace TurtleTests.Tags
{
	[TestFixture]
	public class TagTransformTests
	{
		TagEnvironment CreateEnvironment()
		{
			TagEnvironment env = new TagEnvironment();
			env.Properties.LoadEnvironmentVariables();

			env.Items.AddFile("ProjectOutput", "assembly.dll", "c:\\work\\app\\bin\\debug");
			env.Items.AddFile("ProjectOutput", "en\\assembly.resources.dll", "c:\\work\\app\\bin\\debug");
			env.Items.AddFile("Scripts", "setup.info", "c:\\work\\app");

			return env;
		}

		[Test]
		public void CreateBatch()
		{
			TagEnvironment env = CreateEnvironment();
			Assert.That(env, Is.Not.Null);

			TagBatchDefinition<string> tbd = new TagBatchDefinition<string>();
			tbd.Add("src", "@(ProjectOutput)", typeof(ITagItem));
			tbd.Add("to", "@(ProjectOutput=>'%(FullPath)')", typeof(string));
			tbd.Add("info", "%(Origin)", typeof(string));
			tbd.Add("if", "'%(Origin)' == 'c:\\work\\app\\bin\\debug'", typeof(string));

			foreach (TagBatchInstance<string> r in env.RunBatch(tbd))
			{
				Assert.That(r, Is.Not.Null);

				Assert.That(r["src"], Is.TypeOf(typeof(ITagItem)));
				Assert.That(r["to"], Is.TypeOf(typeof(string)));
				Assert.That(r["info"], Is.TypeOf(typeof(string)));
				Assert.That(r["if"], Is.TypeOf(typeof(bool)));
			}
		}
	}
}
