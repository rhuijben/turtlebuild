using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Text.RegularExpressions;
using QQn.TurtleBuildUtils.AttributeParsers;
using System.Diagnostics;

namespace TurtleTests
{
	[TestFixture]
	public class AsmInfoTests
	{
		[Test]
		public void CreateRegex()
		{
			AttributeParser ap = AttributeParser.Get("assemblyinfo.cs");

			Regex re = ap.AttributeRegex;

			Match m = re.Match(@"[assembly: AssemblyTitle(""QQn.TurtleBuildUtils"", ""qq"", 12,a=true,b=q.r.s)] // banaan");

			Assert.That(m.Success);
			Trace.WriteLine(m.Groups.Count);

			Trace.WriteLine("NsPrefix:");
			WriteCaptures(m.Groups["nsPrefix"]);

			Trace.WriteLine("Attribute:");
			WriteCaptures(m.Groups["attribute"]);

			Trace.WriteLine("Arg:");
			WriteCaptures(m.Groups["arg"]);
			Trace.WriteLine("Name:");
			WriteCaptures(m.Groups["name"]);
			Trace.WriteLine("Value:");
			WriteCaptures(m.Groups["value"]);

			Trace.WriteLine("Comment:");
			WriteCaptures(m.Groups["comment"]);
			Trace.WriteLine("/");

			foreach (Group g in m.Groups)
			{
				Trace.WriteLine(g.Value);
				WriteCaptures(g);

			}

			Assert.That(m.Success);
		}

		private static void WriteCaptures(Group g)
		{
			foreach (Capture c in g.Captures)
			{
				Trace.WriteLine(string.Format("- {0}", c.Value));
			}
		}

	}
}
