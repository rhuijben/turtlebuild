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
				CspParameters csp = new CspParameters(1);

				RSACryptoServiceProvider rcsp = new RSACryptoServiceProvider(csp);
				rcsp.ImportCspBlob(br.ReadBytes((int)br.BaseStream.Length));

				return new AssemblyStrongNameKey(rcsp);
			}
		}

		public static AssemblyStrongNameKey LoadFrom(byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException("bytes");

			using (MemoryStream ms = new MemoryStream(bytes))
			{
				return LoadFrom(ms);
			}
		}

		public static AssemblyStrongNameKey LoadFromContainer(string containerName, bool machineScope)
		{
			if (string.IsNullOrEmpty(containerName))
				throw new ArgumentNullException("containerName");

			CspParameters csp = new CspParameters();
			csp.KeyContainerName = containerName;
			csp.KeyNumber = 2;
			csp.Flags = CspProviderFlags.UseNonExportableKey;
			if (machineScope)
				csp.Flags |= CspProviderFlags.UseMachineKeyStore;

			return new AssemblyStrongNameKey(new RSACryptoServiceProvider(csp));
		}


		readonly RSACryptoServiceProvider _rcsp;
		AssemblyStrongNameKey(RSACryptoServiceProvider rcsp)
		{
			if (rcsp == null)
				throw new ArgumentNullException("rcsp");

			_rcsp = rcsp;			
		}

		public byte[] SignHash(byte[] hash)
		{
			if (hash == null)
				throw new ArgumentNullException("hash");

			return _rcsp.SignHash(hash, CryptoConfig.MapNameToOID(HashAlgorithmName));
		}

		public bool VerifyHash(byte[] hash, byte[] signature)
		{
			if (hash == null)
				throw new ArgumentNullException("hash");
			else if (signature == null)
				throw new ArgumentNullException("signature");

			return _rcsp.VerifyHash(hash, CryptoConfig.MapNameToOID(HashAlgorithmName), signature);
		}

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
				return _publicKeyToken;
			}
		}

		public int SignatureLength
		{
			get
			{
				return _rcsp.KeySize / 8;
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

		public HashAlgorithm CreateHasher()
		{
			return HashAlgorithm.Create(HashAlgorithmName);
		}
	}
}
