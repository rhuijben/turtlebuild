using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace QQn.TurtleBuildUtils
{
	public class FileList : SortedList<string, string>, IEnumerable<string>
	{
		public FileList()
			: base(StringComparer.InvariantCultureIgnoreCase)
		{
		}

		public void Add(string filename)
		{
			Add(filename, filename);
		}

		public new IEnumerator<string> GetEnumerator()
		{
			return Keys.GetEnumerator();
		}

		public bool Contains(string filename)
		{
			return ContainsKey(filename);
		}
	}
}
