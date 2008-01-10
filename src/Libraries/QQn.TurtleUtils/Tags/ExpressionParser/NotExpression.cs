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

		internal override ExValue Evaluate<TKey>(TagBatchInstance<TKey> instance)
		{
			return new ExValue(!InnerExpression.Evaluate(instance).ToBool());
		}
	}
}
