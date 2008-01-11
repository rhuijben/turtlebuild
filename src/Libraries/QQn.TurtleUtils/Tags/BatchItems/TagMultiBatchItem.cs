using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tags.ExpressionParser;

namespace QQn.TurtleUtils.Tags.BatchItems
{
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

		internal override void PrePrepare(TagBatchDefinition batchDefinition)
		{
			base.PrePrepare(batchDefinition);
		}

		ValuePart[] _parts;


		protected internal override void Prepare(TagBatchDefinition batchDefinition, int offset)
		{
			InitPreparation(batchDefinition, offset);

			List<ValuePart> parts = new List<ValuePart>();

			foreach (string item in TagExpander.SplitItemList(Definition))
			{
				ValueItem[] vi = PrepareString(batchDefinition, item, PrepareMode.List);

				if (vi.Length == 1)
				{
					ValueItem v = vi[0];

					ItemValueItem ivi;
					StringValueItem svi;
					if (null != (ivi = v as ItemValueItem))
					{
						if (string.IsNullOrEmpty(ivi.Separator) || ivi.Separator.Trim() == ";")
						{
							parts.Add(new ItemValuePart(ivi));
							continue;
						}
					}
					else if(null != (svi = v as StringValueItem))
					{
						// TODO: Shortcut single string value
						parts.Add(new ConstantValuePart(svi.Value));
						continue;
					}
				}

				parts.Add(new DynamicValuePart(vi));
			}

			_parts = parts.ToArray();
		}

		protected internal override void PostPrepare(TagBatchDefinition batchDefinition)
		{
			base.PostPrepare(batchDefinition);

			foreach (ValuePart vp in _parts)
				vp.PostPrepare(batchDefinition, this);
		}

		internal override object GetValue<TKey>(TagBatchDefinition<TKey> definition, TagBatchInstance<TKey> instance)
		{
			List<ITagItem> items = new List<ITagItem>();

			foreach (ValuePart vp in _parts)
				vp.ApplyTo(items, instance);

			return CreateValue(items);			
		}

		private object CreateValue(List<ITagItem> items)
		{
			Array a = Array.CreateInstance(ItemType, items.Count);

			if (ItemType == typeof(string))
			{
				for (int i = 0; i < items.Count; i++)
					a.SetValue(items[i].ItemSpec, i);
			}
			else if (ItemType == typeof(ITagItem))
			{
				for (int i = 0; i < items.Count; i++)
					a.SetValue(items[i], i);
			}
			else
			{
				throw new NotImplementedException();
			}

			return a;
		}

		/// <summary>
		/// Creates the value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected override object CreateValue(string value)
		{
			IList<string> items = TagExpander.SplitItemList(value);
			Array a = Array.CreateInstance(ItemType, items.Count);

			for (int i = 0; i < items.Count; i++)
				a.SetValue(base.CreateValue(items[i]), i);

			return a;
		}
	}
}
