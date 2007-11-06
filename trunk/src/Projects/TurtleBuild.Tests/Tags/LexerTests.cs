using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtleUtils.Tags.ExpressionParser;
using NUnit.Framework.SyntaxHelpers;

namespace TurtleTests.Tags
{
	[TestFixture]
	public class LexerTests
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
		public void LexVarCompare()
		{
			ParserState state = NewState("'$(banana)' == 'yellow'");

			TagToken tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo(TagTokenType.String));
			Assert.That(tk.Value, Is.EqualTo("'$(banana)'"));

			tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo(TagTokenType.IsEqual));
			Assert.That(tk.Value, Is.EqualTo("=="));

			tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo(TagTokenType.String));
			Assert.That(tk.Value, Is.EqualTo("'yellow'"));

			tk = Lexer.GetNextToken(state);

			Assert.That(tk, Is.Null);
		}

		[Test]
		public void LexItemExists()
		{
			ParserState state = NewState("exists(@(ProjectOutput)) != false or 12.5");

			TagToken tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo(TagTokenType.Function));
			Assert.That(tk.Value, Is.EqualTo("exists"));

			tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo((TagTokenType)'('));
			Assert.That(tk.Value, Is.EqualTo("("));

			tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo(TagTokenType.Item));
			Assert.That(tk.Value, Is.EqualTo("@(ProjectOutput)"));

			tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo((TagTokenType)')'));
			Assert.That(tk.Value, Is.EqualTo(")"));


			tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo(TagTokenType.IsNot));
			Assert.That(tk.Value, Is.EqualTo("!="));


			tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo(TagTokenType.Literal));
			Assert.That(tk.Value, Is.EqualTo("false"));

			tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo(TagTokenType.Or));
			Assert.That(tk.Value, Is.EqualTo("or"));

			tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo(TagTokenType.Number));
			Assert.That(tk.Value, Is.EqualTo("12.5"));

			tk = Lexer.GetNextToken(state);

			Assert.That(tk, Is.Null);
		}

		[Test]
		public void LexTagItem()
		{
			ParserState state = NewState("@(ProjectOutput=>'%(RelativePath)') != 'bin/$(Configuration)/test.txt'");

			TagToken tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo(TagTokenType.Item));
			Assert.That(tk.Value, Is.EqualTo("@(ProjectOutput=>'%(RelativePath)')"));

			tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo(TagTokenType.IsNot));
			Assert.That(tk.Value, Is.EqualTo("!="));

			tk = Lexer.GetNextToken(state);

			Assert.That(tk.TokenType, Is.EqualTo(TagTokenType.String));
			Assert.That(tk.Value, Is.EqualTo("'bin/$(Configuration)/test.txt'"));

			tk = Lexer.GetNextToken(state);

			Assert.That(tk, Is.Null);
		}

		[Test]
		public void LexOperators()
		{
			ParserState state = NewState("== != <= < > >= ! or and ()");

			Assert.That(Lexer.GetNextToken(state).TokenType, Is.EqualTo(TagTokenType.IsEqual));
			Assert.That(Lexer.GetNextToken(state).TokenType, Is.EqualTo(TagTokenType.IsNot));
			Assert.That(Lexer.GetNextToken(state).TokenType, Is.EqualTo(TagTokenType.IsLte));
			Assert.That(Lexer.GetNextToken(state).TokenType, Is.EqualTo(TagTokenType.IsLt));
			Assert.That(Lexer.GetNextToken(state).TokenType, Is.EqualTo(TagTokenType.IsGt));
			Assert.That(Lexer.GetNextToken(state).TokenType, Is.EqualTo(TagTokenType.IsGte));
			Assert.That(Lexer.GetNextToken(state).TokenType, Is.EqualTo(TagTokenType.Not));
			Assert.That(Lexer.GetNextToken(state).TokenType, Is.EqualTo(TagTokenType.Or));
			Assert.That(Lexer.GetNextToken(state).TokenType, Is.EqualTo(TagTokenType.And));
			Assert.That(Lexer.GetNextToken(state).TokenType, Is.EqualTo(TagTokenType.ParenOpen));
			Assert.That(Lexer.GetNextToken(state).TokenType, Is.EqualTo(TagTokenType.ParenClose));

			Assert.That(Lexer.GetNextToken(state), Is.Null);
		}
	}
}
