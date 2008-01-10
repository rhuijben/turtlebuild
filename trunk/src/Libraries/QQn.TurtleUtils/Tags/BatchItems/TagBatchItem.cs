using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tags.ExpressionParser;
using System.Text.RegularExpressions;

namespace QQn.TurtleUtils.Tags.BatchItems
{
	enum PrepareMode
	{
		Condition,
		String,
		List
	}
	abstract class TagBatchItem
	{
		readonly string _definition;
		readonly Type _itemType;

		protected Type ItemType
		{
			get { return _itemType; }
		} 

		TagBatchDefinition _preparedFor;
		int _offset = -1;

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

		ValueItem[] _items;

		protected void InitPreparation(TagBatchDefinition batchDefinition, int offset)
		{
			_preparedFor = batchDefinition;
			_offset = offset;
		}

		/// <summary>
		/// Prepares the specified batch definition.
		/// </summary>
		/// <param name="batchDefinition">The batch definition.</param>
		/// <param name="offset">The offset.</param>
		protected internal virtual void Prepare(TagBatchDefinition batchDefinition, int offset)
		{
			InitPreparation(batchDefinition, offset);

			_items = PrepareString(batchDefinition, Definition, ProvidesList ? PrepareMode.List : PrepareMode.String);
		}

		protected internal static ValueItem[] PrepareString(TagBatchDefinition batchDefinition, string definition, PrepareMode mode)
		{
			int lastOffset = 0;
			int offset = 0;

			List<ValueItem> list = new List<ValueItem>();
			List<string> freeConstraints = new List<string>();

			string flat = TagExpander.ItemKeyOrPropertyRegex.Replace(definition, delegate(Match match)
			{
				if (offset > lastOffset)
					list.Add(new StringValueItem(definition.Substring(lastOffset, offset - lastOffset)));
				int ofs = match.Index - offset;
				offset = match.Index + match.Length;
				lastOffset = offset;

				Group g, gp, gs;
				if (null != (g = match.Groups[TagExpander.RegexGroupItem]) && g.Success)
				{
					gp = match.Groups[TagExpander.RegexGroupTransform];
					gs = match.Groups[TagExpander.RegexGroupSeparator];

					batchDefinition.AddUsedItem(g.Value);

					list.Add(new ItemValueItem(g.Value, gp.Success ? gp.Value : null, gs.Success ? gs.Value : null, mode));

				}
				else if (null != (g = match.Groups[TagExpander.RegexGroupKey]) && g.Success)
				{
					gp = match.Groups[TagExpander.RegexGroupItemPrefix];

					list.Add(new TagValueItem(gp.Success ? gp.Value : null, g.Value));
				}
				else if (null != (g = match.Groups[TagExpander.RegexGroupProperty]) && g.Success)
				{
					list.Add(new PropertyValueItem(g.Value));
				}
				return "";
			});

			offset = definition.Length;
			if (offset > lastOffset)
				list.Add(new StringValueItem(definition.Substring(lastOffset, offset - lastOffset)));

			return list.ToArray();
		}

		/// <summary>
		/// Posts the prepare.
		/// </summary>
		protected virtual internal void PostPrepare(TagBatchDefinition batchDefinition)
		{
			if(_items != null)
				foreach (ValueItem tv in _items)
				{
					tv.PostPrepare(batchDefinition);
				}
		}

		protected virtual string ApplyItems(TagBatchInstance instance)
		{
			StringBuilder sb = new StringBuilder();

			foreach (ValueItem vi in _items)
				vi.ApplyTo(sb, instance);

			return sb.ToString();
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

		public int Offset
		{
			get { return _offset; }
		}

		internal abstract object GetValue<TKey>(TagBatchDefinition<TKey> definition, TagBatchInstance<TKey> instance)
			where TKey : class;

		protected virtual object CreateValue(string value)
		{
			if (_itemType.IsAssignableFrom(typeof(string)))
				return value;
			else if (_itemType == typeof(ITagItem))
				return new StubItem(value);
			else if (_itemType == TagBatchDefinition.ITaskItemTypeUnsafe)
				return TagBatchDefinition.CreateTaskItem("", value);

			throw new InvalidOperationException();
		}
	}
}
