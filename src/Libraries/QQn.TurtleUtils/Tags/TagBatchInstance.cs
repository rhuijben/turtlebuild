using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tags.BatchItems;
using QQn.TurtleUtils.Items;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class TagBatchInstance
	{
		readonly TagBatchDefinition _definition;
		readonly TagContext _context;
		readonly object[] _values;
		string[] _cValues;
		IList<TagItemCollection> _items;

		internal TagBatchInstance(TagContext context, TagBatchDefinition definition)
		{
			if (context == null)
				throw new ArgumentNullException("context");
			else if (definition == null)
				throw new ArgumentNullException("definition");

			_definition = definition;
			_context = context;
			_values = new object[definition.ItemCount];
		}

		/// <summary>
		/// Gets the <see cref="System.Object"/> with the specified key.
		/// </summary>
		/// <value></value>
		public object this[object key]
		{
			get { return GetValue(key); }
		}

		/// <summary>
		/// Gets the value of the specified key
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		protected abstract object GetValue(object key);

		/// <summary>
		/// Gets the batch definition.
		/// </summary>
		/// <value>The batch definition.</value>
		public TagBatchDefinition BatchDefinition
		{
			get { return _definition; }
		}

		/// <summary>
		/// Gets the context.
		/// </summary>
		/// <value>The context.</value>
		public TagContext Context
		{
			get { return _context; }
		}

		/// <summary>
		/// Gets the properties.
		/// </summary>
		/// <value>The properties.</value>
		public TagPropertyCollection Properties
		{
			get { return Context.Properties; }
		}

		/// <summary>
		/// Gets the default item.
		/// </summary>
		/// <value>The default item.</value>
		public string DefaultItem
		{
			get { return null; }
		}

		/// <summary>
		/// Gets the item.
		/// </summary>
		/// <param name="itemName">Name of the item.</param>
		/// <returns></returns>
		public TagItemCollection GetItems(string itemName)
		{
			if (itemName == null)
				throw new ArgumentNullException("itemName");

			if (itemName == DefaultItem)
				return Items[0];

			int n = 0;
			foreach (string s in _definition.ItemsUsed)
			{
				if (itemName == s)
				{
					return _items[n];
				}
				n++;
			}

			return null;
		}

		internal string GetItemValue(string item, string Key, string separator)
		{
			TagItemCollection items = GetItems(item ?? DefaultItem);

			if (items == null || items.Count == 0)
				return null;

			if (items.Count == 1)
				return items[0].ExpandedValue(Context, Key);

			if (separator == null)
				throw new InvalidOperationException();

			StringBuilder sb = new StringBuilder();
			bool first = true;
			foreach (TagItem i in items)
			{
				if (first)
					first = false;
				else
					sb.Append(separator);

				sb.Append(i.ExpandedValue(Context, Key));
			}

			return sb.ToString();
		}

		internal string GetKeyValue(string item, string key)
		{
			return _cValues[_definition.GetConstraintOffset(item, key)];
		}

		/// <summary>
		/// Fills the specified items.
		/// </summary>
		/// <param name="items">The items.</param>
		/// <param name="constraintValues">The constraint values.</param>
		internal protected void Fill(AutoKeyedCollection<string, TagItemCollection> items, string[] constraintValues)
		{
			ClearValues();
			_items = items;
			_cValues = constraintValues;
		}

		/// <summary>
		/// Clears the values.
		/// </summary>
		protected void ClearValues()
		{
			for (int i = 0; i < _values.Length; i++)
				_values[i] = null;
		}

		/// <summary>
		/// Gets the values.
		/// </summary>
		/// <value>The values.</value>
		protected object[] Values
		{
			get { return _values; }
		}

		/// <summary>
		/// Gets the items.
		/// </summary>
		/// <value>The items.</value>
		protected IList<TagItemCollection> Items
		{
			get { return _items; }
		}

		/// <summary>
		/// Gets the result of all evaluated conditions
		/// </summary>
		/// <returns></returns>
		public abstract bool ConditionResult();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	public class TagBatchInstance<TKey> : TagBatchInstance
		where TKey : class
	{
		readonly TagBatchDefinition<TKey> _definition;
		internal TagBatchInstance(TagContext context, TagBatchDefinition<TKey> definition)
			: base(context, definition)
		{
			_definition = definition;
		}

		/// <summary>
		/// Gets the <see cref="System.Object"/> with the specified key.
		/// </summary>
		/// <value></value>
		public object this[TKey key]
		{
			get
			{
				TagBatchItem tbi = _definition[key];

				object value = Values[tbi.Offset];
				if (value == null)
					return (Values[tbi.Offset] = tbi.GetValue(_definition, this));

				return value;
			}
		}

		/// <summary>
		/// Conditions the result.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public bool ConditionResult(TKey key)
		{
			return (bool)_definition[key].GetValue(_definition, this);
		}

		/// <summary>
		/// Gets the value of the specified key
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		protected override object GetValue(object key)
		{
			return this[(TKey)key];
		}

		/// <summary>
		/// Gets the batch definition.
		/// </summary>
		/// <value>The batch definition.</value>
		public new TagBatchDefinition<TKey> BatchDefinition
		{
			get { return _definition; }
		}

		/// <summary>
		/// Gets the result of all evaluated conditions
		/// </summary>
		/// <returns></returns>
		public override bool ConditionResult()
		{
			foreach (TagBatchItem tbi in _definition.AllValues)
			{
				TagConditionItem tci = tbi as TagConditionItem;

				if ((tci != null) && !tci.EvaluateCondition(this))
					return false;
			}

			return true;
		}
	}
}
