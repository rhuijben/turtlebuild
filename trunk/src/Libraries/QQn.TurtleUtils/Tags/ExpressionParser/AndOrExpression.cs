using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	sealed class AndOrExpression : BinaryExpression
	{
		bool _isAnd;
		TagToken _token;

		public AndOrExpression(TagToken token, TagExpression lhs, TagExpression rhs)
			: base(token, lhs, rhs)
		{
			ForceExpression(token, lhs, rhs);			
		}

		/// <summary>
		/// Gets a value indicating whether this instance is an AND expression.
		/// </summary>
		/// <value><c>true</c> if this instance is and; otherwise, <c>false</c>.</value>
		public bool IsAnd
		{
			get { return _isAnd; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is an OR expression.
		/// </summary>
		/// <value><c>true</c> if this instance is or; otherwise, <c>false</c>.</value>
		public bool IsOr
		{
			get { return !_isAnd; }
		}

		/// <summary>
		/// Gets the token.
		/// </summary>
		/// <value>The token.</value>
		public override TagToken Token
		{
			get
			{
				return _token ?? base.Token;
			}
		}

		internal void ForceExpression(TagToken token, TagExpression lhs, TagExpression rhs)
		{
			switch (token.TokenType)
			{
				case TagTokenType.And:
					_isAnd = true;
					break;
				case TagTokenType.Or:
					break;
				default:
					throw new ArgumentException("Only AND and OR are allowed");
			}
			_token = token;
			SetEditable(true);
			LeftHand = lhs;
			RightHand = rhs;
			SetEditable(false);
		}
	}
}
