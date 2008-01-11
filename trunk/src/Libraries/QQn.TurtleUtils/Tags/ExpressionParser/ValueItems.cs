using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using QQn.TurtleUtils.Tags.BatchItems;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	abstract class ValueItem
	{
		/// <summary>
		/// Converts the item to a string for the specified instance
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <returns></returns>
		public abstract string ToString(TagBatchInstance instance);

		internal virtual void PostPrepare(TagBatchDefinition batchDefinition, TagBatchItem batchItem)
		{

			//throw new NotImplementedException();
		}

		/// <summary>
		/// Applies the ValueItem to the specified StringBuilder
		/// </summary>
		/// <param name="to">To.</param>
		/// <param name="instance">The instance.</param>
		internal virtual void ApplyTo(StringBuilder to, TagBatchInstance instance)
		{
			to.Append(ToString(instance));
		}
	}

	[DebuggerDisplay("StringValue Value={Value}")]
	sealed class StringValueItem : ValueItem
	{
		readonly string _value;

		public StringValueItem(string value)
		{
			_value = value;
		}

		/// <summary>
		/// Converts the item to a string for the specified instance
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <returns></returns>
		public override string ToString(TagBatchInstance instance)
		{
			return _value;
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <value>The value.</value>
		public string Value
		{
			get { return _value; }
		}

		internal override void ApplyTo(StringBuilder to, TagBatchInstance items)
		{
			to.Append(_value);
		}
	}

	[DebuggerDisplay("PropertyValue Property={PropertyName}")]
	sealed class PropertyValueItem : ValueItem
	{
		readonly string _propertyName;
		public PropertyValueItem(string propertyName)
		{
			_propertyName = propertyName;
		}

		public override string ToString(TagBatchInstance instance)
		{
			TagPropertyCollection tpc = instance.Context.Properties;

			return tpc.Contains(PropertyName) ? tpc[PropertyName].ExpandedValue() : "";
		}

		/// <summary>
		/// Gets the name of the property.
		/// </summary>
		/// <value>The name of the property.</value>
		public string PropertyName
		{
			get { return _propertyName; }
		}
	}

	[DebuggerDisplay("TagValue Item={Item}, Key={Key}")]
	class TagValueItem : ValueItem
	{
		readonly string _key;
		readonly string _item;

		public TagValueItem(string item, string key)
		{
			// Item = null -> To be filled in
			// Key = null -> Use identity
			_item = item;
			_key = key;
		}

		public override string ToString(TagBatchInstance instance)
		{
			return instance.GetKeyValue(_item, _key);
		}

		public string Key
		{
			get { return _key; }
		}

		public string Item
		{
			get { return _item; }
		}

		protected virtual bool IsConstraint
		{
			get { return true; }
		}

		internal override void PostPrepare(TagBatchDefinition batchDefinition, TagBatchItem batchItem)
		{
			base.PostPrepare(batchDefinition, batchItem);

			if(IsConstraint)
				batchDefinition.AddConstraint(Item, Key);
		}
	}

	[DebuggerDisplay("ItemValue Item={Item}, Key={Key}, Separator={Separator}")]
	sealed class ItemValueItem : TagValueItem
	{
		readonly string _separator;
		readonly PrepareMode _mode;

		public ItemValueItem(string item, string key, string separator, PrepareMode mode)
			: base(item, key)
		{
			if (string.IsNullOrEmpty(item))
				throw new ArgumentNullException("item");

			_separator = separator;
			_mode = mode;
		}

		public override string ToString(TagBatchInstance instance)
		{
			return instance.GetItemValue(Item, Key, Separator ?? ";");
		}

		public string Separator
		{
			get { return _separator; }
		}

		protected override bool IsConstraint
		{
			get
			{
				return false;
			}
		}

		ValueItem[] _parts;
		internal override void PostPrepare(TagBatchDefinition batchDefinition, TagBatchItem batchItem)
		{
			base.PostPrepare(batchDefinition, batchItem);

			if (Key != null)
				_parts = batchItem.PrepareString(batchDefinition, Key, _mode);
		}
	}
}
