using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.IO;
using System.IO;
using System.Diagnostics;

namespace QQn.TurtleBuildUtils
{
	public class SortedRelativeFileList : SortedFileList
	{
		string _basePath;

		public string BasePath
		{
			[DebuggerStepThrough]
			get { return _basePath; }
			set
			{
				if (Count == 0)
					_basePath = value;
				else
					throw new InvalidOperationException("Can't set basepath of SortedRelativeFileList if items are added");
			}
		}

		/// <summary>
		/// Adds the specified filename.
		/// </summary>
		/// <param name="filename">The filename.</param>
		public override void Add(string filename)
		{
			base.Add(ForceRelative(filename));
		}

		/// <summary>
		/// Adds the specified file if it was not already added
		/// </summary>
		/// <param name="filename"></param>
		public override void AddUnique(string filename)
		{
			base.AddUnique(ForceRelative(filename));
		}

		static readonly string dotSlash = "." + Path.DirectorySeparatorChar;
		/// <summary>
		/// Forces the specified filename to be relative from the basePath
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <returns></returns>
		protected string ForceRelative(string filename)
		{
			if (string.IsNullOrEmpty(BasePath))
				return Path.GetFullPath(filename);
			else if(Path.IsPathRooted(filename))
				return QQnPath.MakeRelativePath(BasePath, filename);
			else if(filename.IndexOf(Path.AltDirectorySeparatorChar) >= 0 || filename.Contains(dotSlash))
				return QQnPath.MakeRelativePath(BasePath, Path.Combine(BasePath, filename));
			else
				return filename;			
		}
	}
}
