using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tags.BatchItems;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	abstract class ValuePart
	{
		public abstract void ApplyTo(List<ITagItem> tagItems, TagBatchInstance instance);

		public virtual void PostPrepare(TagBatchDefinition batchDefinition)
		{
		}
	}

	sealed class ConstantValuePart : ValuePart
	{
		readonly string _value;

		public ConstantValuePart(string value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			_value = value;
		}

		public override void ApplyTo(List<ITagItem> tagItems, TagBatchInstance instance)
		{
			tagItems.Add(new StubItem(_value));
		}
	}

	sealed class DynamicValuePart : ValuePart
	{
		readonly ValueItem[] _items;

		public DynamicValuePart(ValueItem[] items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			_items = items;
		}

		public override void ApplyTo(List<ITagItem> tagItems, TagBatchInstance instance)
		{
			StringBuilder sb = new StringBuilder();
			foreach (ValueItem vi in _items)
				vi.ApplyTo(sb, instance);

			tagItems.Add(new StubItem(sb.ToString()));
		}

		public override void PostPrepare(TagBatchDefinition batchDefinition)
		{
			base.PostPrepare(batchDefinition);

			foreach (ValueItem vi in _items)
				vi.PostPrepare(batchDefinition);
		}
	}

	sealed class ItemValuePart : ValuePart
	{
		ItemValueItem _item;
		public ItemValuePart(ItemValueItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			_item = item;
		}

		public override void ApplyTo(List<ITagItem> tagItems, TagBatchInstance instance)
		{
			TagItemCollection items = instance.GetItems(_item.Item);

			foreach (TagItem ti in items)
			{
				if (string.IsNullOrEmpty(_item.Key))
					tagItems.Add(ti);
				else
				{
					throw new NotImplementedException();
				}
			}
		}

		public override void PostPrepare(TagBatchDefinition batchDefinition)
		{
			base.PostPrepare(batchDefinition);

			_item.PostPrepare(batchDefinition);
		}
	}

}
