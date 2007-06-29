using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtlePackage
{
	public class TurtleFileItem : TurtleItem
	{
		readonly string _filename;
		readonly string _fromFile;

		protected internal TurtleFileItem(string filename)
			: base(true)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");

			_filename = filename;
		}

		protected internal TurtleFileItem(string filename, string fromFile)
			: this(filename)
		{
			_fromFile = fromFile;
		}

		public string Filename
		{
			get { return _filename; }
		}

		public override string Name
		{
			get { return Filename; }
		}
	}
}
