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
			: base(input)
		{
		}

		public MyBinaryReader(Stream input, Encoding encoding)
			: base(input, encoding)
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
			: base(output)
		{
		}

		public MyBinaryWriter(Stream output, Encoding encoding)
			: base(output, encoding)
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

	class SignedFileHeader
	{
		const uint FileSignature = 0xBADCAB;
		readonly string _fileType;
		byte[] _fileHash;
		byte[] _hashSignature;
		readonly byte[] _publicKey;
		readonly AssemblyStrongNameKey _key;		
		readonly Guid _guid;
		long _headerPosition;
		long _hashPosition;

		public SignedFileHeader(Stream source)
			: this(source, VerificationMode.Full)
		{
		}

		public SignedFileHeader(Stream source, VerificationMode mode)
		{
			MyBinaryReader br = new MyBinaryReader(source, Encoding.UTF8);
			uint vFileSignature = br.ReadUInt32();

			if (vFileSignature != FileSignature)
				throw new FormatException();

			_fileType = br.ReadString();
			_fileHash = br.ReadByteArray();
			_hashSignature = br.ReadByteArray();
			_publicKey = br.ReadByteArray();

			if (_publicKey.Length > 0)
			{
				_key = AssemblyStrongNameKey.LoadFrom(_publicKey);

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
			MyBinaryWriter bw = new MyBinaryWriter(stream, Encoding.UTF8);

			_headerPosition = SafePosition(stream);
			bw.Write(FileSignature);
			bw.Write(FileType);
			bw.WriteByteArray(_fileHash);
			bw.WriteByteArray(_hashSignature);
			bw.WriteByteArray(_publicKey);
			_hashPosition = SafePosition(stream);
			bw.Write(_guid.ToByteArray());
		}

		static int _sha256HashSize;
		static int Sha256HashSize
		{
			get 
			{
				if(_sha256HashSize == 0)
				{
					using(SHA256 sha256 = SHA256.Create())
					{
						_sha256HashSize = sha256.HashSize;
					}
				}
				return _sha256HashSize;
			}
		}

		public SignedFileHeader(string fileType, AssemblyStrongNameKey snk)
		{
			if (string.IsNullOrEmpty(fileType))
				throw new ArgumentNullException("fileType");

			_fileType = fileType;

			if(snk == null)
			{
				_fileHash = new byte[Sha256HashSize];
				_hashSignature = new byte[0];
				_publicKey = new byte[0];
			}
			else
			{
				_key = snk;
				_fileHash = new byte[snk.HashLength];
				_hashSignature = new byte[snk.SignatureLength];
				_publicKey = snk.GetPublicKeyData();
			}
			_guid = Guid.NewGuid();
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

		internal bool VerifyHash(Stream innerStream)
		{			
			byte[] fileHash = CalculateHash(innerStream);

			if (fileHash.Length == _fileHash.Length)
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
			byte[] fileHash = CalculateHash(stream);
			if (_key != null)
			{
				if (_key.PublicOnly)
					throw new InvalidOperationException();

				byte[] signature =_key.SignHash(fileHash);

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

		byte[] CalculateHash(Stream innerStream)
		{
			innerStream.Position = 0;
			long pos = innerStream.Position;

			

			using(HashAlgorithm hasher = (_key != null) ? _key.CreateHasher() : SHA256.Create())
			{
				if(pos != _hashPosition)
				{
					if(pos == _hashPosition + 16)
					{
						byte[] guid = _guid.ToByteArray();

						hasher.TransformBlock(guid, 0, guid.Length, null, 0);
					}
					else
						innerStream.Position = _hashPosition;
				}

				byte[] buffer = new byte[8192];
				int nRead;

				while(0 != (nRead = innerStream.Read(buffer, 0, buffer.Length)))
				{
					hasher.TransformBlock(buffer, 0, nRead, null, 0);
				}

				hasher.TransformFinalBlock(buffer, 0, 0);

				if(innerStream.CanSeek)
					innerStream.Position = pos;

				return hasher.Hash;
			}
		}
	}
}
