using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Net;

namespace QQn.TurtleUtils.Cryptography
{
	/// <summary>
	/// Crypto utilities; used by the other components
	/// </summary>
	public static class QQnCryptoHelpers
	{
		/// <summary>
		/// Generates a string version of the byte-array
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <returns></returns>
		public static string HashString(byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException("bytes");
			StringBuilder sb = new StringBuilder(bytes.Length * 2);

			foreach (byte b in bytes)
				sb.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);

			return sb.ToString();
		}

		static HashAlgorithm CreateAlgorithm(HashType hashType)
		{
			switch (hashType & ~(HashType.PlusSize | HashType.PlusType))
			{
				case HashType.MD5:
					return MD5.Create();
				case HashType.SHA1:
					return SHA1.Create();
				case HashType.SHA256:
					return SHA256.Create();
				case HashType.SHA512:
					return SHA512.Create();
				default:
					throw new ArgumentException("Invalid hashtype", "hashType");
			}
		}

		/// <summary>
		/// Calculates the file hash.
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <param name="hashType">Type of the hash.</param>
		/// <returns></returns>
		public static string CalculateFileHash(string filename, HashType hashType)
		{
			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");
			else if (!File.Exists(filename))
				throw new FileNotFoundException("File not found", filename);

			// Open the stream with some extra buffering
			using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 16384))
			{
				return CalculateStreamHash(fs, hashType);
			}
		}

		/// <summary>
		/// Calculates the stream hash.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="hashType">Type of the hash.</param>
		/// <returns></returns>
		public static string CalculateStreamHash(Stream stream, HashType hashType)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			if (hashType == HashType.Null)
				hashType = HashType.SHA256 | HashType.PlusType | HashType.PlusSize;

			long length;
			byte[] bytes;
			using (HashAlgorithm ha = CreateAlgorithm(hashType))
			{
				length = stream.Length;
				bytes = ha.ComputeHash(stream);
			}

			StringBuilder sb = new StringBuilder(bytes.Length * 2 + 64);

			foreach (byte b in bytes)
				sb.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);

			if ((hashType & HashType.PlusType) != 0)
				sb.AppendFormat(CultureInfo.InvariantCulture, ",type={0}", (hashType & ~(HashType.PlusSize | HashType.PlusType)).ToString());
			if ((hashType & HashType.PlusSize) != 0)
				sb.AppendFormat(CultureInfo.InvariantCulture, ",size={0}", length);

			return sb.ToString();
		}

		static bool TryParsehash(string hash, out HashType hashType, out long length, out string hashValue)
		{
			string[] parts = hash.Split(new char[] { ',' }, 3);
			int len = parts[0].Length;


			hashType = HashType.Null;
			length = -1;
			hashValue = parts[0];

			foreach (string p in parts)
			{
				if (p.StartsWith("type=", StringComparison.InvariantCultureIgnoreCase))
				{
					try
					{
						hashType = (HashType)Enum.Parse(typeof(HashType), p.Substring(5), true) & ~(HashType.PlusSize | HashType.PlusType);

						if (!Enum.IsDefined(typeof(HashType), hashType))
							hashType = HashType.Null;
					}
					catch
					{
						hashType = HashType.Null;
					}
				}
				else if (p.StartsWith("size=", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!long.TryParse(p.Substring(5), NumberStyles.None, CultureInfo.InvariantCulture, out length))
						length = -1;
					else if (length < 0)
						length = -1;
				}
				else if (p.StartsWith("hash=", StringComparison.InvariantCultureIgnoreCase))
				{
					hashValue = p.Substring(5);
				}
			}

			if (hashType == HashType.Null)
			{
				switch (len)
				{
					case 32:
						hashType = HashType.MD5;
						break;
					case 40:
						hashType = HashType.SHA1;
						break;
					case 64:
						hashType = HashType.SHA256;
						break;
					case 128:
						hashType = HashType.SHA512;
						break;
					default:
						return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Verifies if the hash matches the specified file
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <param name="hash">The hash.</param>
		/// <returns></returns>
		public static bool VerifyFileHash(string filename, string hash)
		{
			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");
			else if (!File.Exists(filename))
				throw new FileNotFoundException("File not found", filename);

			HashType hashType;
			string hashValue;
			long length;

			if(!TryParsehash(hash, out hashType, out length, out hashValue))
				return false;

			if(length >= 0 && new FileInfo(filename).Length != length)
				return false;

			return string.Equals(CalculateFileHash(filename, hashType), hashValue, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Verifies if the given hash matches the stream
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="hash">The hash.</param>
		/// <returns></returns>
		public static bool VerifyStreamHash(Stream stream, string hash)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			HashType hashType;
			string hashValue;
			long length;

			if (!TryParsehash(hash, out hashType, out length, out hashValue))
				return false;

			if (length >= 0 && stream.Length != length)
				return false;

			return string.Equals(CalculateStreamHash(stream, hashType), hashValue, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Generates a unique guid from the specified hash value with the algorithm specified by RFC 4122
		/// </summary>
		/// <param name="baseGuid">The base GUID.</param>
		/// <param name="bytesToHash">The bytes to hash.</param>
		/// <returns>The guid</returns>
		public static Guid GuidFromHash(Guid baseGuid, byte[] bytesToHash)
		{
			if(bytesToHash == null)
				throw new ArgumentNullException("bytesToHash");

			// See RFC 4122 for C implementation examples

			byte[] hash;
			using (SHA1 sha1 = SHA1.Create())
			{
				sha1.TransformBlock(ToNetworkOrder(baseGuid.ToByteArray()), 0, 16, null, 0);
				sha1.TransformFinalBlock(bytesToHash, 0, bytesToHash.Length);

				hash = sha1.Hash;
			}

			hash = ToHostOrder(hash); // Treat as guid

			Int32 timeLow = BitConverter.ToInt32(hash, 0);
			Int16 timeMid = BitConverter.ToInt16(hash, 4);
			Int16 timeHiAndVersion = BitConverter.ToInt16(hash, 6);
			Byte clockSeqHi = hash[8];
			Byte clockSeqLow = hash[9];

			return new Guid(timeLow, timeMid, (short)((timeHiAndVersion & 0xFFF) | (5 << 12)), (byte)((clockSeqHi & 0x35) | 0x80), clockSeqLow, hash[10], hash[11], hash[12], hash[13], hash[14], hash[15]);
		}

		/// <summary>
		/// Calculates a unique guid based on baseGuid and stringToHash
		/// </summary>
		/// <param name="baseGuid"></param>
		/// <param name="stringToHash"></param>
		/// <returns></returns>
		/// <remarks>Uses the UTF-8 representation of stringToHash with <see cref="GuidFromHash"/></remarks>
		public static Guid GuidFromString(Guid baseGuid, string stringToHash)
		{
			if (stringToHash == null)
				throw new ArgumentNullException("bytesToHash");

			return GuidFromHash(baseGuid, Encoding.UTF8.GetBytes(stringToHash));
		}

		private static byte[] ToHostOrder(byte[] a)
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(a, 0, 4);
				Array.Reverse(a, 4, 2);
				Array.Reverse(a, 6, 2);
			}
			return a;
		}

		private static byte[] ToNetworkOrder(byte[] a)
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(a, 0, 4);
				Array.Reverse(a, 4, 2);
				Array.Reverse(a, 6, 2);
			}

			return a;
		}
	}
}