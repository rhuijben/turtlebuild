using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using QQn.TurtleUtils.Cryptography;

namespace QQn.TurtleUtils.IO
{
	class AssuredStreamHeader
	{
		const uint FileSignature = 0xABDCBA;
		readonly string _fileType;
		byte[] _fileHash;
		byte[] _hashSignature;
		readonly StrongNameKey _snk;
		readonly Guid _guid;
		long _headerPosition;
		long _hashPosition;
		long _bodyLength;

		public AssuredStreamHeader(Stream source)
			: this(source, VerificationMode.Full)
		{
		}

		public AssuredStreamHeader(Stream source, VerificationMode mode)
		{
			QQnBinaryReader br = new QQnBinaryReader(source);
			uint vFileSignature = br.ReadUInt32();

			if (vFileSignature != FileSignature)
				throw new FormatException();

			_fileType = br.ReadString();
			_fileHash = br.ReadByteArray();
			_hashSignature = br.ReadByteArray();
			byte[] publicKey = br.ReadByteArray();

			if (publicKey.Length > 0)
			{
				_snk = StrongNameKey.LoadFrom(publicKey);

				if (mode != VerificationMode.None)
				{
					if (!_snk.VerifyHash(_fileHash, _hashSignature))
						throw new CryptographicException("Stream hash verification failed");
				}
			}
			_guid = new Guid(br.ReadBytes(16));
			_bodyLength = br.ReadInt64();

			_hashPosition = source.Position;
		}

		static long SafePosition(Stream stream)
		{
			if (stream.CanSeek)
				return stream.Position;
			else
				return -1;
		}

		public void WriteHeader(Stream stream)
		{
			QQnBinaryWriter bw = new QQnBinaryWriter(stream);

			_headerPosition = SafePosition(stream);
			bw.Write(FileSignature);
			bw.Write(FileType);
			bw.WriteByteArray(_fileHash);
			bw.WriteByteArray(_hashSignature);
			bw.WriteByteArray(_snk != null ? _snk.GetPublicKeyData() : new byte[0]);
			bw.Write(_guid.ToByteArray());
			bw.Write(_bodyLength);
			_hashPosition = SafePosition(stream);
		}

		const int Sha256HashSize = 256 / 8;

		public AssuredStreamHeader(string fileType, StrongNameKey snk)
			: this(new AssuredStreamCreateArgs(snk, fileType))
		{
		}

		internal AssuredStreamHeader(AssuredStreamCreateArgs args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			args.VerifyArgs("args");

			_fileType = args.FileType;

			if (args.StrongNameKey == null)
			{
				_fileHash = new byte[Sha256HashSize];
				_hashSignature = new byte[0];
			}
			else
			{
				_snk = args.StrongNameKey;
				_fileHash = new byte[_snk.HashLength];
				_hashSignature = new byte[_snk.SignatureLength];
			}
			_guid = args.NullGuid ? Guid.Empty : Guid.NewGuid();
		}

		public string FileType
		{
			get { return _fileType ?? ""; }
		}

		public Guid Guid
		{
			get { return _guid; }
		}

		public byte[] HashValue
		{
			get { return (byte[])_fileHash.Clone(); }
		}

		public string PublicKeyToken
		{
			get { return (_snk != null) ? _snk.PublicKeyToken : null; }
		}

		public StrongNameKey AssemblyStrongNameKey
		{
			get { return _snk; }
		}

		long StreamLength(Stream stream)
		{
			return stream.Length - _hashPosition;
		}

		internal bool VerifyHash(Stream stream)
		{
			byte[] fileHash = CalculateHash(stream, false);

			if (fileHash.Length == _fileHash.Length && _bodyLength == StreamLength(stream))
			{
				for (int i = 0; i < fileHash.Length; i++)
				{
					if (fileHash[i] != _fileHash[i])
						return false;
				}
				return true;
			}
			return false;
		}

		internal void UpdateHash(Stream stream)
		{
			byte[] fileHash = CalculateHash(stream, true);
			if (_snk != null)
			{
				if (_snk.PublicOnly)
					throw new InvalidOperationException();

				byte[] signature = _snk.SignHash(fileHash);

				if (signature.Length != _hashSignature.Length)
					throw new InvalidOperationException();
				else if (fileHash.Length != _fileHash.Length)
					throw new InvalidOperationException();

				_fileHash = fileHash;
				_hashSignature = signature;
			}

			stream.Position = _headerPosition;
			WriteHeader(stream);
		}

		byte[] CalculateHash(Stream stream, bool create)
		{
			long pos = stream.Position;

			if (create)
				_bodyLength = StreamLength(stream);

			using (HashAlgorithm hasher = (_snk != null) ? _snk.CreateHasher() : SHA256.Create())
			{
				long newPos = _hashPosition;

				if (stream.Position != newPos)
					stream.Position = newPos;

				byte[] buffer;

				using (MemoryStream ms = new MemoryStream()) // Use memorystream and writer to resolve endian-issues
				using (QQnBinaryWriter bw = new QQnBinaryWriter(ms))
				{
					bw.Write(_guid.ToByteArray());
					bw.Write(_bodyLength);

					buffer = ms.ToArray();
				}

				hasher.TransformBlock(buffer, 0, buffer.Length, null, 0);

				buffer = new byte[8192];

				int nRead;

				while (0 != (nRead = stream.Read(buffer, 0, buffer.Length)))
				{
					hasher.TransformBlock(buffer, 0, nRead, null, 0);
				}

				hasher.TransformFinalBlock(buffer, 0, 0);

				if (stream.CanSeek)
					stream.Position = pos;

				return hasher.Hash;
			}
		}
	}
}
