using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace QQn.TurtleUtils.Cryptography
{
	public enum VerificationMode
	{
		Full,
		Sign,
		None
	}

	/// <summary>
	/// Stream wrapper with embedded hashing and optionally signing the hash
	/// </summary>
	public class SignedStream : Stream
	{
		readonly Stream _innerStream;		
		readonly bool _seekable;
		readonly bool _creating;

		readonly SignedFileHeader _header;
		readonly long _basePosition;

		private SignedStream(Stream stream, bool create)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");
			else if (!stream.CanRead)
				throw new ArgumentException("Must be able to read from the stream");

			_innerStream = stream;
			_seekable = stream.CanSeek;
			_creating = create;

			if (create)
			{
				if (!stream.CanWrite || !stream.CanSeek)
					throw new ArgumentException("Can't create a SignedStream on an unwritable/unseekable stream");
			}
		}

		/// <summary>
		/// Opens an existing stream and optionally verifies the stream
		/// </summary>
		/// <param name="stream">The stream to open</param>
		/// <param name="mode">The verification to use</param>
		public SignedStream(Stream stream, VerificationMode mode)
			: this(stream, false)
		{
			_header = new SignedFileHeader(stream, mode);

			if (mode == VerificationMode.Full)
			{
				if (!_seekable)
					throw new InvalidOperationException("Can't fully verify unseekable streams");

				_basePosition = stream.Position;
				if (!_header.VerifyHash(stream))
					throw new FormatException("Invalid hash value");

				stream.Position = _basePosition;
			}
			else
				_basePosition = stream.CanSeek ? stream.Position : 0;
		}

		/// <summary>
		/// Creates a new SignedStream with the specified parameters
		/// </summary>
		/// <param name="stream">The stream to write to (Must be writable and seekable)</param>
		/// <param name="strongName">The strong name to hash/sign with (can be null)</param>
		/// <param name="fileType">The string to use as fileType</param>
		public SignedStream(Stream stream, AssemblyStrongNameKey strongName, string fileType)
			: this(stream, true)
		{
			if (string.IsNullOrEmpty(fileType))
				throw new ArgumentNullException("fileSignature");

			long pos = stream.Position;

			_header = new SignedFileHeader(fileType, strongName);
			_header.WriteHeader(stream);

			_basePosition = stream.Position;
		}

		public string FileType
		{
			get { return _header.FileType; }
		}

		public Guid Guid
		{
			get { return _header.Guid; }
		}

		public string HashString
		{
			get { return QQnCryptoHelpers.HashString(_header.HashValue); }
		}

		public string PublicKeyToken
		{
			get { return QQnCryptoHelpers.HashString(_header.PublicKeyToken); }
		}

		public AssemblyStrongNameKey AssemblyStrongNameKey
		{
			get { return _header.AssemblyStrongNameKey; }
		}

		public override bool CanRead
		{
			get { return _innerStream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _innerStream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _creating; }
		}

		public override void Flush()
		{
			_innerStream.Flush();
		}

		public override long Length
		{
			get { return _innerStream.Length - _basePosition; }
		}

		public override long Position
		{
			get { return _innerStream.Position - _basePosition; }
			set 
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException("value", value, "value must be >= 0");

				_innerStream.Position = value + _basePosition; 
			}			
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _innerStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Current:
				case SeekOrigin.End:
					return InnerStream.Seek(offset, origin) - _basePosition; // Allows overshooting for now
				case SeekOrigin.Begin:
					return InnerStream.Seek(offset + _basePosition, SeekOrigin.Begin) - _basePosition;

				default:
					throw new ArgumentOutOfRangeException("origin", origin, "Invalid origin");
			}
		}

		public override void SetLength(long value)
		{
			if (!_creating)
				throw new InvalidOperationException();

			InnerStream.SetLength(value + _basePosition);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!_creating)
				throw new InvalidOperationException();

			InnerStream.Write(buffer, offset, count);
		}

		/// <summary>
		/// Gets the inner stream
		/// </summary>

		protected Stream InnerStream
		{
			get { return _innerStream; }
		}

		protected virtual void UpdateHash()
		{
			if (!_creating)
				throw new InvalidOperationException();

			_header.UpdateHash(InnerStream);
		}

		/// <summary>
		/// Closes the stream
		/// </summary>
		public override void Close()
		{
			if (_creating)
				UpdateHash();

			base.Close();
		}
	}
}
