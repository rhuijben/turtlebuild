using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	sealed class NotExpression : UnaryExpression
	{
		public NotExpression(TagToken token, TagExpression inner)
			: base(token, inner)
		{
		}
	}
}
