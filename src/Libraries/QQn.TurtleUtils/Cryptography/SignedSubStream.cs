using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace QQn.TurtleUtils.Cryptography
{
	public class SignedSubStream : StreamProxy
	{
		readonly AssemblyStrongNameKey _key;
		long _headerPosition;
		long _hashPosition;
		byte[] _streamHash;
		byte[] _hashSignature;
		long _hashLength;
		bool _updating;

		public SignedSubStream(Stream parentStream, VerificationMode mode)
			: base(parentStream, true)
		{
			SignedStream signedParent = GetService<SignedStream>();
			
			if (signedParent != null)
				_key = signedParent.AssemblyStrongNameKey;
			
			_headerPosition = parentStream.Position;

			_streamHash = new byte[(_key != null) ? _key.HashLength : 256 / 8]; // 256 = Bits of SHA256
			_hashSignature = new byte[(_key != null) ? _key.SignatureLength : 0];

			if (parentStream.CanWrite && parentStream.CanSeek && parentStream.Position == parentStream.Length)
			{
				_updating = true;
				WriteHeader();
			}
			else
			{
				QQnBinaryReader br = new QQnBinaryReader(ParentStream);
				_streamHash = br.ReadBytes(_streamHash.Length);
				_hashSignature = br.ReadBytes(_hashSignature.Length);				
				_hashLength = br.ReadInt64();
				_hashPosition = ParentStream.Position;

				if(mode != VerificationMode.None)
				{
					if(_key != null && !_key.VerifyHash(_streamHash, _hashSignature))
						throw new CryptographicException("Stream hash verification failed");
				}

				if (mode == VerificationMode.Full)
				{
					if (!VerifyHash())
						throw new CryptographicException("Invalid hash value");
				}
			}
		}

		public bool VerifyHash()
		{
			byte[] streamHash = CalculateHash(false);

			if (streamHash.Length == _streamHash.Length && _hashLength == Length)
			{
				for (int i = 0; i < streamHash.Length; i++)
				{
					if (streamHash[i] != _streamHash[i])
						return false;
				}
				return true;
			}
			return false;			
		}

		void WriteHeader()
		{
			if(ParentStream.Position != _headerPosition)
				ParentStream.Position = _headerPosition;

			QQnBinaryWriter bw = new QQnBinaryWriter(ParentStream);
			bw.Write(_streamHash);
			bw.Write(_hashSignature);			
			bw.Write(_hashLength);
			_hashPosition = ParentStream.Position;
		}

		protected override long PositionToSubStream(long parentPosition)
		{
			return parentPosition - _hashPosition;
		}

		protected override long PositionToParent(long subStreamPosition)
		{
			return subStreamPosition + _hashPosition;
		}

		/// <summary>
		/// Gets a string representation of the stream-hash.
		/// </summary>
		/// <remarks>When creating the hash is not valid until after closing the stream</remarks>
		public string HashString
		{
			get { return QQnCryptoHelpers.HashString(_streamHash); }
		}

		public override void Write(byte[] buffer, int offset, int count)
		{			
			base.Write(buffer, offset, count);
			_updating = true;
		}

		public override void SetLength(long value)
		{
			base.SetLength(value);
			_updating = true;
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

			if (_updating)
				UpdateHash();

			base.Close();
		}

		protected virtual void UpdateHash()
		{
			byte[] fileHash = CalculateHash(true);
			_streamHash = fileHash;

			if (_key != null)
			{
				if (_key.PublicOnly)
					throw new InvalidOperationException();

				byte[] signature = _key.SignHash(fileHash);

				if (signature.Length != _hashSignature.Length)
					throw new InvalidOperationException();
				else if (fileHash.Length != _streamHash.Length)
					throw new InvalidOperationException();
				
				_hashSignature = signature;
			}			

			WriteHeader();
		}

		byte[] CalculateHash(bool create)
		{
			if (create)
				_hashLength = Length;

			long oldPos = Position;
			Position = 0;
			using (HashAlgorithm hasher = (_key != null) ? _key.CreateHasher() : SHA256.Create())
			{
				byte[] buffer;
				using (MemoryStream ms = new MemoryStream(16)) // Use memorystream and writer to resolve endian-issues
				using(QQnBinaryWriter bw = new QQnBinaryWriter(ms))
				{					
					bw.Write(_hashLength);
					buffer = ms.ToArray();
				}

				hasher.TransformBlock(buffer, 0, buffer.Length, null, 0);
				
				buffer = new byte[8192];

				int nRead;

				while (0 != (nRead = Read(buffer, 0, buffer.Length)))
				{
					hasher.TransformBlock(buffer, 0, nRead, null, 0);
				}

				hasher.TransformFinalBlock(buffer, 0, 0);

				if (CanSeek)
					Position = oldPos;

				return hasher.Hash;
			}			
		}
	}
}
