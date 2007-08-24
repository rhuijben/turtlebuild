using System;
using System.Collections.Generic;
using System.Text;
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

		public void Update()
		{
			using (Updater())
			{
				VerifyUtils.UpdateFile(Map.FullPath, this);
			}
		}

		#region IUpdatableVerifiableFile Members

		public void UpdateVerifyData(string fileHash, long? size, DateTime? lastWriteTimeUtc)
		{
			UpdateData(fileHash, size.Value, lastWriteTimeUtc.Value);
		}

		#endregion

		public bool Unmodified()
		{
			return VerifyUtils.VerifyFile(Map.FullPath, this, VerifyMode.TimeSize);
		}
	}	
}
