using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Streams;
using QQn.TurtleUtils.Tokenizer;
using System.IO;
using System.ComponentModel;

namespace QQn.TurtlePackage
{
	public class PackFile : PackItem, IVerifiableFile
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

		[Token("lastWritten", TypeConverter=typeof(QQn.TurtleUtils.Tokenizer.Converters.UtcDateTimeConverter)), DefaultValue(typeof(DateTime), "")]
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

		static string NormalizePath(string value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			return value.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
		}

		#region IVerifiableFile Members

		string IVerifiableFile.Filename
		{
			get { return Name; }
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
	}
}
