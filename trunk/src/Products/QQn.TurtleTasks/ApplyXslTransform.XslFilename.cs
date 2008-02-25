using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleTasks
{
	partial class ApplyXslTransform
	{
		class XslFilename : IEquatable<XslFilename>
		{
			string _filename;
			bool _debug;

			public XslFilename(string filename)
				: this(filename, false)
			{
			}

			public XslFilename(XslFilename filename)
				: this(SafeFilename(filename), filename.Debug)
			{
			}

			private static string SafeFilename(XslFilename filename)
			{
				if (filename == null)
					throw new ArgumentNullException("filename");

				return filename.Filename;
			}

			public XslFilename(string filename, bool debug)
			{
				if (string.IsNullOrEmpty(filename))
					throw new ArgumentNullException("filename");

				_filename = filename;
				_debug = debug;
			}

			public string Filename
			{
				get { return _filename; }
				set { _filename = value; }
			}

			public bool Debug
			{
				get { return _debug; }
				set { _debug = value; }
			}

			#region IEquatable<XslFilename> Members

			public bool Equals(XslFilename other)
			{
				if (other == null)
					return false;

				if (!string.Equals(other.Filename, Filename) || Debug != other.Debug)
					return false;

				return true;
			}

			#endregion
		}
	}
}
