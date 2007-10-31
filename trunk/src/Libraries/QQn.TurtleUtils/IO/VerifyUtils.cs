using System;
using System.Collections.Generic;
using System.IO;
using QQn.TurtleUtils.Cryptography;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// Represents a file which can be compared with a physical file (on disk) or an other <see cref="IVerifiableFile"/>
	/// </summary>
	public interface IVerifiableFile
	{
		/// <summary>
		/// Gets the filename.
		/// </summary>
		/// <value>The filename.</value>
		string Filename { get; }

		/// <summary>
		/// Gets the file hash.
		/// </summary>
		/// <value>The file hash.</value>
		string FileHash { get; }

		/// <summary>
		/// Gets the size of the file.
		/// </summary>
		/// <value>The size of the file.</value>
		long FileSize { get; }

		/// <summary>
		/// Gets the last written time in UTC.
		/// </summary>
		/// <value>The last written time in UTC.</value>
		DateTime? LastWriteTimeUtc { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IUpdatableVerifiableFile : IVerifiableFile
	{
		/// <summary>
		/// Updates the verify data.
		/// </summary>
		/// <param name="fileHash">The file hash.</param>
		/// <param name="size">The size.</param>
		/// <param name="lastWriteTimeUtc">The last write time UTC.</param>
		void UpdateVerifyData(string fileHash, long? size, DateTime? lastWriteTimeUtc);
	}


	/// <summary>
	/// Specifies the level of verification that must be performed to tell the file is equal
	/// </summary>
	public enum VerifyMode
	{
		/// <summary>
		/// Specifies no verification should be done; only the existence of the file is checked
		/// </summary>
		None,
		/// <summary>
		/// The time of the file is checked
		/// </summary>
		Time,
		/// <summary>
		/// FileTime and FileSize are checked
		/// </summary>
		TimeSize,
		/// <summary>
		/// FileTime, FileSize and FileHash are checked
		/// </summary>
		FileHash,
		/// <summary>
		/// Full check
		/// </summary>
		Full=Int32.MaxValue
	}

	/// <summary>
	/// 
	/// </summary>
	public static class VerifyUtils
	{
		/// <summary>
		/// Verifies the file.
		/// </summary>
		/// <param name="baseDirectory">The base directory.</param>
		/// <param name="verifyData">The verify data.</param>
		/// <param name="verificationMode">The verification mode.</param>
		/// <returns></returns>
		public static bool VerifyFile(string baseDirectory, IVerifiableFile verifyData, VerifyMode verificationMode)		
		{
			if (string.IsNullOrEmpty(baseDirectory))
				throw new ArgumentNullException("baseDirectory");
			else if (verifyData == null)
				throw new ArgumentNullException("verifyData");

			string path = QQnPath.Combine(baseDirectory, verifyData.Filename);

			FileInfo fif = new FileInfo(path);

			if (!fif.Exists)
				return false;

			if (verificationMode < VerifyMode.Time)
				return true;

			bool hasTime = verifyData.LastWriteTimeUtc.HasValue;
			bool timeFailed = hasTime && fif.LastWriteTimeUtc != verifyData.LastWriteTimeUtc.Value;
			if (timeFailed && verificationMode == VerifyMode.Time)
				return false;

			if (verificationMode < VerifyMode.TimeSize)
				return true;

			bool hasSize = (verifyData.FileSize >= 0);
			bool sizeFailed = hasSize && fif.Length != verifyData.FileSize;
			if((timeFailed || sizeFailed) && verificationMode == VerifyMode.TimeSize)
				return false;

			if (verificationMode < VerifyMode.FileHash)
				return true;

			if (!string.IsNullOrEmpty(verifyData.FileHash) && !QQnCryptoHelpers.VerifyFileHash(fif.FullName, verifyData.FileHash))
				return false;

			return true;
		}

		/// <summary>
		/// Checks whether the two verifiers verify the same file
		/// </summary>
		/// <param name="one">The one.</param>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public static bool AreEqualVerifiers(IVerifiableFile one, IVerifiableFile other)
		{
			if (one == null)
				throw new ArgumentNullException("one");
			else if (other == null)
				throw new ArgumentNullException("other");

			if ((one.FileHash != null) && (other.FileHash != null))
			{
				// Compare hashes without size
				if (!string.Equals(QQnCryptoHelpers.NormalizeHashValue(one.FileHash, true), QQnCryptoHelpers.NormalizeHashValue(other.FileHash, true)))
					return false;
			}

			long oneSize = one.FileSize;
			long otherSize = other.FileSize;

			// Get sizes from hash if they are not available directly
			if ((oneSize < 0) && one.FileHash != null)
				oneSize = QQnCryptoHelpers.GetSizeFromHash(one.FileHash);

			if ((otherSize < 0) && other.FileHash != null)
				otherSize = QQnCryptoHelpers.GetSizeFromHash(other.FileHash);

			if ((oneSize >= 0) && (otherSize >= 0) && (oneSize != otherSize))
				return false;

			if (one.LastWriteTimeUtc.HasValue && other.LastWriteTimeUtc.HasValue && one.LastWriteTimeUtc.Value != other.LastWriteTimeUtc.Value)
				return false;

			return true;
		}

		/// <summary>
		/// Updates the file.
		/// </summary>
		/// <param name="baseDirectory">The base directory.</param>
		/// <param name="verifyData">The verify data.</param>
		public static void UpdateFile(string baseDirectory, IUpdatableVerifiableFile verifyData)
		{
			if (string.IsNullOrEmpty(baseDirectory))
				throw new ArgumentNullException("baseDirectory");
			else if (verifyData == null)
				throw new ArgumentNullException("verifyData");

			UpdateFile(baseDirectory, verifyData, QQnCryptoHelpers.DefaultHashType | HashType.PlusType);
		}

		/// <summary>
		/// Updates the file.
		/// </summary>
		/// <param name="baseDirectory">The base directory.</param>
		/// <param name="verifyData">The verify data.</param>
		/// <param name="hashType">Type of the hash.</param>
		public static void UpdateFile(string baseDirectory, IUpdatableVerifiableFile verifyData, HashType hashType)
		{
			if (string.IsNullOrEmpty(baseDirectory))
				throw new ArgumentNullException("baseDirectory");
			else if (verifyData == null)
				throw new ArgumentNullException("verifyData");

			FileInfo fif = new FileInfo(QQnPath.Combine(baseDirectory, verifyData.Filename));
			if (!fif.Exists)
				return;

			long size = fif.Length;
			string hash = QQnCryptoHelpers.CalculateFileHash(fif.FullName, hashType);
			DateTime dt = fif.LastWriteTimeUtc;

			verifyData.UpdateVerifyData(hash, size, dt);
		}
	}
}
