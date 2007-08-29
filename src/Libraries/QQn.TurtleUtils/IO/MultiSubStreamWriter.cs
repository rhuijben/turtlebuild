using System;
using System.Collections.Generic;
using System.IO;

namespace QQn.TurtleUtils.IO
{
	sealed class MultipleSubStreamWriter : ProxyStream
	{
		readonly MultipleStreamWriter _writer;
		readonly MultiStreamItemHeader _header;		
		bool _closed;

		public MultipleSubStreamWriter(MultipleStreamWriter writer, Stream baseStream, MultiStreamItemHeader header)
			: base(baseStream, false)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			_writer = writer;
			_header = header;
		}

		public override void Close()
		{
			if (_closed)
				return;

			_closed = true;
			_header.Length = Length;			
			_writer.CloseStream(this);
		}

		protected override long PositionToSubStream(long parentPosition)
		{
			return parentPosition - _header.Offset;
		}

		protected override long PositionToParent(long subStreamPosition)
		{
			return subStreamPosition + _header.Offset;
		}

		public MultiStreamItemHeader Header
		{
			get { return _header; }
		}
	}
}
