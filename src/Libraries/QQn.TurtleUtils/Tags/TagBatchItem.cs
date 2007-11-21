using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags
{
	class TagBatchItem
	{
		readonly string _definition;
		Type _itemType;
		public TagBatchItem(string definition, Type itemType)
		{
			if (string.IsNullOrEmpty(definition))
				throw new ArgumentNullException("definition");

			_definition = definition;
			_itemType = itemType;
		}
	}

	class TagMultiBatchItem : TagBatchItem
	{
		public TagMultiBatchItem(string definition, Type elementType)
			: base(definition, elementType)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");
		}
	}

	class TagSingleBatchItem : TagBatchItem
	{
		public TagSingleBatchItem(string definition, Type itemType)
			: base(definition, itemType)
		{
			if (itemType == null)
				throw new ArgumentNullException("itemType");
		}
	}

	class TagConditionItem : TagBatchItem
	{
		public TagConditionItem(string definition)
			: base(definition, typeof(bool))
		{
		}
	}
}
