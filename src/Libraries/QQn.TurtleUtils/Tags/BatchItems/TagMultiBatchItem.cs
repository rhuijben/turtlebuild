using System;
using System.Collections.Generic;
using System.Text;

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

		internal override object GetValue<TKey>(TagContext context, TagBatchDefinition<TKey> definition, TagBatchInstance<TKey> instance)
		{
			string s = base.ApplyItems(context, instance);
			//throw new NotImplementedException();

			return CreateValue(s);
		}

		/// <summary>
		/// Creates the value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected override object CreateValue(string value)
		{
			string[] items = value.Split(';');
			Array a = Array.CreateInstance(ItemType, items.Length);

			for (int i = 0; i < items.Length; i++)
				a.SetValue(base.CreateValue(items[i]), i);

			return a;
		}
	}
}
