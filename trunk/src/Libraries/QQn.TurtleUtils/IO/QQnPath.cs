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
	{/// <summary>
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
	}
}
