using System;

namespace QQn.TurtleTasks
{
	sealed class ExtractItem
	{
		readonly Uri _uri;
		readonly string _file;
		readonly string _toDir;
		readonly string _prefix;
		readonly string _name;
		bool _isUpdated;

		public ExtractItem(Uri uri, string file, string toDir, string prefix, string filename)
		{
			_uri = uri;
			_file = file;
			_toDir = toDir;
			_prefix = prefix;
			_name = filename;
		}

		public Uri Uri
		{
			get { return _uri; }
		}

		public string TmpFile
		{
			get { return _file; }
		}

		public string ToDir
		{
			get { return _toDir; }
		}

		public string Prefix
		{
			get { return _prefix; }
		}

		public bool IsUpdated
		{
			get { return _isUpdated; }
			set { _isUpdated = value; }
		}

		public string Name
		{
			get { return _name; }
		}
	}
}
