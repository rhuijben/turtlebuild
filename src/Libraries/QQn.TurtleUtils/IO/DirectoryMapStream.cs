using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using QQn.TurtleUtils.Cryptography;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class DirectoryMapStream : StreamProxy
	{
		readonly DirectoryMapItem _file;
		readonly HashType _hashType;
		readonly bool _updateOnClose;

		bool _hashOnClose;
		string _newHash;
		long _size;		
		bool _hashWhileWriting;
		

		static bool UseAsync(FileMode mode, string hash)
		{
			switch (mode)
			{
				case FileMode.Append:
					return true;
				case FileMode.Create:
				case FileMode.CreateNew:
				case FileMode.Truncate:
					return !string.IsNullOrEmpty(hash);
				case FileMode.Open:
					return true;
				default:
					throw new ArgumentException("Invalid filemode", "mode");
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryMapStream"/> class.
		/// </summary>
		/// <param name="parentStream">The parent stream.</param>
		/// <param name="file">The file.</param>
		/// <param name="mode">The mode.</param>
		/// <param name="hash">The hash.</param>
		/// <param name="size">The size.</param>
		/// <param name="hashType">Type of the hash.</param>
		internal DirectoryMapStream(Stream parentStream, DirectoryMapFile file, FileMode mode, string hash, long size, HashType hashType)
			: base(parentStream, true, UseAsync(mode, hash))
		{
			if (mode == FileMode.Create || mode == FileMode.CreateNew)
			{
				_newHash = hash;
				_updateOnClose = true;

				if (string.IsNullOrEmpty(hash))
				{
					_hashOnClose = true;
					_hashWhileWriting = true;
				}				
			}

			_hashType = hashType;
			_file = file;
			_size = size;
		}

		HashAlgorithm _hashAlgorithm;
		/// <summary>
		/// Gets the hasher.
		/// </summary>
		/// <value>The hasher.</value>
		HashAlgorithm Hasher
		{
			get
			{
				if (_hashAlgorithm == null)
					_hashAlgorithm = QQnCryptoHelpers.CreateHashAlgorithm(_hashType);

				return _hashAlgorithm;
			}
		}

		/// <summary>
		/// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		/// <exception cref="T:System.ArgumentNullException">buffer is null. </exception>
		/// <exception cref="T:System.ArgumentException">The sum of offset and count is greater than the buffer length. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (_hashWhileWriting)
				Hasher.TransformBlock(buffer, 0, count, null, 0);

			base.Write(buffer, offset, count);
		}

		/// <summary>
		/// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
		/// </summary>
		/// <param name="value">The byte to write to the stream.</param>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support writing, or the stream is already closed. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override void WriteByte(byte value)
		{
			if (_hashWhileWriting)
				Hasher.TransformBlock(new byte[] { value }, 0, 1, null, 0);

			base.WriteByte(value);
		}

		static readonly byte[] _emptyBytes = new byte[0];
		bool _closed;
		/// <summary>
		/// Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
		/// </summary>
		public override void Close()
		{
			if (_closed)
				return;

			_closed = true;
			if (!string.IsNullOrEmpty(_newHash) && _size != Length)
			{
				Debug.WriteLine(">>> Got invalid predefined hash");
				_hashOnClose = true;				
			}


			if (_hashOnClose)
			{
				if (!_hashWhileWriting)
				{
					if (_hashAlgorithm != null)
					{
						Hasher.TransformFinalBlock(_emptyBytes, 0, 0);
						GC.KeepAlive(Hasher.Hash);
					}

					base.Position = 0;
					byte[] buffer = new byte[16384];
					int nRead;

					while (0 <= (nRead = base.Read(buffer, 0, buffer.Length)))
						Hasher.TransformBlock(buffer, 0, nRead, null, 0);
				}

				Hasher.TransformFinalBlock(_emptyBytes, 0, 0);

				_newHash = QQnCryptoHelpers.HashString(Hasher.Hash, _hashType);
				_size = Length;

			}
			base.Close();

			if (_updateOnClose)
				_file.UpdateData(_newHash, _size, File.GetLastWriteTimeUtc(_file.FullName));

			if (_hashAlgorithm != null)
			{
				_hashAlgorithm.Clear(); // Disposes the hasher
				_hashAlgorithm = null;
			}
		}

		/// <summary>
		/// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>
		/// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">The sum of offset and count is larger than the buffer length. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
		/// <exception cref="T:System.ArgumentNullException">buffer is null. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
			return base.Read(buffer, offset, count);
		}

		/// <summary>
		/// Sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the origin parameter.</param>
		/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"></see> indicating the reference point used to obtain the new position.</param>
		/// <returns>
		/// The new position within the current stream.
		/// </returns>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override long Seek(long offset, SeekOrigin origin)
		{
			_hashWhileWriting = false;
			return base.Seek(offset, origin);
		}

		/// <summary>
		/// Gets or sets the position within the current stream.
		/// </summary>
		/// <value></value>
		/// <returns>The current position within the stream.</returns>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override long Position
		{
			get { return base.Position; }
			set
			{
				_hashWhileWriting = false;
				base.Position = value;
			}
		}

		/// <summary>
		/// Sets the length of the current stream
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		/// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
		public override void SetLength(long value)
		{
			_hashWhileWriting = false;
			base.SetLength(value);
		}
	}
}
