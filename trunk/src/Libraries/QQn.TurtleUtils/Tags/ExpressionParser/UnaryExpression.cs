using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	class UnaryExpression : TagExpression
	{
		TagExpression _inner;

		public UnaryExpression(TagToken token, TagExpression inner)
			: base(token)
		{
			if (inner == null)
				throw new ArgumentNullException("inner");
			_inner = inner;
		}

		/// <summary>
		/// Gets all sub expressions.
		/// </summary>
		/// <value>The sub expressions.</value>
		protected internal override TagExpression[] SubExpressions
		{
			get { return new TagExpression[] { _inner }; }
		}

		/// <summary>
		/// Gets the inner expression.
		/// </summary>
		/// <value>The inner expression.</value>
		public TagExpression InnerExpression
		{
			get { return _inner; }
		}

		public override string ToString()
		{
			return Token.ToString() + "(" + InnerExpression.ToString() + ")";
		}
	}
}
