using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleBuildUtils;
using QQn.TurtlePackager.Origins;

namespace QQn.TurtlePackager
{
	class FileData : IHasFileName
	{
		readonly FileDataList _list;
		string _filename;
		string _copiedFrom;
		Origin _origin;
		bool _findOrigin;

		public FileData(string filename, FileDataList list)
		{
			if (String.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");
			else if (list == null)
				throw new ArgumentNullException("list");

			_filename = filename;
			_list = list;
		}

		#region IHasFileName Members

		public string FileName
		{
			get { return _filename; }
		}

		#endregion

		public string CopiedFrom
		{
			get { return _copiedFrom; }
			set { _copiedFrom = value; }
		}

		public Origin Origin
		{
			get { return _origin; }
			set { _origin = value; }
		}

		public bool FindOrigin
		{
			get { return _findOrigin; }
			set { _findOrigin = value; }
		}
	}

	class FileDataList : FileCollection<FileData>
	{
		public FileDataList()
		{
		}
	}
}
