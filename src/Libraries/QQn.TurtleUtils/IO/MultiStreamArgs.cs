using System;
using System.Collections.Generic;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// Create argument of a substream within a MultiStream
	/// </summary>
	public class MultiStreamArgs
	{
		bool _assured;
		bool _zipped;
		int _itemType;
		long _fixedLength;

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiStreamArgs"/> class.
		/// </summary>
		public MultiStreamArgs()
		{
			_fixedLength = -1;
		}

		/// <summary>
		/// Free short for marking substreams; Allowed values are 0-16777215
		/// </summary>
		/// <value>The type of the stream.</value>
		public int StreamType
		{
			get { return _itemType; }
			set
			{
				if ((value < 0) || (value > 0xFFFFFF))
					throw new ArgumentOutOfRangeException("value", value, "Value must be between 0 and 16777216");
				_itemType = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="MultiStreamArgs"/> is assured.
		/// </summary>
		/// <value><c>true</c> if assured; otherwise, <c>false</c>.</value>
		/// <remarks>Assured streams are seekable if the parent stream is seekable</remarks>
		public bool Assured
		{
			get { return _assured; }
			set { _assured = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the substream should be GZipped.
		/// </summary>
		/// <value><c>true</c> if GZipped; otherwise, <c>false</c>.</value>
		/// <remarks>GZipped substreams are never seekable</remarks>
		public bool GZipped
		{
			get { return _zipped; }
			set { _zipped = value; }
		}

		/// <summary>
		/// Gets or sets the fixed length.
		/// </summary>
		/// <value>The fixed length.</value>
		public long FixedLength
		{
			get { return _fixedLength; }
			set 
			{
				if (value < 0)
					_fixedLength = -1;
				else
					_fixedLength = value; 
			}
		}
	}
}
