using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tags.BatchItems;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	sealed class DynamicStringExpression : StringExpression
	{
		ValueItem[] _parts;

		public DynamicStringExpression(TagToken token)
			: base(token)
		{
		}

		internal void Prepare(TagBatchDefinition batchDefinition)
		{
			_parts = TagBatchItem.PrepareString(batchDefinition, Value, PrepareMode.Condition);
		}

		/// <summary>
		/// Determines whether the specified token is a dynamic string
		/// </summary>
		/// <param name="tk">The tk.</param>
		/// <returns>
		/// 	<c>true</c> if the specified tk is dynamic; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsDynamic(TagToken tk)
		{
			string value = tk.Value;

			return value.StartsWith("\'") && value.EndsWith("\'") && TagExpander.ItemKeyOrPropertyRegex.Match(value).Success;
		}
	}
}
