using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags
{
	abstract class TagBatchItem
	{
		readonly string _definition;
		Type _itemType;
		TagBatchDefinition _preparedFor;
		public TagBatchItem(string definition, Type itemType)
		{
			if (string.IsNullOrEmpty(definition))
				throw new ArgumentNullException("definition");

			_definition = definition;
			_itemType = itemType;
		}

		internal virtual void PrePrepare(TagBatchDefinition batchDefinition)
		{
			if (_preparedFor != batchDefinition)
			{
				batchDefinition.Prepared = false;
				_preparedFor = null;
			}
		}

		protected internal virtual void Prepare(TagBatchDefinition batchDefinition)
		{
			_preparedFor = batchDefinition;
		}

		/// <summary>
		/// Gets the definition.
		/// </summary>
		/// <value>The definition.</value>
		public string Definition
		{
			get { return _definition; }
		}

		/// <summary>
		/// Gets a value indicating whether the item provides a list of results
		/// </summary>
		/// <value><c>true</c> if [provides list]; otherwise, <c>false</c>.</value>
		public abstract bool ProvidesList { get; }

		public abstract bool IsConstant { get; }
	}

	sealed class TagMultiBatchItem : TagBatchItem
	{
		public TagMultiBatchItem(string definition, Type elementType)
			: base(definition, elementType)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");
		}

		public override bool ProvidesList
		{
			get { return true; }
		}

		public override bool IsConstant
		{
			get { return false; }
		}
	}

	sealed class TagSingleBatchItem : TagBatchItem
	{
		public TagSingleBatchItem(string definition, Type itemType)
			: base(definition, itemType)
		{
			if (itemType == null)
				throw new ArgumentNullException("itemType");
		}

		public override bool ProvidesList
		{
			get { return false; }
		}

		public override bool IsConstant
		{
			get { return false; }
		}
	}

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

		protected internal override void Prepare(TagBatchDefinition batchDefinition)
		{
			base.Prepare(batchDefinition);

			if (_expression == null)
				_expression = QQn.TurtleUtils.Tags.ExpressionParser.Parser.Parse(Definition, ConditionArgs);
		}

		public override bool IsConstant
		{
			get { return false; }
		}
	}
}
