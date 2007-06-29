using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace QQn.TurtlePackage.StreamFiles
{
	class StreamFile10 : StreamFile
	{
		Stream _stream;
		public StreamFile10(Stream stream, string filename, Guid guid)
			: base(stream, filename, guid)
		{
			_stream = stream;
			byte[] intBytes = new byte[sizeof(int)];
			byte[] longBytes = new byte[2*sizeof(long)];

			ReadBuffer(intBytes);
			int count = BitConverter.ToInt32(intBytes, 0);

			long[] positions = new long[count];
			long[] sizes = new long[count];
			for (int i = 0; i < count; i++)
			{
				ReadBuffer(longBytes);
				positions[i] = BitConverter.ToInt64(longBytes, 0);
				sizes[i] = BitConverter.ToInt64(longBytes, sizeof(long));
			}
			SetInfo(positions, sizes);
		}

		void ReadBuffer(byte[] buffer)
		{
			_stream.Read(buffer, 0, buffer.Length);
		}
	}
}
