using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tags.ExpressionParser;

namespace QQn.TurtleUtils.Tags.BatchItems
{
	sealed class TagConditionItem : TagBatchItem
	{
		TagExpression _expression;

		public TagConditionItem(string definition)
			: base(definition, typeof(bool))
		{
		}

		public override bool ProvidesList
		{
			get { return false; }
		}

		internal override void PrePrepare(TagBatchDefinition batchDefinition)
		{
			base.PrePrepare(batchDefinition);

			if (_expression == null)
				_expression = QQn.TurtleUtils.Tags.ExpressionParser.Parser.Parse(Definition, ConditionArgs);
		}

		static QQn.TurtleUtils.Tags.ExpressionParser.ParserArgs _parserArgs;

		public static QQn.TurtleUtils.Tags.ExpressionParser.ParserArgs ConditionArgs
		{
			get
			{
				if (_parserArgs == null)
				{
					_parserArgs = new QQn.TurtleUtils.Tags.ExpressionParser.ParserArgs();
					_parserArgs.AllowItems = false;
					_parserArgs.AllowProperties = true;
					_parserArgs.AllowTags = true;
				}
				return _parserArgs;
			}
		}

		protected internal override void Prepare(TagBatchDefinition batchDefinition, int offset)
		{
			InitPreparation(batchDefinition);

			foreach (TagExpression te in _expression.GetAllLeaveExpressions())
			{
				DynamicStringExpression dse = te as DynamicStringExpression;

				if (dse != null)
					dse.Prepare(batchDefinition);
			}

			base.Prepare(batchDefinition, offset);
		}

		protected internal override void PostPrepare(TagBatchDefinition batchDefinition)
		{
			base.PostPrepare(batchDefinition);

			foreach (TagExpression te in _expression.GetAllLeaveExpressions())
			{
				DynamicStringExpression dse = te as DynamicStringExpression;

				if (dse != null)
					dse.PostPrepare(batchDefinition);
			}

		}

		public override bool IsConstant
		{
			get { return false; }
		}

		internal override object GetValue<TKey>(TagBatchDefinition<TKey> definition, TagBatchInstance<TKey> instance)
		{
			return EvaluateCondition(instance);
		}

		internal bool EvaluateCondition<TKey>(TagBatchInstance<TKey> instance)
			where TKey : class
		{
			return _expression.Evaluate(instance).ToBool();
		}
	}
}
