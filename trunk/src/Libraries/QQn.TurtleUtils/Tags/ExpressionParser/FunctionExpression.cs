using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	class FunctionExpression : TagExpression
	{
		readonly IList<TagExpression> _args;

		public FunctionExpression(TagToken token, IList<TagExpression> args)
			: base(token)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			_args = args;
		}
	}
}
