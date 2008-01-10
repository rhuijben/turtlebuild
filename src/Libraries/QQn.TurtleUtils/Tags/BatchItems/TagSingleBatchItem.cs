using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.BatchItems
{
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

		internal override object GetValue<TKey>(TagBatchDefinition<TKey> definition, TagBatchInstance<TKey> instance)
		{
			string s = base.ApplyItems(instance);
			//throw new NotImplementedException();

			return CreateValue(s);
		}
	}
}
