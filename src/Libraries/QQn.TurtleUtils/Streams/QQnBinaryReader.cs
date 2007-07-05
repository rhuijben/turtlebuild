using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace QQn.TurtleUtils.Streams
{
	class QQnBinaryReader : BinaryReader
	{
		public QQnBinaryReader(Stream input)
			: base(input, Encoding.UTF8)
		{
		}

		public int ReadSmartInt()
		{
			return base.Read7BitEncodedInt();
		}

		public byte[] ReadByteArray()
		{
			int length = ReadSmartInt();
			return ReadBytes(length);
		}
	}
}
