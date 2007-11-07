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
				ResolveAndOrConflicts(expr, state);

			return expr;
		}

		internal static void ResolveAndOrConflicts(TagExpression expr, ParserState state)
		{
			if (expr == null)
				throw new ArgumentNullException("expr");

			AndOrExpression andOr = expr as AndOrExpression;
			if(andOr != null && (andOr.LeftHand is AndOrExpression || andOr.RightHand is AndOrExpression))
			{
				List<TagToken> tokens = new List<TagToken>();
				List<TagExpression> exprs = new List<TagExpression>();

				AddAndOr(andOr, tokens, exprs); // Create a list of tokens and separating expressions

				if (exprs.Count != tokens.Count + 1)
					throw new InvalidOperationException(); // Not a valid token chain

				TagTokenType tt = tokens[0].TokenType;

				bool hasConflict = false;
				for(int i = 1; i < tokens.Count; i++)
					if(tokens[i].TokenType != tt)
					{
						if(!state.Args.ApplyAndOrPriority)
							throw new PriorityException("And or conflict; please resolve using parens", tokens[i], state);
						else
						{
							hasConflict = true;
						}
					}

				if (hasConflict)
				{
					// We re-orden the children to prioritize 'and' above 'or'

					// We assume: we have at least one 'and' at least one 'or'

					int i;

					// Re-create all groups of and-s, from the back
					while (0 <= (i = LastToken(tokens, TagTokenType.And)))
					{
						exprs[i] = new AndOrExpression(tokens[i], exprs[i], exprs[i + 1]);
						tokens.RemoveAt(i);
						exprs.RemoveAt(i + 1);
					}

					// Re-create all groups of or-s, from the back
					while (1 <= (i = LastToken(tokens, TagTokenType.Or)))
					{
						exprs[i] = new AndOrExpression(tokens[i], exprs[i], exprs[i + 1]);
						tokens.RemoveAt(i);
						exprs.RemoveAt(i + 1);
					}

					if (exprs.Count != 2 && tokens.Count != 1)
						throw new InvalidOperationException();

					andOr.ForceExpression(tokens[0], exprs[0], exprs[1]);
				}				
			}

			
			TagExpression[] te = expr.SubExpressions;

			if (te != null)
			{				
				foreach (TagExpression ee in te)
					ResolveAndOrConflicts(ee, state);
			}
		}

		private static int LastToken(List<TagToken> tokens, TagTokenType tokenType)
		{
			for (int i = tokens.Count - 1; i >= 0; i--)
			{
				if (tokens[i].TokenType == tokenType)
					return i;
			}
			return -1;
		}

		private static void AddAndOr(TagExpression tagExpression, List<TagToken> tokens, List<TagExpression> exprs)
		{
			AndOrExpression andOr = tagExpression as AndOrExpression;

			if (andOr != null)
			{
				AddAndOr(andOr.LeftHand, tokens, exprs);
				tokens.Add(andOr.Token);
				AddAndOr(andOr.RightHand, tokens, exprs);
			}
			else
				exprs.Add(tagExpression);
		}

		internal static TagExpression CompleteExpression(ParserState state)
		{
			TagExpression expr = Expr(state);

			while (state.PeekTokenType() != TagTokenType.AtEof)
			{
				TagToken tk = state.NextToken();
				switch (tk.TokenType)
				{
					case TagTokenType.Or:
						expr = new AndOrExpression(tk, expr, BooleanExpression(state));
						break;

					case TagTokenType.And:
						expr = new AndOrExpression(tk, expr, BooleanExpression(state));
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

			TagToken token;
			switch (state.PeekTokenType())
			{
				case TagTokenType.IsEqual:
				case TagTokenType.IsNot:
				case TagTokenType.IsLte:
				case TagTokenType.IsLt:
				case TagTokenType.IsGt:
				case TagTokenType.IsGte:
					token = state.NextToken();

					return new CompareExpression(token, baseExpression, Part(state));				

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
			TagExpression expr = BooleanExpression(state);

			switch(state.PeekTokenType())
			{
				case TagTokenType.Or:
					return new AndOrExpression(state.NextToken(), expr, BooleanExpression(state));
				case TagTokenType.And:
					return new AndOrExpression(state.NextToken(), expr, BooleanExpression(state));
				default:
					return expr;
			}
		}


	}
}
