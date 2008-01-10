using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	sealed class ParenExpression : UnaryExpression
	{
		public ParenExpression(TagToken token, TagExpression inner)
			: base(token, inner)
		{
		}

		public override string ToString()
		{
			return InnerExpression.ToString();
		}

		internal override ExValue Evaluate<TKey>(TagBatchInstance<TKey> instance)
		{
			return InnerExpression.Evaluate(instance);
		}
	}
}
