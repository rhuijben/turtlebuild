using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using QQn.TurtleUtils.ItemSets;
using QQn.TurtleUtils.Tokenizer;


namespace QQn.TurtlePackage
{
	public class PackContainer : PackItem
	{
		public PackContainer()
		{
		}

		PackFileCollection _files;

		[TokenGroup("Item")]
		public virtual PackFileCollection Files
		{
			get { return _files ?? (_files = new PackFileCollection(this)); }
		}

	}
}
