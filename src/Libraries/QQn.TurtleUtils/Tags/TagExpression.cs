using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tags.ExpressionParser;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	public class TagExpression
	{
		readonly TagToken _token;

		/// <summary>
		/// Initializes a new instance of the <see cref="TagExpression"/> class.
		/// </summary>
		/// <param name="token">The token.</param>
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
		public virtual TagToken Token
		{
			get { return _token; }
		}

		/// <summary>
		/// Gets the type of the token.
		/// </summary>
		/// <value>The type of the token.</value>
		public virtual TagTokenType TokenType
		{
			get { return Token.TokenType; }
		}


		static readonly IEnumerable<TagExpression> _emptyEnumerable = new TagExpression[0];
		/// <summary>
		/// Gets all sub expressions.
		/// </summary>
		/// <value>The sub expressions.</value>
		protected internal virtual IEnumerable<TagExpression> SubExpressions
		{
			get { return _emptyEnumerable; }
		}
	}
}
