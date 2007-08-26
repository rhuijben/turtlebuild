using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;

namespace QQn.TurtleUtils.IO
{
	static class NativeMethods
	{
		[DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool PathRelativePathTo(StringBuilder pszPath, string pszFrom, [MarshalAs(UnmanagedType.U4)] int dwAttrFrom, string pszTo, [MarshalAs(UnmanagedType.U4)] int dwAttrTo);
	}

	/// <summary>
	/// Static wrapper for several path utilities
	/// </summary>
	public static class QQnPath
	{
		/// <summary>
		/// Tries to make a relative path
		/// </summary>
		/// <param name="path"></param>
		/// <param name="relativeFrom"></param>
		/// <returns>The relative path; an absolute path is returned if a relative is not available</returns>
		public static string GetRelativePath(string path, string relativeFrom)
		{
			if (path == null || path.Length == 0)
				throw new ArgumentNullException("path");
			if (relativeFrom == null || path.Length == 0)
				throw new ArgumentNullException("relativeFrom");

			path = Path.GetFullPath(path);
			relativeFrom = Path.GetFullPath(relativeFrom);

			string pathRoot = Path.GetPathRoot(path);
			string relRoot = Path.GetPathRoot(relativeFrom);

			if (0 != string.Compare(pathRoot, relRoot, true, CultureInfo.InvariantCulture))
				return Path.GetFullPath(path);

			const int FILE_ATTRIBUTE_FILE = 0x00000000;
			const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
			StringBuilder result = new StringBuilder(260);
			if (NativeMethods.PathRelativePathTo(result, relativeFrom, FILE_ATTRIBUTE_DIRECTORY, path, FILE_ATTRIBUTE_FILE))
			{
				string p = result.ToString();
				if (p.Length > 2 && p[0] == '.' && (p[1] == Path.DirectorySeparatorChar || p[1] == Path.AltDirectorySeparatorChar))
					p = p.Substring(2);

				return p;
			}

			return Path.GetFullPath(path);
		}


		/// <summary>
		/// Makes the itemPath relative to the origin directory
		/// </summary>
		/// <param name="originDirectory">The origin directory.</param>
		/// <param name="itemPath">The item path.</param>
		/// <returns></returns>
		public static string MakeRelativePath(string originDirectory, string itemPath)
		{
			if (string.IsNullOrEmpty(originDirectory))
				throw new ArgumentNullException("originDirectory");
			else if (string.IsNullOrEmpty(itemPath))
				throw new ArgumentNullException("originPath");

			itemPath = Path.GetFullPath(itemPath);
			originDirectory = GetFullDirectory(originDirectory);

			const int FILE_ATTRIBUTE_FILE = 0x00000000;
			const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
			StringBuilder result = new StringBuilder(260);
			if (NativeMethods.PathRelativePathTo(result, originDirectory, FILE_ATTRIBUTE_DIRECTORY, itemPath, FILE_ATTRIBUTE_FILE))
			{
				string p = result.ToString();
				if (p.Length > 2 && p[0] == '.' && (p[1] == Path.DirectorySeparatorChar || p[1] == Path.AltDirectorySeparatorChar))
					p = p.Substring(2);

				return p;
			}

			return Path.GetFullPath(itemPath);
		}		

		/// <summary>
		/// Gets the full directory name ending with a directory separator char
		/// </summary>
		/// <param name="directoryName">Name of the directory.</param>
		/// <returns></returns>
		public static string GetFullDirectory(string directoryName)
		{
			if (string.IsNullOrEmpty(directoryName))
				throw new ArgumentNullException("directoryName");

			directoryName = Path.GetFullPath(directoryName);
			if (directoryName[directoryName.Length - 1] != Path.DirectorySeparatorChar)
				directoryName += Path.DirectorySeparatorChar;

			return directoryName;
		}

		/// <summary>
		/// Gets the parent directory of a directory of file
		/// </summary>
		/// <param name="path">Name of the directory.</param>
		/// <returns></returns>
		public static string GetParentDirectory(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			if(path.Length > 0)
			{
				char c =path[path.Length - 1];
				
				if((c == Path.DirectorySeparatorChar) || (c == Path.AltDirectorySeparatorChar))
				{
					string root = Path.GetPathRoot(path);

					if(root.Length < path.Length)
						path = path.Substring(0, path.Length-1);
				}
			}

			return GetFullDirectory(Path.GetDirectoryName(path));
		}

		/// <summary>
		/// Combines the specified path parts to a complete directory
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="items">The items.</param>
		/// <returns></returns>
		public static string Combine(string path, params string[] items)
		{
			if(string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			foreach (string part in items)
			{
				if (part == null)
					continue;

				path = Path.Combine(path, part);
			}

			return path;
		}

		/// <summary>
		/// Copies the stream.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		public static void CopyStream(Stream from, Stream to)
		{
			CopyStream(from, to, 32768);
		}

		/// <summary>
		/// Copies the stream.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="bufferSize">Size of the buffer.</param>
		public static void CopyStream(Stream from, Stream to, int bufferSize)
		{
			byte[] buffer = new byte[Math.Max(512, bufferSize)];
			int nRead;

			while (0 < (nRead = from.Read(buffer, 0, buffer.Length)))
				to.Write(buffer, 0, nRead);
		}


		static readonly string dotSlash = "." + Path.DirectorySeparatorChar;
		static readonly string doubleSlash = String.Concat(Path.DirectorySeparatorChar, Path.DirectorySeparatorChar);

		/// <summary>
		/// Determines whether the specified path is a subpath.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>
		/// 	<c>true</c> if [is safe sub path] [the specified path]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsRelativeSubPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			if (Path.IsPathRooted(path))
				return false;
			else if (path.IndexOf(Path.AltDirectorySeparatorChar) >= 0 || path.Contains(dotSlash) || path.Contains(doubleSlash))
				return false;

			return true;
		}

		/// <summary>
		/// Ensures the path is a relative path from the specified origin
		/// </summary>
		/// <param name="origin">The origin.</param>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static string EnsureRelativePath(string origin, string path)
		{
			if (string.IsNullOrEmpty(origin))
				throw new ArgumentNullException("origin");
			else if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			if (IsRelativeSubPath(path))
				return path;
			else
				return MakeRelativePath(origin, Path.Combine(origin, path));
		}
	}
}
