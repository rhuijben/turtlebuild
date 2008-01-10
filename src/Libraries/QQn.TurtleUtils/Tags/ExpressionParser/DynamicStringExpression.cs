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

		internal override ExValue Evaluate<TKey>(TagBatchInstance<TKey> instance)
		{
			StringBuilder sb = new StringBuilder();
			foreach (ValueItem vi in _parts)
			{
				vi.ApplyTo(sb, instance);
			}

			return new ExValue(sb.ToString());
		}

		internal void PostPrepare(TagBatchDefinition batchDefinition)
		{
			foreach (ValueItem vi in _parts)
				vi.PostPrepare(batchDefinition);
		}
	}
}
