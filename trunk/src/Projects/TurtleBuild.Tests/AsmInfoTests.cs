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
		public void VerifyCSharp()
		{
			DualParse(AttributeParser.Get("assemblyinfo.cs"), "  [assembly : System.Reflection.TestDescriptionAttribute ( 1 , \"text\", 12.3, FileMode.Read, '7', Number=2, Value = true) ] // Comment ");
		}

		[Test]
		public void VerifyCpp()
		{
			DualParse(AttributeParser.Get("assemblyinfo.cpp"), "  [assembly : System::Reflection::TestDescriptionAttribute ( 1 , \"text\", 12.3, FileMode::Read, 'r', '\\'', Text \"with\" Identifiers, Number=2, Value = true) ]; // Comment ");
		}

		[Test]
		public void VerifyVb()
		{
			DualParse(AttributeParser.Get("assemblyinfo.vb"), "  <assembly : System.Reflection.TestDescriptionAttribute ( 1 , \"text\", 12.3, FileMode.Read, Number=2, Value = true) > ' Comment ");
		}

		[Test]
		public void VerifyJSharp()
		{
			DualParse(AttributeParser.Get("assemblyinfo.jsl"), " /**  @assembly System.Reflection.TestDescriptionAttribute ( 1 , \"text\", 12.3, FileMode.Read, Number=2, Value = true) */ // Comment ");
		}

		[Test]
		public void VerifyJs()
		{
			DualParse(AttributeParser.Get("assemblyinfo.js"), " var TestDescriptionAttribute = [ 1, \"text\", 12.3, FileModeRead, ['Number':2], ['Value': true]]; // Comment ");
		}

		[Test]
		public void VerifyActionScript()
		{
			DualParse(AttributeParser.Get("assemblyinfo.as"), " static var TestDescriptionAttribute = [ 1, \"text\", 12.3, FileModeRead, ['Number':2], ['Value': true]]; // Comment ");
		}

		void DualParse(AttributeParser parser, string line)
		{
			Assert.That(parser, Is.Not.Null, "Parser available");

			AttributeDefinition def = parser.ParseLine(line);

			Assert.That(def, Is.Not.Null, "Can parse attribute");

			Assert.That(def.Name, Is.EqualTo("TestDescriptionAttribute"));

			if(parser.SupportsNamespaces)
				Assert.That(def.NamespacePrefix, Is.EqualTo("System.Reflection"));

			Assert.That(def.Arguments.Count, Is.GreaterThanOrEqualTo(4));
			Assert.That(def.NamedArguments.Count, Is.GreaterThanOrEqualTo(2));

			Assert.That(def.Arguments[0], Is.EqualTo("1"));
			Assert.That(def.Arguments[1].Contains("text"));
			Assert.That(def.Arguments[2].Contains("12") && def.Arguments[2].Contains("3"));
			Assert.That(def.Arguments[3].Contains("FileMode") && def.Arguments[3].Contains("Read"));
			Assert.That(def.NamedArguments[0].Name, Is.EqualTo("Number"));
			Assert.That(def.NamedArguments[0].Value, Is.EqualTo("2"));
			Assert.That(def.NamedArguments[1].Name, Is.EqualTo("Value"));
			Assert.That(def.NamedArguments[1].Value.Contains("true"));
			
			Assert.That(def, Is.Not.Null, "Can parse attribute with parser {0}", parser.GetType().Name);

			string newLine = parser.GenerateAttributeLine(def);

			Assert.That(newLine, Is.Not.Null, "Can regenerate attribute line with parser {0}", parser.GetType().Name);

			AttributeDefinition def2 = parser.ParseLine(newLine);
			Assert.That(def2, Is.Not.Null, "Can parse second time");

			AssertEquals(def, def2);

			def2.Comment = newLine;

			string lineWithComment = parser.GenerateAttributeLine(def2);

			AttributeDefinition def3 = parser.ParseLine(lineWithComment);

			Assert.That(def3, Is.Not.Null, "Can parse with comment");

			string commentItem = def3.Comment;
			Assert.That(commentItem, Is.Not.Null, "Retrieved comment");

			Assert.That(commentItem, Is.EqualTo(newLine));


			//AttributeDefinition def3 = parser.Ge
		}

		private static void AssertEquals(AttributeDefinition def, AttributeDefinition def2)
		{			
			Assert.That(def2.Name, Is.EqualTo(def.Name));
			Assert.That(def2.NamespacePrefix, Is.EqualTo(def.NamespacePrefix));
			Assert.That(def2.Comment, Is.EqualTo(def.Comment));
			Assert.That(def2.Arguments.Count, Is.EqualTo(def.Arguments.Count));

			for (int i = 0; i < def.Arguments.Count; i++)
			{
				Assert.That(def2.Arguments[i], Is.EqualTo(def.Arguments[i]));
			}

			Assert.That(def2.NamedArguments.Count, Is.EqualTo(def.NamedArguments.Count));

			for (int i = 0; i < def.NamedArguments.Count; i++)
			{
				Assert.That(def2.NamedArguments[i].Name, Is.EqualTo(def.NamedArguments[i].Name));
				Assert.That(def2.NamedArguments[i].Value, Is.EqualTo(def.NamedArguments[i].Value));
			}
		}
	}
}
