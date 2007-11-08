using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	sealed class DynamicStringExpression : StringExpression
	{
		public DynamicStringExpression(TagToken token)
			: base(token)
		{
		}
	}
}
