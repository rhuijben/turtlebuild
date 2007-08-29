using System;
using System.Collections.Generic;
using System.IO;

namespace QQn.TurtleUtils.IO
{
	sealed class MultipleSubStreamReader : ProxyStream
	{
		readonly MultipleStreamReader _reader;
		readonly MultiStreamItemHeader _header;
		bool _closed;

		public MultipleSubStreamReader(MultipleStreamReader reader, Stream baseStream, MultiStreamItemHeader header)
			: base(baseStream, false)
		{
			if (reader == null)
				throw new ArgumentNullException("writer");

			_reader = reader;
			_header = header;
			BaseStream.Position = _header.Offset;
		}

		public override void Close()
		{
			if (_closed)
				return;

			_closed = true;
			_header.Length = Length;
			_reader.CloseStream(this);
		}

		public override bool CanWrite
		{
			get { return false; }
		}

		public override long Length
		{
			get { return _header.Length; }
		}

		protected override long PositionToSubStream(long parentPosition)
		{
			return parentPosition - _header.Offset;
		}

		protected override long PositionToParent(long subStreamPosition)
		{
			return subStreamPosition + _header.Offset;
		}
	}
}
