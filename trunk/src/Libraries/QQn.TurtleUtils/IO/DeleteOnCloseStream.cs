using System;
using System.Collections.Generic;
using System.IO;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class DeleteOnCloseStream : FileStream
	{
		bool _deleteOnClose;
		string _path;


		/// <summary>
		/// Creates a new temporary file
		/// </summary>
		/// <param name="deleteOnClose">if set to <c>true</c> [delete on close].</param>
		public DeleteOnCloseStream(bool deleteOnClose)
			: base(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None)
		{
			_path = Path.GetFullPath(base.Name);
			_deleteOnClose = deleteOnClose;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DeleteOnCloseStream"/> class.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="mode">The mode.</param>
		/// <param name="deleteOnClose">if set to <c>true</c> [delete on close].</param>
		public DeleteOnCloseStream(string path, FileMode mode, bool deleteOnClose)
			: base(Path.GetFullPath(path), mode)
		{
			_path = Path.GetFullPath(Name);
			_deleteOnClose = deleteOnClose;
		}

		/// <summary>
		/// Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
		/// </summary>
		/// <remarks>Deletes the file if deleteOnClose is troe</remarks>
		public override void Close()
		{
			base.Close();

			if (_deleteOnClose)
			{
				_deleteOnClose = false;
				File.Delete(_path);
			}
		}
	}
}
