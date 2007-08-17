using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using QQn.TurtleUtils.Tokens.Definitions;
using QQn.TurtleUtils.Tokens;
using QQn.TurtleUtils.Tokens.Converters;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// 
	/// </summary>
	public class DirectoryMapFile : IVerifiableFile, ITokenizerInitialize
	{
		bool _initCompleted;
		string _fileName;
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryMapFile"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public DirectoryMapFile(string name)
		{
			_fileName = name;
			_initCompleted = true;
		}

		internal DirectoryMapFile()
		{
			_initCompleted = false;
		}

		string _hash;
		long _size;
		DateTime _lwt;
		
		#region IVerifiableFile Members

		/// <summary>
		/// Gets the filename.
		/// </summary>
		/// <value>The filename.</value>
		[Token("name")]
		public string Filename
		{
			get { return _fileName; }
			set
			{
				if (_initCompleted)
					throw new InvalidOperationException();

				_fileName = value;
			}
		}

		/// <summary>
		/// Gets the file hash.
		/// </summary>
		/// <value>The file hash.</value>
		public string FileHash
		{
			get { return _hash; }
			set
			{
				if (_initCompleted)
					throw new InvalidOperationException();

				_hash = value;
			}
		}

		/// <summary>
		/// Gets the size of the file.
		/// </summary>
		/// <value>The size of the file.</value>
		public long FileSize
		{
			get { return _size; }
			set
			{
				if (_initCompleted)
					throw new InvalidOperationException();

				_size = value;
			}
		}

		/// <summary>
		/// Gets the last written time in UTC.
		/// </summary>
		/// <value>The last written time in UTC.</value>
		[Token("time", TypeConverter=typeof(TicksDateTimeConverter))]
		public DateTime? LastWriteTimeUtc
		{
			get { return _lwt; }
			set
			{
				if (_initCompleted)
					throw new InvalidOperationException();

				_lwt = value.Value;
			}
		}

		#endregion

		#region ITokenizerInit Members

		void ITokenizerInitialize.EndInit()
		{
			_initCompleted = true;
		}

		#endregion
	}
}
