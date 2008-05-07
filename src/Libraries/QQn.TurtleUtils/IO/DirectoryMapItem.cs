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
	public abstract class DirectoryMapItem : IVerifiableFile, ITokenizerInitialize
	{
		bool _initCompleted;
		string _fileName;
		DirectoryMapData _map;
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryMapFile"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		protected DirectoryMapItem(string name)
			: this()
		{
			_fileName = name;
			_initCompleted = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryMapItem"/> class.
		/// </summary>
		protected DirectoryMapItem()
		{
			//_initCompleted = false;
			_size = -1;
		}

		bool _toBeDeleted;

		internal bool ToBeDeleted
		{
			get { return _toBeDeleted; }
			set { _toBeDeleted = value; }
		}

        DirectoryMapAnnotationCollection _annotations;
        /// <summary>
        /// Gets the annotations.
        /// </summary>
        /// <value>The annotations.</value>
        [TokenGroup("A")]
        public DirectoryMapAnnotationCollection Annotations
        {
            get { return _annotations ?? (_annotations = new DirectoryMapAnnotationCollection()); }
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

		/// <summary>
		/// Called when initialization via the tokenizer starts
		/// </summary>
		/// <param name="e">The <see cref="QQn.TurtleUtils.Tokens.TokenizerEventArgs"/> instance containing the event data.</param>
		void ITokenizerInitialize.OnBeginInitialize(TokenizerEventArgs e)
		{
			OnBeginInitialize(e);			
		}

		/// <summary>
		/// Raises the <see cref="E:BeginInitialize"/> event.
		/// </summary>
		/// <param name="e">The <see cref="QQn.TurtleUtils.Tokens.TokenizerEventArgs"/> instance containing the event data.</param>
		protected virtual void OnBeginInitialize(TokenizerEventArgs e)
		{
		}
		/// <summary>
		/// Ends the init.
		/// </summary>
		/// <param name="e">The <see cref="QQn.TurtleUtils.Tokens.TokenizerEventArgs"/> instance containing the event data.</param>
		void ITokenizerInitialize.OnEndInitialize(TokenizerEventArgs e)
		{
			OnEndInitialize(e);
			_initCompleted = true;
		}

		/// <summary>
		/// Raises the <see cref="E:EndInitialize"/> event.
		/// </summary>
		/// <param name="e">The <see cref="QQn.TurtleUtils.Tokens.TokenizerEventArgs"/> instance containing the event data.</param>
		protected virtual void OnEndInitialize(TokenizerEventArgs e)
		{
		}

		#endregion

		internal class Initializer : IDisposable
		{
			readonly DirectoryMapItem _file;
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

		/// <summary>
		/// Temporarily allows updating this instance
		/// </summary>
		/// <returns></returns>
		protected internal IDisposable Updater()
		{
			return new Initializer(this);
		}

		#region ISupportInitialize Members

		internal void BeginInit()
		{
			_initCompleted = false;
			_map.Dirty = true;
		}

		internal void EndInit()
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
			get { return QQnPath.Combine(_map.FullPath, Filename); }
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
