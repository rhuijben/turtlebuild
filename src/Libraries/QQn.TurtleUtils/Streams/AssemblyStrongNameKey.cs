using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;

namespace QQn.TurtleUtils.Streams
{
	/// <summary>
	/// An in-memory wrapper of a StrongKeyName (aka Snk file)
	/// </summary>
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
		/// 
		/// </summary>
		/// <param name="stream"></param>
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
		/// 
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
		/// 
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
		/// 
		/// </summary>
		/// <param name="hash"></param>
		/// <returns></returns>
		public byte[] SignHash(byte[] hash)
		{
			if (hash == null)
				throw new ArgumentNullException("hash");

			return _rcsp.SignHash(hash, CryptoConfig.MapNameToOID(HashAlgorithmName));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hash"></param>
		/// <param name="signature"></param>
		/// <returns></returns>
		public bool VerifyHash(byte[] hash, byte[] signature)
		{
			if (hash == null)
				throw new ArgumentNullException("hash");
			else if (signature == null)
				throw new ArgumentNullException("signature");

			return _rcsp.VerifyHash(hash, CryptoConfig.MapNameToOID(HashAlgorithmName), signature);
		}

		/// <summary>
		/// Gets the public key data in a way it can be used to create a public-only <see cref="StrongNameKey"/>
		/// </summary>
		/// <returns></returns>
		public byte[] GetPublicKeyData()
		{
			return _rcsp.ExportCspBlob(false);
		}

		byte[] _publicKeyToken;
		/// <summary>
		/// Gets the public key token from the public key. This matches the public key token of assemblies signed with the same key
		/// </summary>
		public byte[] PublicKeyToken
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
					_publicKeyToken = last8;
				}
				return (byte[])_publicKeyToken.Clone();
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

		int _hashLength;
		/// <summary>
		/// Gets the length of a hash in bytes
		/// </summary>
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

		/// <summary>
		/// Gets a boolean indicating whether the <see cref="StrongNameKey"/> only contains a public key
		/// </summary>
		public bool PublicOnly
		{
			get { return _rcsp.PublicOnly; }
		}

		string HashAlgorithmName
		{
			get
			{
				string alg = _rcsp.SignatureAlgorithm;
				alg = alg.Substring(alg.LastIndexOf('#') + 1);
				string[] parts = alg.Split('-');

				return parts[1];
			}
		}

		/// <summary>
		/// Creates a new <see cref="HashAlgorithm"/> which delivers a hash which can be used with this <see cref="StrongNameKey"/>
		/// </summary>
		/// <returns></returns>
		public HashAlgorithm CreateHasher()
		{
			return HashAlgorithm.Create(HashAlgorithmName);
		}
	}
}
