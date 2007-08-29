using System;
using System.Collections.Generic;
using QQn.TurtleUtils.Tokens.Definitions;
using System.ComponentModel;
using QQn.TurtleUtils.Tokens;
using QQn.TurtleUtils.Tokens.Converters;
using System.IO;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// 
	/// </summary>
	public class DirectoryMapItem : IVerifiableFile, ITokenizerInitialize, ISupportInitialize
	{
		bool _initCompleted;
		string _fileName;
		DirectoryMapData _map;
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryMapFile"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public DirectoryMapItem(string name)
			: this()
		{
			_fileName = name;
			_initCompleted = true;
		}

		internal DirectoryMapItem()
		{
			_initCompleted = false;
			_size = -1;
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
				EnsureWritable();

				_fileName = value;
			}
		}

		/// <summary>
		/// Ensures the item is writable
		/// </summary>
		protected void EnsureWritable()
		{
			if (_initCompleted)
				throw new InvalidOperationException();
		}		

		/// <summary>
		/// Gets the file hash.
		/// </summary>
		/// <value>The file hash.</value>
		[Token("hash")]
		public string FileHash
		{
			get { return _hash; }
			set
			{
				EnsureWritable();

				_hash = value;
			}
		}

		/// <summary>
		/// Gets the size of the file.
		/// </summary>
		/// <value>The size of the file.</value>
		[Token("size"), DefaultValue(-1L)]
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
		[Token("time", TypeConverter = typeof(TicksDateTimeConverter)), DefaultValue(typeof(DateTime),"")]
		public DateTime? LastWriteTimeUtc
		{
			get { return _lwt; }
			set
			{
				EnsureWritable();
				if (!value.HasValue)
					throw new ArgumentNullException("value");

				_lwt = value.Value.ToUniversalTime();
			}
		}

		#endregion

		#region ITokenizerInit Members

		void ITokenizerInitialize.BeginInitialize(TokenizerEventArgs e)
		{
			
		}
		/// <summary>
		/// Ends the init.
		/// </summary>
		/// <param name="e">The <see cref="QQn.TurtleUtils.Tokens.TokenizerEventArgs"/> instance containing the event data.</param>
		void ITokenizerInitialize.EndInitialize(TokenizerEventArgs e)
		{
			_initCompleted = true;
		}

		#endregion

		internal class Initializer : IDisposable
		{
			readonly ISupportInitialize _file;
			public Initializer(DirectoryMapItem file)
			{
				_file = file;
				_file.BeginInit();
			}

			public void Dispose()
			{
				_file.EndInit();

			}
		}

		internal Initializer Updater()
		{
			return new Initializer(this);
		}

		#region ISupportInitialize Members

		void ISupportInitialize.BeginInit()
		{
			_initCompleted = false;
			_map.Dirty = true;
		}

		void ISupportInitialize.EndInit()
		{
			_initCompleted = true;
		}

		#endregion

		/// <summary>
		/// Gets the full name.
		/// </summary>
		/// <value>The full name.</value>
		public string FullName
		{
			get { return Path.Combine(_map.FullPath, Filename); }
		}

		internal DirectoryMapData Map
		{
			get { return _map; }
			set { _map = value; }
		}

		internal virtual void UpdateData(string newHash, long size, DateTime dateTime)
		{
			//throw new Exception("The method or operation is not implemented.");
			throw new InvalidOperationException();
		}
	}
}
