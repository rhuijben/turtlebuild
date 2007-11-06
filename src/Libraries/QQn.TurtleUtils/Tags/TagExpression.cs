using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tags.ExpressionParser;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	public enum TagValueType
	{
		/// <summary>
		/// 
		/// </summary>
		String,
		/// <summary>
		/// 
		/// </summary>
		Number,
		/// <summary>
		/// 
		/// </summary>
		Bool
	}

	/// <summary>
	/// 
	/// </summary>
	public class TagExpression
	{
		readonly TagToken _token;

		protected TagExpression(TagToken token)
		{
			if (token == null)
				throw new ArgumentNullException("token");

			_token = token;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return "(" + Token.ToString() + ")";
		}

		/// <summary>
		/// Gets the token.
		/// </summary>
		/// <value>The token.</value>
		public TagToken Token
		{
			get { return _token; }
		}

		/// <summary>
		/// Gets the type of the token.
		/// </summary>
		/// <value>The type of the token.</value>
		public virtual TagTokenType TokenType
		{
			get { return _token.TokenType; }
		}

		/// <summary>
		/// Gets all sub expressions.
		/// </summary>
		/// <value>The sub expressions.</value>
		protected internal virtual TagExpression[] SubExpressions
		{
			get { return null; }
		}
	}
}
