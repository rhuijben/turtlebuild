using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Compression;

namespace QQn.TurtlePackage.StreamFiles
{
	public class StreamFile : IDisposable
	{
		string _fileName;
		readonly string _tmpName;
		readonly Stream _stream;
		Guid _guid;
		Stream _currentStream;

		long _posOffset;
		long[] _positions;
		long[] _size;

		int _nStream;

		public StreamFile(string fileName, int maxStreams)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException("fileName");
			else if (maxStreams < 1)
				throw new ArgumentOutOfRangeException("streams", maxStreams, "Streams must be at least 1");

			_fileName = Path.GetFullPath(fileName);
			_guid = new Guid();
			_stream = File.Create(_tmpName = (_fileName + ".tmp"));
			WriteBuffer(Encoding.ASCII.GetBytes("TurtleStream/1.0"));			

			WriteBuffer(_guid.ToByteArray());
			WriteBuffer(BitConverter.GetBytes(maxStreams));

			byte[] twoLongs = new byte[2 * sizeof(long)];

			_positions = new long[maxStreams];
			_size = new long[maxStreams];

			_posOffset = _stream.Position;

			for (int i = 0; i < maxStreams; i++)
			{
				WriteBuffer(twoLongs);
			}

			PadFile();
		}

		protected StreamFile(Stream stream, string filename, Guid guid)
		{
			_stream = stream;
			_fileName = filename;
			_guid = guid;
		}

		protected void SetInfo(long[] positions, long[] sizes)
		{
			_positions = positions;
			_size = sizes;
		}

		public static StreamFile Open(string filename)
		{
			FileStream fs = File.OpenRead(filename);
			try
			{
				byte[] header = new byte[16];
				fs.Read(header, 0, header.Length);

				string headerLine = Encoding.ASCII.GetString(header);

				fs.Read(header, 0, header.Length);



				switch (headerLine)
				{
					case "TurtleStream/1.0":
						return new StreamFile10(fs, filename, new Guid(header));
					default:
						throw new InvalidOperationException();
				}
			}
			catch
			{
				fs.Close();
				throw;
			}
		}

		const int _padSize = 256;
		private void PadFile()
		{
			int rest = (int)(_stream.Position % (long)_padSize);

			if (rest > 0)
			{
				byte[] buffer = new byte[_padSize - rest];

				WriteBuffer(buffer);
			}			
		}

		void WriteBuffer(byte[] buffer)
		{
			_stream.Write(buffer, 0, buffer.Length);
		}

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		public void Close()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_stream.CanWrite)
			{
				_stream.Seek(_posOffset, SeekOrigin.Begin);

				for (int i = 0; i < _positions.Length; i++)
				{
					WriteBuffer(BitConverter.GetBytes(_positions[i]));
					WriteBuffer(BitConverter.GetBytes(_size[i]));
				}

				_stream.Close();

				File.Move(_tmpName, _fileName);
			}
		}

		#endregion

		public Stream CreateNextStream()
		{
			if (_currentStream != null)
				throw new InvalidOperationException();
			else if(!_stream.CanWrite)
				throw new InvalidOperationException();

			PadFile();
			_positions[_nStream] = _stream.Position;
			return _currentStream = new MyGZipStream(this, CompressionMode.Compress);
		}

		public Stream GetNextStream()
		{
			if(_currentStream != null)
				throw new InvalidOperationException();
			else if (_stream.CanWrite)
				throw new InvalidOperationException();

			_stream.Seek(_positions[_nStream], SeekOrigin.Begin);

			return _currentStream = new MyGZipStream(this, CompressionMode.Decompress);
		}

		class MyGZipStream : GZipStream
		{
			readonly StreamFile _parent;
			int _bytesWritten;

			public MyGZipStream(StreamFile parent, CompressionMode mode)
				: base(parent._stream, mode, true)
			{
				_parent = parent;
			}

			public override void Write(byte[] array, int offset, int count)
			{
				base.Write(array, offset, count);
				_bytesWritten += count;
			}

			public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
			{
				_bytesWritten += count;
				return base.BeginWrite(array, offset, count, asyncCallback, asyncState);
			}

			public override void Close()
			{
				base.Close();

				if (_parent._currentStream == this)
					_parent.CloseStream(this, _bytesWritten);

			}
		}

		void CloseStream(MyGZipStream myGZipStream, int bytesWritten)
		{
			if (_currentStream == myGZipStream)
			{
				if (_stream.CanWrite)
				{
					_size[_nStream] = bytesWritten;
				}
				_currentStream = null;

				_nStream++;
			}
		}
	}
}
