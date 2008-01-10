using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	sealed class CompareExpression : BinaryExpression
	{
		public CompareExpression(TagToken token, TagExpression lhs, TagExpression rhs)
			: base(token, lhs, rhs)
		{
		}

		internal override ExValue Evaluate<TKey>(TagBatchInstance<TKey> instance)
		{
			ExValue left = LeftHand.Evaluate(instance);
			ExValue right = RightHand.Evaluate(instance);

			switch (Token.TokenType)
			{
				case TagTokenType.IsEqual:
					return new ExValue(left.Equals(right));
				case TagTokenType.IsNot:
					return new ExValue(!left.Equals(right));
				case TagTokenType.IsLte:
					return new ExValue(left.CompareTo(right) <= 0);
				case TagTokenType.IsLessThan:
					return new ExValue(left.CompareTo(right) < 0);
				case TagTokenType.IsGreaterThan:
					return new ExValue(left.CompareTo(right) > 0);
				case TagTokenType.IsGte:
					return new ExValue(left.CompareTo(right) >= 0);
				default:
					throw new InvalidOperationException();
			}
		}
	}
}
