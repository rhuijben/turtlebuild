using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Streams
{
	/// <summary>
	/// 
	/// </summary>
	public class MultiStreamArgs
	{
		bool _assured;
		bool _zipped;
		short _itemType;

		/// <summary>
		/// Free short for marking substreams; Allowed values are 0-4095
		/// </summary>
		public short StreamType
		{
			get { return _itemType; }
			set
			{
				if ((value < 0) || (value > MultiStreamItemHeader.TypeMask))
					throw new ArgumentOutOfRangeException("value", value, "Value must be between 0 and 4096");
				_itemType = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool Assured
		{
			get { return _assured; }
			set { _assured = value; }
		}		

		/// <summary>
		/// 
		/// </summary>
		public bool GZipped
		{
			get { return _zipped; }
			set { _zipped = value; }
		}
	}
}
