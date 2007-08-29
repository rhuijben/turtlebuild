using System;
using System.Collections.Generic;
using System.ComponentModel;
using QQn.TurtleUtils.Tokens.Definitions;
using QQn.TurtleUtils.Tokens;
using QQn.TurtleUtils.Tokens.Converters;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using QQn.TurtleUtils.Cryptography;
using System.Diagnostics;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// 
	/// </summary>
	public class DirectoryMapFile : DirectoryMapItem, IUpdatableVerifiableFile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryMapFile"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public DirectoryMapFile(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryMapFile"/> class.
		/// </summary>
		public DirectoryMapFile()
			: base()
		{
		}

		/// <summary>
		/// Updates the data.
		/// </summary>
		/// <param name="newHash">The new hash.</param>
		/// <param name="size">The size.</param>
		/// <param name="dateTime">The date time.</param>
		internal override void UpdateData(string newHash, long size, DateTime dateTime)
		{
			using (Updater())
			{
				this.FileHash = newHash;

				this.FileSize = (size < 0L) ? -1L : size;

				this.LastWriteTimeUtc = dateTime;
			}
		}

		/// <summary>
		/// Updates this instance.
		/// </summary>
		public void Update()
		{
			using (Updater())
			{
				VerifyUtils.UpdateFile(Map.FullPath, this);
			}
		}

		#region IUpdatableVerifiableFile Members

		/// <summary>
		/// Updates the verify data.
		/// </summary>
		/// <param name="fileHash">The file hash.</param>
		/// <param name="size">The size.</param>
		/// <param name="lastWriteTimeUtc">The last write time UTC.</param>
		public void UpdateVerifyData(string fileHash, long? size, DateTime? lastWriteTimeUtc)
		{
			UpdateData(fileHash, size.Value, lastWriteTimeUtc.Value);
		}

		#endregion

		/// <summary>
		/// Unmodifieds this instance.
		/// </summary>
		/// <returns></returns>
		public bool Unmodified()
		{
			return VerifyUtils.VerifyFile(Map.FullPath, this, VerifyMode.TimeSize);
		}
	}	
}
