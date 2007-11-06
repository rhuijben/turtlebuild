using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	/// <summary>
	/// 
	/// </summary>
	public static class Parser
	{
		/// <summary>
		/// Parses the specified expression.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <param name="args">The args.</param>
		/// <returns></returns>
		public static TagExpression Parse(string expression, ParserArgs args)
		{
			if (string.IsNullOrEmpty(expression))
				throw new ArgumentNullException("expression");
			else if (args == null)
				throw new ArgumentNullException("args");

			return ParseExpression(new ParserState(expression, args));
		}
		/// <summary>
		/// Parses the expression.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <returns></returns>
		internal static TagExpression ParseExpression(ParserState state)
		{
			TagExpression expr = CompleteExpression(state);

			if (expr != null)
			{
				return ResolveAndOrConflicts(expr, state);
			}

			return null;
		}

		private static TagExpression ResolveAndOrConflicts(TagExpression expr, ParserState state)
		{
			return expr;
		}

		internal static TagExpression CompleteExpression(ParserState state)
		{
			TagExpression expr = BooleanExpression(state);

			while (state.PeekTokenType() != TagTokenType.AtEof)
			{
				TagToken tk = state.NextToken();

				switch (tk.TokenType)
				{
					case TagTokenType.Or:
						expr = new OrExpression(tk, expr, BooleanExpression(state));
						break;

					case TagTokenType.And:
						expr = new AndExpression(tk, expr, BooleanExpression(state));
						break;

					default:
						throw new ParserException(string.Format("Unexpected token {0}", tk), tk, state);
				}
			}

			return expr;
		}

		static TagExpression BooleanExpression(ParserState state)
		{
			TagExpression baseExpression = Part(state);

			if (baseExpression == null)
				return null; // Should not happen

			switch (state.PeekTokenType())
			{
				case TagTokenType.IsEqual:
				case TagTokenType.IsNot:
				case TagTokenType.IsLte:
				case TagTokenType.IsLt:
				case TagTokenType.IsGt:
				case TagTokenType.IsGte:
					TagToken expression = state.NextToken();

					return new CompareExpression(expression, baseExpression, Part(state));
				default:
					return baseExpression;
			}			
		}

		internal static TagExpression ArgValue(ParserState state)
		{
			TagToken tk = state.PeekToken();

			if (tk == null)
				return null;

			switch (tk.TokenType)
			{
				case TagTokenType.String:
					state.NextToken();
					return new StringExpression(tk);
				case TagTokenType.Number:
					state.NextToken();
					return new NumberExpression(tk);
				case TagTokenType.Property:
				case TagTokenType.Item:
				case TagTokenType.Tag:
					state.NextToken();
					return new DynamicStringExpression(tk);
				default:
					return null;
			}
		}

		internal static TagExpression Part(ParserState state)
		{
			TagExpression expression = ArgValue(state);
			if (expression != null)
				return expression;

			TagToken tk = state.NextToken();
			TagToken next;

			if (tk == null)
				return null;

			switch (tk.TokenType)
			{
				case TagTokenType.Function:
					next = state.NextToken();
					if (next == null)
						throw new ParserException("Expected '('", tk, state);
					else if (next.TokenType != TagTokenType.ParenOpen)
					{
						throw new ParserException(string.Format("Expected '(' instead of '{0}')", next), state);
					}

					List<TagExpression> functionArgs = ArgList(state);

					next = state.NextToken();

					if (next == null)
						throw new ParserException(string.Format("Expected ')' before end of expression", state.PeekToken()), state);
					else if (next.TokenType != TagTokenType.ParenClose)
						throw new ParserException(string.Format("Expected ')' instead of '{0}'", next), state);

					expression = new FunctionExpression(tk, functionArgs.AsReadOnly());
					break;
				case TagTokenType.ParenOpen:
					expression = new ParenExpression(tk, Expr(state));

					next = state.NextToken();
					if (next == null)
						throw new ParserException(string.Format("Expected ')' before end of expression", state.PeekToken()), state);
					else if (next.TokenType != TagTokenType.ParenClose)
						throw new ParserException(string.Format("Expected ')' instead of '{0}'", next), state);

					break;
				case TagTokenType.Not:
					expression = new NotExpression(tk, Expr(state));
					break;
				default:
					throw new ParserException(string.Format("Unexpected token '{0}'", tk), state);
			}

			return expression;
		}

		private static List<TagExpression> ArgList(ParserState state)
		{
			List<TagExpression> args = new List<TagExpression>();
			while (state.PeekTokenType() != TagTokenType.AtEof)
			{
				TagExpression expr = ArgValue(state);

				if (expr == null)
					throw new ParserException(string.Format("Expected argument value instead of '{0}'", state.PeekToken()), state);

				args.Add(expr);

				if (state.PeekTokenType() != TagTokenType.Comma)
					break;

				state.NextToken();
			}

			return args;
		}

		private static TagExpression Expr(ParserState state)
		{
			throw new Exception("The method or operation is not implemented.");
		}


	}
}
