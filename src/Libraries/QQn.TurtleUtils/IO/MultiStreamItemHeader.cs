using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace QQn.TurtleUtils.IO
{
	class MultiStreamItemHeader
	{
		long _offset;
		long _length;
		int _itemType;

		public MultiStreamItemHeader()
		{
		}

		public const int ItemSize = (1 + 8 + 4 + 4) + 1; // 1 more than length of fields

		internal MultiStreamItemHeader(QQnBinaryReader reader)
		{
			byte version = reader.ReadByte();
			
			if (version == 1)
			{
				_offset = reader.ReadInt64();
				_length = reader.ReadUInt32(); // As uint
				_itemType = reader.ReadInt32();
			}
			else if (version == 2)
			{
				// Define some format which allows +4GB substream
				// When this is used we will need some more padding space; but it probably will never be written anyway
				// At least we can read them with this version

				_offset = reader.ReadInt64();
				_length = reader.ReadInt64(); // As long
				_itemType = reader.ReadInt32();
			}
			else
				throw new InvalidOperationException();
		}

		internal void WriteTo(QQnBinaryWriter writer)
		{
			if (_length < uint.MaxValue)
			{
				writer.Write((byte)1);
				writer.Write(_offset);
				writer.Write((uint)_length); // As UInt32
				writer.Write(_itemType);
			}
			else
			{
				writer.Write((byte)2);
				writer.Write(_offset);
				writer.Write(_length); // As long
				writer.Write(_itemType);

				//throw new NotSupportedException("Big chance on buffer overflows on substreams greater than 4GB; Please review before enabling");
				// If only 1 in 4 streams is version 2 we are ok 
			}
		}

		public long Offset
		{
			get { return _offset; }
			set 
			{
				Debug.Assert(value >= 0);
				_offset = value;
			}
		}

		public long Length
		{
			get { return _length; }
			set 
			{ 
				Debug.Assert(value >= 0);
				_length = value; 
			}
		}
		
		public int ItemType
		{
			get { return _itemType; }
			set { _itemType = value; }
		}

		public const short ZippedFlag = 0x01;
		public const short AssuredFlag = 0x02;
	}
}
