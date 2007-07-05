using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;

namespace QQn.TurtleUtils.Cryptography
{
	public sealed class AssemblyStrongNameKey
	{
		private AssemblyStrongNameKey()
		{
		}

		/// <summary>
		/// Loads an Snk file
		/// </summary>
		/// <param name="path">Path to snk file</param>
		/// <returns></returns>
		public static AssemblyStrongNameKey LoadFrom(string path)
		{
			using (FileStream s = File.OpenRead(path))
			{
				return LoadFrom(s);
			}
		}

		public static AssemblyStrongNameKey LoadFrom(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			if (stream.CanSeek)
				stream.Position = 0;

			using (BinaryReader br = new BinaryReader(stream))
			{
				return new AssemblyStrongNameKey(br);
			}
		}

		public static AssemblyStrongNameKey LoadFrom(byte[] bytes)
		{
			if(bytes == null)
				throw new ArgumentNullException("bytes");

			using(MemoryStream ms = new MemoryStream(bytes))
			{
				return LoadFrom(ms);
			}
		}

		RSACryptoServiceProvider _dcsp;
		AssemblyStrongNameKey(BinaryReader binaryReader)
		{
			CspParameters csp = new CspParameters(1);

			_dcsp = new RSACryptoServiceProvider(csp);
			_dcsp.ImportCspBlob(binaryReader.ReadBytes((int)binaryReader.BaseStream.Length));
		}

		public byte[] SignHash(byte[] hash)
		{
			if (hash == null)
				throw new ArgumentNullException("hash");

			return _dcsp.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
		}

		public bool VerifyHash(byte[] hash, byte[] signature)
		{
			if(hash == null)
				throw new ArgumentNullException("hash");
			else if(signature == null)
				throw new ArgumentNullException("signature");

			return _dcsp.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signature);
		}

		public byte[] GetPublicKeyData()
		{
			return _dcsp.ExportCspBlob(false);
		}

		byte[] _publicKeyToken;
		public byte[] PublicKeyToken
		{
			get
			{				
				if (_publicKeyToken == null)
				{
					StrongNameKeyPair snkp = new StrongNameKeyPair(GetPublicKeyData());
					byte[] publicKeyHash;
					using (SHA1 sha = SHA1.Create())
					{
						publicKeyHash = sha.ComputeHash(snkp.PublicKey);
					}
					byte[] last8 = new byte[8];
					Array.Copy(publicKeyHash, publicKeyHash.Length - 8, last8, 0, 8);
					Array.Reverse(last8);
					_publicKeyToken = last8;
				}
				return _publicKeyToken;
			}
		}

		public int SignatureLength
		{
			get
			{
				return _dcsp.KeySize  / 8;
			}
		}

		int _hashLength;
		public int HashLength
		{
			get
			{
				if (_hashLength == 0)
				{
					using (HashAlgorithm hasher = CreateHasher())
					{
						_hashLength = hasher.HashSize / 8;
					}
				}
				return _hashLength;
			}
		}

		public bool PublicOnly
		{
			get { return _dcsp.PublicOnly; }
		}

		public HashAlgorithm CreateHasher()
		{
			string alg = _dcsp.SignatureAlgorithm;
			alg = alg.Substring(alg.LastIndexOf('#') + 1);
			string[] parts = alg.Split('-');

			return HashAlgorithm.Create(parts[1]);
		}
	}
}
