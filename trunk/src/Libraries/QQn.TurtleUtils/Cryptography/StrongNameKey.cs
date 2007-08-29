using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.Diagnostics;

namespace QQn.TurtleUtils.Cryptography
{
	/// <summary>
	/// An in-memory wrapper of a StrongKeyName (aka Snk file)
	/// </summary>
	[DebuggerDisplay("PublicKeyToken={PublicKeyToken}, PublicOnly={PublicOnly}")]
	public sealed class StrongNameKey
	{
		/// <summary>
		/// Loads an Snk file
		/// </summary>
		/// <param name="path">Path to snk file</param>
		/// <returns></returns>
		public static StrongNameKey LoadFrom(string path)
		{
			using (FileStream s = File.OpenRead(path))
			{
				return LoadFrom(s);
			}
		}

		/// <summary>
		/// Loads a strong key from the stream
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <returns></returns>
		public static StrongNameKey LoadFrom(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			if (stream.CanSeek)
				stream.Position = 0;

			using (BinaryReader br = new BinaryReader(stream))
			{
				CspParameters csp = new CspParameters(1);

				RSACryptoServiceProvider rcsp = new RSACryptoServiceProvider(csp);
				rcsp.ImportCspBlob(br.ReadBytes((int)br.BaseStream.Length));

				return new StrongNameKey(rcsp);
			}
		}

		/// <summary>
		/// Loads a strong name from the byte array
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static StrongNameKey LoadFrom(byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException("bytes");

			using (MemoryStream ms = new MemoryStream(bytes))
			{
				return LoadFrom(ms);
			}
		}

		/// <summary>
		/// Loads a strong name from the specified container
		/// </summary>
		/// <param name="containerName"></param>
		/// <param name="machineScope"></param>
		/// <returns></returns>
		public static StrongNameKey LoadFromContainer(string containerName, bool machineScope)
		{
			if (string.IsNullOrEmpty(containerName))
				throw new ArgumentNullException("containerName");

			CspParameters csp = new CspParameters();
			csp.KeyContainerName = containerName;
			csp.KeyNumber = 2;
			csp.Flags = CspProviderFlags.UseNonExportableKey;
			if (machineScope)
				csp.Flags |= CspProviderFlags.UseMachineKeyStore;

			return new StrongNameKey(new RSACryptoServiceProvider(csp));
		}


		readonly RSACryptoServiceProvider _rcsp;
		StrongNameKey(RSACryptoServiceProvider rcsp)
		{
			if (rcsp == null)
				throw new ArgumentNullException("rcsp");

			_rcsp = rcsp;			
		}

		/// <summary>
		/// Signs the hash.
		/// </summary>
		/// <param name="hash">The hash.</param>
		/// <returns></returns>
		public byte[] SignHash(byte[] hash)
		{
			if (hash == null)
				throw new ArgumentNullException("hash");
			else if (PublicOnly)
				throw new InvalidOperationException();

			return _rcsp.SignHash(hash, HashAlgorithmOID);
		}

		/// <summary>
		/// Verifies the hash.
		/// </summary>
		/// <param name="hash">The hash.</param>
		/// <param name="signature">The signature.</param>
		/// <returns></returns>
		public bool VerifyHash(byte[] hash, byte[] signature)
		{
			if (hash == null)
				throw new ArgumentNullException("hash");
			else if (signature == null)
				throw new ArgumentNullException("signature");

			return _rcsp.VerifyHash(hash, HashAlgorithmOID, signature);
		}		

		/// <summary>
		/// Gets the public key data in a way it can be used to create a public-only <see cref="StrongNameKey"/>
		/// </summary>
		/// <returns></returns>
		public byte[] GetPublicKeyData()
		{
			return _rcsp.ExportCspBlob(false);
		}

		string _publicKeyToken;
		/// <summary>
		/// Gets the public key token from the public key. This matches the public key token of assemblies signed with the same key
		/// </summary>
		public string PublicKeyToken
		{
			get
			{
				if (_publicKeyToken == null)
				{
					// We use the StronNameKeyPair class to orden the public key the way .Net does
					StrongNameKeyPair snkp = new StrongNameKeyPair(GetPublicKeyData());
					byte[] publicKeyHash;
					using (SHA1 sha = SHA1.Create())
					{
						publicKeyHash = sha.ComputeHash(snkp.PublicKey);
					}
					byte[] last8 = new byte[8];
					Array.Copy(publicKeyHash, publicKeyHash.Length - 8, last8, 0, 8);
					Array.Reverse(last8);
					_publicKeyToken = QQnCryptoHelpers.HashString(last8);
				}
				return _publicKeyToken;
			}
		}

		/// <summary>
		/// Gets the length of a signature in bytes
		/// </summary>
		public int SignatureLength
		{
			get
			{
				return _rcsp.KeySize / 8;
			}
		}

		/// <summary>
		/// Gets the length of a hash in bytes
		/// </summary>
		public int HashLength
		{
			get { return QQnCryptoHelpers.GetHashBits(HashType) / 8; }
		}

		/// <summary>
		/// Gets a boolean indicating whether the <see cref="StrongNameKey"/> only contains a public key
		/// </summary>
		public bool PublicOnly
		{
			get { return _rcsp.PublicOnly; }
		}

		string _hashAlgorithmName;
		string HashAlgorithmName
		{
			get
			{
				if (_hashAlgorithmName == null)
				{
					string algorithm = _rcsp.SignatureAlgorithm;

					int dash = algorithm.LastIndexOf('#');
					if (dash < 0)
						return null;

					int stripe = algorithm.IndexOf('-', dash);
					_hashAlgorithmName = algorithm.Substring(stripe + 1);
				}
				return _hashAlgorithmName;
			}
		}

		HashType _hashType;
		/// <summary>
		/// Gets the type of the hash.
		/// </summary>
		/// <value>The type of the hash.</value>
		public HashType HashType
		{
			get
			{
				if (_hashType == HashType.None)
					_hashType = (HashType)Enum.Parse(typeof(HashType), HashAlgorithmName, true);
				return _hashType;
			}
		}

		string _algorithmOid;
		/// <summary>
		/// Gets the hash algorithm OID.
		/// </summary>
		/// <value>The hash algorithm OID.</value>
		string HashAlgorithmOID
		{
			get
			{
				if(_algorithmOid == null)
					_algorithmOid = CryptoConfig.MapNameToOID(HashAlgorithmName);

				return _algorithmOid;
			}
		}

		/// <summary>
		/// Creates a new <see cref="HashAlgorithm"/> which delivers a hash which can be used with this <see cref="StrongNameKey"/>
		/// </summary>
		/// <returns></returns>
		public HashAlgorithm CreateHasher()
		{
			return QQnCryptoHelpers.CreateHashAlgorithm(HashType);
		}
	}
}
