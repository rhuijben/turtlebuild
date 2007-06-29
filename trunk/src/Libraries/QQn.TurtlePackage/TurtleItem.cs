using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtlePackage
{
	public abstract class TurtleItem
	{
		bool _readOnly;

		public TurtleItem(bool readOnly)
		{
			_readOnly = false;
		}

		public bool ReadOnly
		{
			get { return _readOnly; }
		}

		public abstract string Name
		{
			get;
		}

		internal void EnsureWritable()
		{
			if(ReadOnly)
				throw new InvalidOperationException(string.Format("{0} '{1}' is not writable", GetType().Name, Name));
		}

		protected void SetReadOnly()
		{
			_readOnly = true;
		}
	}
}
