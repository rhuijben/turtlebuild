using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.BatchItems
{
	sealed class StubItem : ITagItem
	{
		readonly string _itemSpec;
		public StubItem(string itemSpec)
		{
			if (string.IsNullOrEmpty("itemSpec"))
				throw new ArgumentNullException("itemSpec");

			_itemSpec = itemSpec;
		}
		#region ITagItem Members

		public string ItemSpec
		{
			get { return _itemSpec; }
			set { throw new InvalidOperationException(); }
		}

		public int MetadataCount
		{
			get { return 0; }
		}

		public ICollection<string> KeyNames
		{
			get { return null; }
		}

		public string this[string keyName]
		{
			get { return null; }
			set { throw new InvalidOperationException(); }
		}

		public void RemoveKey(string keyName)
		{
			throw new InvalidOperationException();
		}

		#endregion
	}
}
 