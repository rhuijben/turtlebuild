using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace QQn.TurtleUtils.IO
{
	sealed class MultiSubStreamWriter : StreamProxy
	{
		readonly MultiStreamWriter _writer;
		readonly MultiStreamItemHeader _header;		
		bool _closed;

		public MultiSubStreamWriter(MultiStreamWriter writer, Stream baseStream, MultiStreamItemHeader header)
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

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (_closed)
				throw new NotSupportedException();
			return base.Seek(offset, origin);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_closed)
				throw new NotSupportedException();

			return base.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (_closed)
				throw new NotSupportedException();
			base.Write(buffer, offset, count);
		}

		public override void SetLength(long value)
		{
			if (_closed)
				throw new NotSupportedException();
			base.SetLength(value);
		}

		public override long Position
		{
			get
			{
				return base.Position;
			}
			set
			{
				if (_closed)
					throw new NotSupportedException();
				base.Position = value;
			}
		}

		protected override long PositionToSubStream(long parentPosition)
		{
			return parentPosition - _header.Offset;
		}

		protected override long PositionToParent(long subStreamPosition)
		{
			return _header.Offset - _header.Offset;
		}

		public MultiStreamItemHeader Header
		{
			get { return _header; }
		}
	}
}
