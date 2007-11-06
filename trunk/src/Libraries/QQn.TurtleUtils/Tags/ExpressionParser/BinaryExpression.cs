using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	class BinaryExpression : TagExpression
	{
		readonly TagExpression _lhs;
		readonly TagExpression _rhs;

		public BinaryExpression(TagToken token, TagExpression lhs, TagExpression rhs)
			: base(token)
		{
			if (lhs == null)
				throw new ArgumentNullException("lhs");
			else if (rhs == null)
				throw new ArgumentNullException("rhs");

			_lhs = lhs;
			_rhs = rhs;
		}

		/// <summary>
		/// Gets all sub expressions.
		/// </summary>
		/// <value>The sub expressions.</value>
		protected internal override TagExpression[] SubExpressions
		{
			get { return new TagExpression[] { _lhs, _rhs }; }
		}

		/// <summary>
		/// Gets the left hand.
		/// </summary>
		/// <value>The left hand.</value>
		public TagExpression LeftHand
		{
			get { return _lhs; }
		}

		/// <summary>
		/// Gets the right hand.
		/// </summary>
		/// <value>The right hand.</value>
		public TagExpression RightHand
		{
			get { return _rhs; }
		}

		public override string ToString()
		{
			return "(" + LeftHand.ToString() + " " + Token.ToString() + " " + RightHand.ToString() + ")";
		}
	}
}
