using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	/// <summary>
	/// 
	/// </summary>
	public static class Lexer
	{
		/// <summary>
		/// Gets the next token.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <returns></returns>
		internal static TagToken GetNextToken(ParserState state)
		{
			state.SkipWhitespace();

			char c = state.NextChar();

			switch (c)
			{
				case '\'':
					return ScanQuoteString(state);
				case '<':
					if(state.PeekChar == '=')
					{
						state.Next();
						return state.CreateToken(TagTokenType.IsLte, "<=");
					}
					else
						return state.CreateToken('<');

				case '>':
					if (state.PeekChar == '=')
					{
						state.Next();
						return state.CreateToken(TagTokenType.IsGte, ">=");
					}
					else
						return state.CreateToken('>');

				case '!':
					if (state.PeekChar == '=')
					{
						state.Next();
						return state.CreateToken(TagTokenType.IsNot, "!=");
					}
					else
						return state.CreateToken('!');
				case '=':
					if (state.PeekChar == '=')
					{
						state.Next();
						return state.CreateToken(TagTokenType.IsEqual, "==");
					}

					throw new LexerException("Ill formatted equals in expression", state);

				case '(':
				case ')':
				case ',':
				case ';':
					return state.CreateToken(c);

				case '$':
					if (!state.Args.AllowProperties)
						throw new LexerException("A property reference is not allowed at this point", state);

					return ParsePropertyOrTag(state, TagTokenType.Property);

				case '%':
					if (!state.Args.AllowTags)
						throw new LexerException("A tag reference is not allowed at this point", state);

					return ParsePropertyOrTag(state, TagTokenType.Tag);
				
				case '@':
					if(!state.Args.AllowItems)
						throw new LexerException("An item reference is not allowed at this point", state);

					return ParseItem(state);

				case '\0':
					if(state.AtEnd)
						return null;
					else
						goto default;				
				default:
					return ParseLiteral(state, c);
			}
		}

		/// <summary>
		/// Walks the tokens.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <returns></returns>
		internal static IEnumerable<TagToken> WalkTokens(ParserState state)
		{
			TagToken tk;

			while (null != (tk = GetNextToken(state)))
			{
				yield return tk;
			}
		}

		/// <summary>
		/// Walks the tokens.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <param name="args">The args.</param>
		/// <returns></returns>
		public static IEnumerable<TagToken> WalkTokens(string expression, ParserArgs args)
		{
			ParserState state = new ParserState(expression, args);

			return WalkTokens(new ParserState(expression, args));
		}

		static TagToken ParseLiteral(ParserState state, char c)
		{
			// We skipped whitespace at this point
			if (c == '_' || char.IsLetter(c))
			{
				state.SkipLiteralChars();

				if(state.LiteralEquals("and"))
					return state.CreateToken(TagTokenType.And, "AND");
				else if(state.LiteralEquals("or"))
					return state.CreateToken(TagTokenType.Or, "OR");

				string name = state.CurrentTokenText;

				state.SkipWhitespace();

				if (state.PeekChar == '(')
					return state.CreateToken(TagTokenType.Function, name.Trim());
				else
					return state.CreateToken(TagTokenType.Literal, name.Trim());
			}
			else if ((c == '+') || (c == '-') || (c == '.') || char.IsDigit(c))
			{
				return ParseNumber(state, c);
			}

			throw new LexerException(string.Format("Unexpected character '{0}' in expression", c), state);
		}

		static TagToken ParseNumber(ParserState state, char c)
		{
			if (c == '0' && state.PeekChar == 'x' || state.PeekChar == 'X')
			{
				state.Next();

				// Scanning 0x0E3F hex string
				state.SkipHexDigits(); // Bug: 0x is a valid value!

				return state.CreateToken(TagTokenType.Number, state.CurrentTokenText);
			}
			else if (c == '+' || c == '-')
			{
				if(!char.IsDigit(state.PeekChar) && state.PeekChar != '.')
					throw new LexerException(string.Format("Unexpected character '{0}' in expression", state.PeekChar), state);

				c = state.NextChar();
			}

			state.SkipDigits();

			if (state.PeekChar == '.')
			{
				state.Next();
				state.SkipDigits();
			}

			return state.CreateToken(TagTokenType.Number, state.CurrentTokenText);
		}

		static TagToken ParseItem(ParserState state)
		{
			ParseItemInternal(state);

			return state.CreateToken(TagTokenType.Item, state.CurrentTokenText);
		}

		static TagToken ParsePropertyOrTag(ParserState state, TagTokenType type)
		{
			ParsePropertyOrTagInternal(state);

			return state.CreateToken(type, state.CurrentTokenText);
		}

		static TagToken ScanQuoteString(ParserState state)
		{
			char c;
			do
			{
				c = state.NextChar();

				switch(c)
				{
					case '\'':
						// Break out of loop
						break; 
					case '%':
						if(state.Args.AllowTags && state.PeekChar == '(')
						{
							ParsePropertyOrTagInternal(state);
							continue;
						}
						goto default;
					case '@':
						if(state.Args.AllowItems && state.PeekChar == '(')
						{
							ParseItemInternal(state);
							continue;
						}
						goto default;
					case '$':
						if(state.Args.AllowProperties && state.PeekChar == '(')
						{
							ParsePropertyOrTagInternal(state);
							continue;
						}
						goto default;
					default:
						// Store character and continue
						break;
				}
			}
			while(c != '\0' && c != '\'');

			if(c == '\'')
			{
				return state.CreateToken(TagTokenType.String, state.CurrentTokenText);
			}
			else if(state.AtEnd)
				throw new LexerException("Unexpected end of expression in string", state);
			else
				throw new LexerException(string.Format("Unexpected character '{0}' in string", c), state);
		}

		private static void ParsePropertyOrTagInternal(ParserState state)
		{
			char c = state.NextChar();

			if(c != '(')
				throw new LexerException("Expected '('", state);

			while(!state.AtEnd)
			{
				c = state.NextChar();

				if(c == ')')
					break;
			}

			if(c != ')')
				throw new LexerException("Expected ')' before the end of the expression", state);
		}

		static void ParseItemInternal(ParserState state)
		{
			char c = state.NextChar();

			if (c != '(')
				throw new LexerException("Expected '('", state);

			bool quote = false;
			while (!state.AtEnd)
			{
				c = state.NextChar();

				if (c == '\'')
					quote = !quote;
				else if (c == ')' && !quote)
					break;
			}

			if (c != ')' || quote)
				throw new LexerException("Expected ')' before the end of the expression", state);
		}
	}
}

