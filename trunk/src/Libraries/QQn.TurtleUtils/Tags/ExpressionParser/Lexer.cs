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
		static Token GetNextToken(ParserState state)
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
						return state.CreateToken(TokenType.IsLte, "<=");
					}
					else
						return state.CreateToken('<');

				case '>':
					if (state.PeekChar == '=')
					{
						state.Next();
						return state.CreateToken(TokenType.IsGte, ">=");
					}
					else
						return state.CreateToken('>');

				case '!':
					if (state.PeekChar == '=')
					{
						state.Next();
						return state.CreateToken(TokenType.IsNot, "!=");
					}
					else
						return state.CreateToken('!');
				case '=':
					if (state.PeekChar == '=')
					{
						state.Next();
						return state.CreateToken(TokenType.IsEqual, "==");
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

					return ParsePropertyOrTag(state, TokenType.Property);

				case '%':
					if (!state.Args.AllowTags)
						throw new LexerException("A tag reference is not allowed at this point", state);

					return ParsePropertyOrTag(state, TokenType.Tag);
				
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

		static Token ParseLiteral(ParserState state, char c)
		{
			// We skipped whitespace at this point
			if (c == '_' || char.IsLetter(c))
			{
				state.SkipLiteralChars();

				if(state.LiteralEquals("and"))
					return state.CreateToken(TokenType.And, state.CurrentTokenText);
				else if(state.LiteralEquals("or"))
					return state.CreateToken(TokenType.Or, state.CurrentTokenText);

				string name = state.CurrentTokenText;

				state.SkipWhitespace();

				if (state.PeekChar == '(')
				{
					state.CreateToken(TokenType.Function, name.Trim());
				}
				else
					state.CreateToken(TokenType.Literal, name.Trim());
			}
			else if ((c == '+') || (c == '-') || (c == '.') || char.IsDigit(c))
			{
				return ParseNumber(state, c);
			}

			throw new LexerException(string.Format("Unexpected character '{0}' in expression", c), state);
		}

		static Token ParseNumber(ParserState state, char c)
		{
			if (c == '0' && state.PeekChar == 'x' || state.PeekChar == 'X')
			{
				state.Next();

				// Scanning 0x0E3F hex string
				state.SkipHexDigits(); // Bug: 0x is a valid value!

				return state.CreateToken(TokenType.Number, state.CurrentTokenText);
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

			return state.CreateToken(TokenType.Number, state.CurrentTokenText);
		}

		static Token ParseItem(ParserState state)
		{
			ParseItemInternal(state);

			throw new Exception();
		}

		static void ParseItemInternal(ParserState state)
		{
 			throw new Exception("The method or operation is not implemented.");
		}

		static Token ParsePropertyOrTag(ParserState state, TokenType type)
		{
			throw new Exception();
		}

		static Token ScanQuoteString(ParserState state)
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
							ParseTagInternal(state);
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
							ParsePropertyInternal(state);
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
				return state.CreateToken(TokenType.String, state.CurrentTokenText);
			}
			else if(state.AtEnd)
				throw new LexerException("Unexpected end of expression in string", state);
			else
				throw new LexerException(string.Format("Unexpected character '{0}' in string", c), state);
		}

		private static void ParseTagInternal(ParserState state)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		private static void ParsePropertyInternal(ParserState state)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}

