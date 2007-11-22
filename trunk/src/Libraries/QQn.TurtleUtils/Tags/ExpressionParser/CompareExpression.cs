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
	}
}