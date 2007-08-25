using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace QQn.TurtleBuildUtils
{
	/// <summary>
	/// 
	/// </summary>
	public class SortedFileList : SortedList<string, string>, IEnumerable<string>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SortedFileList"/> class.
		/// </summary>
		public SortedFileList()
			: base(StringComparer.InvariantCultureIgnoreCase)
		{
		}

		/// <summary>
		/// Adds the specified filename.
		/// </summary>
		/// <param name="filename">The filename.</param>
		public virtual void Add(string filename)
		{
			Add(filename, filename);
		}

		/// <summary>
		/// Adds the specified file if it was not already added
		/// </summary>
		/// <param name="filename"></param>
		public virtual void AddUnique(string filename)
		{
			if (!ContainsKey(filename))
				Add(filename, filename);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator&lt;string&gt;"/> that can be used to iterate through the collection.
		/// </returns>
		public new IEnumerator<string> GetEnumerator()
		{
			return Keys.GetEnumerator();
		}

		/// <summary>
		/// Determines whether [contains] [the specified filename].
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <returns>
		/// 	<c>true</c> if the list contains the specified filename; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(string filename)
		{
			return ContainsKey(filename);
		}
	}
}
