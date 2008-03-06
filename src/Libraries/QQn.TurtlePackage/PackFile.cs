using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using QQn.TurtleUtils.IO;
using QQn.TurtleUtils.Tokens;
using System.Diagnostics.CodeAnalysis;

namespace QQn.TurtlePackage
{
	/// <summary>
	/// 
	/// </summary>
	[DebuggerDisplay("File={Name}")]
	public class PackFile : PackItem, IVerifiableFile, IUpdatableVerifiableFile
	{
		long _size = -1L;
		string _hash;
		DateTime _lastWriteTimeUtc;
		string _assemblyName;
		string _debugId;
		string _streamName;

		/// <summary>
		/// Gets the size of the file.
		/// </summary>
		/// <value>The size of the file.</value>
		[Token("size"), DefaultValue(-1L)]
		public long FileSize
		{
			get { return _size; }
			set { EnsureWritable(); _size = value; }
		}

		/// <summary>
		/// Gets the file hash.
		/// </summary>
		/// <value>The file hash.</value>
		[Token("hash"), DefaultValue(null)]
		public string FileHash
		{
			get { return _hash; }
			set { EnsureWritable(); _hash = value; }
		}

		/// <summary>
		/// Gets the assemblyname of the assembly.
		/// </summary>
		/// <value>The name of the assembly.</value>
		[Token("assemblyName"), DefaultValue(null)]
		public string AssemblyName
		{
			get { return _assemblyName; }
			set { EnsureWritable(); _assemblyName = value; }
		}

		/// <summary>
		/// Gets or sets the debug id.
		/// </summary>
		/// <value>The debug id.</value>
		[Token("debugId"), DefaultValue(null)]
		public string DebugId
		{
			get { return _debugId; }
			set { EnsureWritable(); _debugId = value; }
		}

		/// <summary>
		/// Gets the full name (based on BaseDir and Name)
		/// </summary>
		/// <value>The full name.</value>
		public string FullName
		{
			get { return QQnPath.Combine(BaseDir, Name); }
		}

		/// <summary>
		/// Gets the name of the stream within the package
		/// </summary>
		/// <value>The name of the stream.</value>
		[Token("streamName"), DefaultValue(null)]
		public string StreamName
		{
			get { return _streamName; }
			set { EnsureWritable(); _streamName = value; }
		}

		/// <summary>
		/// Gets the last written time in UTC.
		/// </summary>
		/// <value>The last written time in UTC.</value>
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

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
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

		/// <summary>
		/// Gets the container which contains the file
		/// </summary>
		/// <value>The container.</value>
		public PackContainer Container
		{
			get { return (PackContainer)Parent; }
		}


		/// <summary>
		/// Gets the relative path.
		/// </summary>
		/// <value>The relative path.</value>
		public string RelativePath
		{
			get
			{
				string basePath = BaseDir;

				if (basePath != null)
					return QQnPath.Combine(basePath, Name);
				else
					return Name;
			}
		}

		#region IVerifiableFile Members

		string IVerifiableFile.Filename
		{
			[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
			get 
			{
				string basePath = BaseDir;

				if (basePath != null)
					return QQnPath.Combine(basePath, Name);
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
		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
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
