using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtleUtils.Tags;
using QQn.TurtleUtils.Tags.ExpressionParser;


namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	[TestFixture]
	public class ParserTests
	{
		ParserState NewState(string expression)
		{
			return NewState(expression, false);
		}

		ParserState NewState(string expression, bool usePriority)
		{
			ParserArgs args = new ParserArgs();
			args.AllowProperties = true;
			args.AllowTags = true;
			args.AllowItems = true;
			args.ApplyAndOrPriority = usePriority;

			return new ParserState(expression, args);
		}

		[Test]
		public void ParseSome()
		{
			TagExpression expr = Parser.ParseExpression(NewState(" '$(Configuration)' == '' "));
			Assert.That(expr, Is.Not.Null);
			Assert.That(expr.ToString(), Is.EqualTo("('$(Configuration)' == '')"));

			expr = Parser.ParseExpression(NewState("( '$(ProjectOutput)' <= '@(Configuration)' AnD 12 < 24.0 ) Or 12==24"));
			Assert.That(expr, Is.Not.Null);
			Assert.That(expr.ToString(), Is.EqualTo("((('$(ProjectOutput)' <= '@(Configuration)') AND (12 < 24.0)) OR (12 == 24))"));
		}

		[Test]
		public void ParseExceptions()
		{
			Exception e = null;
			try
			{
				TagExpression expr = Parser.ParseExpression(NewState(" '$(Configuration)' == '' == 24"));
			}
			catch (ExpressionException ee)
			{ e = ee; }
			Assert.That(e, Is.TypeOf(typeof(ParserException)));

			e = null;
			try
			{
				TagExpression expr = Parser.ParseExpression(NewState(" '$(Configuration)' == '' |"));
			}
			catch (ExpressionException ee)
			{ e = ee; }
			Assert.That(e, Is.TypeOf(typeof(LexerException)));

			e = null;
			try
			{
				TagExpression expr = Parser.ParseExpression(NewState(" '$(Configuration)' == '' 12"));
			}
			catch (ExpressionException ee)
			{ e = ee; }
			Assert.That(e, Is.TypeOf(typeof(ParserException)));

			e = null;
			try
			{
				TagExpression expr = Parser.ParseExpression(NewState("('$(Configuration)' == ''"));
			}
			catch (ExpressionException ee)
			{ e = ee; }
			Assert.That(e, Is.TypeOf(typeof(ParserException)));

			e = null;
			try
			{
				TagExpression expr = Parser.ParseExpression(NewState(")'$(Configuration)' == ''"));
			}
			catch (ExpressionException ee)
			{ e = ee; }
			Assert.That(e, Is.TypeOf(typeof(ParserException)));

			e = null;
			try
			{
				TagExpression expr = Parser.ParseExpression(NewState("'$(Configuration)' == '',"));
			}
			catch (ExpressionException ee)
			{ e = ee; }
			Assert.That(e, Is.TypeOf(typeof(ParserException)));
		}

		[Test]
		public void AndOrTests()
		{
			TagExpression onlyAnd = Parser.ParseExpression(NewState("1==2 and 2==2 AND 3==2 and 4==2 and 5==2"));			

			Assert.That(onlyAnd.ToString(), Is.EqualTo("(((((1 == 2) AND (2 == 2)) AND (3 == 2)) AND (4 == 2)) AND (5 == 2))"));

			TagExpression onlyOr = Parser.ParseExpression(NewState("1==2 or 2==2 OR 3==2 or 4==2 or 5==2"));
			Assert.That(onlyOr.ToString(), Is.EqualTo("(((((1 == 2) OR (2 == 2)) OR (3 == 2)) OR (4 == 2)) OR (5 == 2))"));

			Exception e = null;
			try
			{
				TagExpression errorAndException = Parser.ParseExpression(NewState("1==2 and 2==2 OR 3==2"));
			}
			catch (ExpressionException ee)
			{ e = ee; }
			Assert.That(e, Is.TypeOf(typeof(PriorityException)));

			TagExpression andException = Parser.ParseExpression(NewState("1==2 and 2==2 OR 3==2", true));
			Assert.That(andException.ToString(), Is.EqualTo("(((1 == 2) AND (2 == 2)) OR (3 == 2))"));

			andException = Parser.ParseExpression(NewState("1==2 or 2==2 and 3==2", true));
			Assert.That(andException.ToString(), Is.EqualTo("((1 == 2) OR ((2 == 2) AND (3 == 2)))"));

		}
	}
}
