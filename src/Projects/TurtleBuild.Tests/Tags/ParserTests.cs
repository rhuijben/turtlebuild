using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtleUtils.Tags;
using QQn.TurtleUtils.Tags.ExpressionParser;
using NUnit.Framework.SyntaxHelpers;

namespace TurtleTests.Tags
{
	[TestFixture]
	public class ParserTests
	{
		ParserState NewState(string expression)
		{
			ParserArgs args = new ParserArgs();
			args.AllowProperties = true;
			args.AllowTags = true;
			args.AllowItems = true;

			return new ParserState(expression, args);

		}

		[Test]
		public void ParseSome()
		{
			TagExpression expr = Parser.ParseExpression(NewState(" '$(Configuration)' == '' "));
			Assert.That(expr, Is.Not.Null);
			Assert.That(expr.ToString(), Is.EqualTo("('$(Configuration)' == '')"));

			expr = Parser.ParseExpression(NewState(" '$(ProjectOutput)' <= '@(Configuration)' AnD 12 < 24.0 OR 12==24"));
			Assert.That(expr, Is.Not.Null);
			Assert.That(expr.ToString(), Is.EqualTo("((('$(ProjectOutput)' <= '@(Configuration)') and (12 < 24.0)) or (12 == 24))"));
		}
	}
}
