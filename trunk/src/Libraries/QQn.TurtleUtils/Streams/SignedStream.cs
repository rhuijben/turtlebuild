using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using QQn.TurtleUtils.Cryptography;

namespace QQn.TurtleUtils.Streams
{
	/// <summary>
	/// 
	/// </summary>
	public enum VerificationMode
	{
		/// <summary>
		/// 
		/// </summary>
		Full,
		/// <summary>
		/// 
		/// </summary>
		Sign,
		/// <summary>
		/// 
		/// </summary>
		None
	}

	/// <summary>
	/// Stream wrapper with embedded hashing and optionally signing the hash
	/// </summary>
	public class AssuredStream : StreamProxy
	{
		readonly bool _seekable;
		readonly bool _creating;

		readonly AssuredStreamHeader _header;
		readonly long _basePosition;

		private AssuredStream(Stream stream, bool create)
			: base(stream, true)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");
			else if (!stream.CanRead)
				throw new ArgumentException("Must be able to read from the stream");

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
		public AssuredStream(Stream stream, VerificationMode mode)
			: this(stream, false)
		{
			_header = new AssuredStreamHeader(stream, mode);

			if (mode == VerificationMode.Full)
			{
				if (!_seekable)
					throw new InvalidOperationException("Can't fully verify unseekable streams");

				_basePosition = stream.Position;
				if (!_header.VerifyHash(stream))
					throw new CryptographicException("Invalid hash value");

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
		public AssuredStream(Stream stream, StrongNameKey strongName, string fileType)
			: this(stream, new AssuredStreamCreateArgs(strongName, fileType))
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="args"></param>
		public AssuredStream(Stream stream, AssuredStreamCreateArgs args)
			: this(stream, true)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			args.VerifyArgs("args");			

			long pos = stream.Position;

			_header = new AssuredStreamHeader(args);
			_header.WriteHeader(stream);

			_basePosition = stream.Position;
		}

		/// <summary>
		/// 
		/// </summary>
		public string FileType
		{
			get { return _header.FileType; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Guid Guid
		{
			get { return _header.Guid; }
		}

		/// <summary>
		/// Gets a string representation of the stream-hash.
		/// </summary>
		/// <remarks>When creating the hash is not valid until after closing the stream</remarks>
		public string HashString
		{
			get { return QQnCryptoHelpers.HashString(_header.HashValue); }
		}

		/// <summary>
		/// Gets a string representation of a hash over the public key; or null if no public key is available
		/// </summary>
		public string PublicKeyToken
		{
			get { return (_header.AssemblyStrongNameKey != null) ? QQnCryptoHelpers.HashString(_header.PublicKeyToken) : null; }
		}

		/// <summary>
		/// Gets the <see cref="AssemblyStrongNameKey"/> used for the hash-signature, or null if no such key is provided
		/// </summary>
		public StrongNameKey AssemblyStrongNameKey
		{
			get { return _header.AssemblyStrongNameKey; }
		}
		
		/// <summary>
		/// Gets a boolean indicating whether the stream is writable; See <see cref="Stream.CanWrite"/>
		/// </summary>
		public override bool CanWrite
		{
			get { return _creating; }
		}

		/// <summary>
		/// Sets the length of the stream; See <see cref="Stream.SetLength"/>
		/// </summary>
		/// <param name="value"></param>
		public override void SetLength(long value)
		{
			if (!_creating)
				throw new InvalidOperationException();

			ParentStream.SetLength(PositionToParent(value));
		}

		/// <summary>
		/// Writes the buffer to the inner stream; see <see cref="Stream.Write(byte[], int, int)"/>
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!_creating)
				throw new InvalidOperationException();

			base.Write(buffer, offset, count);
		}

		/// <summary>
		/// Updates the hash information header
		/// </summary>
		/// <remarks>Called from Close() just before closing the stream</remarks>
		protected virtual void UpdateHash()
		{
			if (!_creating)
				throw new InvalidOperationException();

			_header.UpdateHash(ParentStream);
		}

		bool _closed;
		/// <summary>
		/// Closes the stream
		/// </summary>
		public override void Close()
		{
			if (_closed)
				return;

			_closed = true;

			if (_creating)
				UpdateHash();

			base.Close();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentPosition"></param>
		/// <returns></returns>
		protected override long PositionToSubStream(long parentPosition)
		{
			return parentPosition - _basePosition;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="subStreamPosition"></param>
		/// <returns></returns>
		protected override long PositionToParent(long subStreamPosition)
		{
			return subStreamPosition + _basePosition;
		}
	}
}
