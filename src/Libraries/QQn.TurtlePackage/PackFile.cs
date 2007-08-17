using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using QQn.TurtleUtils.IO;
using QQn.TurtleUtils.Tokens;

namespace QQn.TurtlePackage
{
	[DebuggerDisplay("File={Name}")]
	public class PackFile : PackItem, IVerifiableFile, IUpdatableVerifiableFile
	{
		long _size = -1L;
		string _hash;
		DateTime _lastWriteTimeUtc;

		[Token("size"), DefaultValue(-1L)]
		public long FileSize
		{
			get { return _size; }
			set { EnsureWritable(); _size = value; }
		}

		[Token("hash"), DefaultValue(null)]
		public string FileHash
		{
			get { return _hash; }
			set { EnsureWritable(); _hash = value; }
		}

		[Token("lastWritten", TypeConverter=typeof(QQn.TurtleUtils.Tokens.Converters.UtcDateTimeConverter)), DefaultValue(typeof(DateTime), "")]
		public DateTime LastWriteTimeUtc
		{
			get { return _lastWriteTimeUtc; }
			set 
			{ 
				EnsureWritable(); // Always save as UTC
				if(value.Kind != DateTimeKind.Utc)
					value = value.ToUniversalTime();

				long offsetTicks = value.Ticks % 10000000;
				if (offsetTicks != 0)
				{
					if (offsetTicks >= 5000000)
						value = value.AddTicks(-offsetTicks);
					else
						value = value.AddTicks(10000000 - offsetTicks);
				}
				
				_lastWriteTimeUtc = value; 
			}
		}

		[Token("name")]
		public override string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = NormalizePath(value);
			}
		}		

		#region IVerifiableFile Members

		string IVerifiableFile.Filename
		{
			get 
			{
				string basePath = BaseDir;

				if (basePath != null)
					return Path.Combine(basePath, Name);
				else
					return Name;
			}
		}

		string IVerifiableFile.FileHash
		{
			get { return FileHash; }
		}

		long IVerifiableFile.FileSize
		{
			get { return FileSize; }
		}

		DateTime? IVerifiableFile.LastWriteTimeUtc
		{
			get { return LastWriteTimeUtc; }
		}

		#endregion

		#region IUpdatableVerifiableFile Members

		/// <summary>
		/// Updates the verify data.
		/// </summary>
		/// <param name="fileHash">The file hash.</param>
		/// <param name="size">The size.</param>
		/// <param name="lastWriteTimeUtc">The last write time UTC.</param>
		void IUpdatableVerifiableFile.UpdateVerifyData(string fileHash, long? size, DateTime? lastWriteTimeUtc)
		{
			EnsureWritable();
			if (fileHash != null)
				FileHash = fileHash;
			if (size.HasValue)
				FileSize = size.Value;
			if (lastWriteTimeUtc.HasValue)
				LastWriteTimeUtc = lastWriteTimeUtc.Value;			
		}

		#endregion
	}
}
