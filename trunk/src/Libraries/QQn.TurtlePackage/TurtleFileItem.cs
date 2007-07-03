using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.ItemSets;

namespace QQn.TurtlePackage
{
	public class TurtleItem : Item<TurtlePackage, TurtleContainer, TurtleItem>
	{
		readonly string _filename;
		readonly string _fromFile;

		public TurtleItem()
		{
		}

		protected internal TurtleItem(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");

			_filename = filename;
		}

		protected internal TurtleItem(string filename, string fromFile)
			: this(filename)
		{
			_fromFile = fromFile;
		}

		public string Filename
		{
			get { return _filename; }
		}
	}
}
