using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace QQn.TurtleUtils.Cryptography
{
	class MyBinaryReader : BinaryReader
	{
		public MyBinaryReader(Stream input)
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

	class MyBinaryWriter : BinaryWriter
	{
		public MyBinaryWriter(Stream output)
			: base(output, Encoding.UTF8)
		{
		}

		public void WriteSmartInt(int value)
		{
			Write7BitEncodedInt(value);
		}

		public void WriteByteArray(byte[] value)
		{
			WriteSmartInt(value.Length);
			base.Write(value);
		}
	}

	class SignedStreamHeader
	{
		const uint FileSignature = 0xBADCAB;
		readonly string _fileType;
		byte[] _fileHash;
		byte[] _hashSignature;
		readonly AssemblyStrongNameKey _key;
		readonly Guid _guid;
		long _headerPosition;
		long _hashPosition;
		long _bodyLength;

		public SignedStreamHeader(Stream source)
			: this(source, VerificationMode.Full)
		{
		}

		public SignedStreamHeader(Stream source, VerificationMode mode)
		{
			MyBinaryReader br = new MyBinaryReader(source);
			uint vFileSignature = br.ReadUInt32();

			if (vFileSignature != FileSignature)
				throw new FormatException();

			_fileType = br.ReadString();
			_fileHash = br.ReadByteArray();
			_hashSignature = br.ReadByteArray();
			byte[] publicKey = br.ReadByteArray();

			if (publicKey.Length > 0)
			{
				_key = AssemblyStrongNameKey.LoadFrom(publicKey);

				if (mode != VerificationMode.None)
				{
					bool unZero = false;
					foreach (byte b in _fileHash)
					{
						if (b != 0)
						{
							unZero = true;
							break;
						}
					}

					if (unZero)
						_key.VerifyHash(_fileHash, _hashSignature);
				}
			}

			_hashPosition = source.Position;
			_guid = new Guid(br.ReadBytes(16));
			_bodyLength = br.ReadInt64();
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
			MyBinaryWriter bw = new MyBinaryWriter(stream);

			_headerPosition = SafePosition(stream);
			bw.Write(FileSignature);
			bw.Write(FileType);
			bw.WriteByteArray(_fileHash);
			bw.WriteByteArray(_hashSignature);
			bw.WriteByteArray(_key != null ? _key.GetPublicKeyData() : new byte[0]);
			_hashPosition = SafePosition(stream);
			bw.Write(_guid.ToByteArray());
			bw.Write(_bodyLength);
		}

		static int _sha256HashSize;
		static int Sha256HashSize
		{
			get
			{
				if (_sha256HashSize == 0)
				{
					using (SHA256 sha256 = SHA256.Create())
					{
						_sha256HashSize = sha256.HashSize;
					}
				}
				return _sha256HashSize;
			}
		}

		public SignedStreamHeader(string fileType, AssemblyStrongNameKey snk)
			: this(new SignedStreamCreateArgs(snk, fileType))
		{
		}

		internal SignedStreamHeader(SignedStreamCreateArgs args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			args.VerifyArgs("args");

			_fileType = args.FileType;

			if (args.StrongName == null)
			{
				_fileHash = new byte[Sha256HashSize];
				_hashSignature = new byte[0];
			}
			else
			{
				_key = args.StrongName;
				_fileHash = new byte[_key.HashLength];
				_hashSignature = new byte[_key.SignatureLength];
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

		public byte[] PublicKeyToken
		{
			get { return (_key != null) ? _key.PublicKeyToken : null; }
		}

		public AssemblyStrongNameKey AssemblyStrongNameKey
		{
			get { return _key; }
		}

		long StreamLength(Stream stream)
		{
			return stream.Length - _hashPosition - 16 - 8;
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
			if (_key != null)
			{
				if (_key.PublicOnly)
					throw new InvalidOperationException();

				byte[] signature = _key.SignHash(fileHash);

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

			using (HashAlgorithm hasher = (_key != null) ? _key.CreateHasher() : SHA256.Create())
			{
				long newPos = _hashPosition + 16 + 8;

				if (stream.Position != newPos)
					stream.Position = newPos;

				byte[] buffer;

				using (MemoryStream ms = new MemoryStream()) // Use memorystream and writer to resolve endian-issues
				using (MyBinaryWriter bw = new MyBinaryWriter(ms))
				{
					bw.Write(_guid.ToByteArray());
					bw.Write(_bodyLength);

					buffer = ms.ToArray();
				}

				hasher.TransformBlock(buffer, 0, buffer.Length, null, 0);



				//byte[] buffer = new byte[8192];

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
