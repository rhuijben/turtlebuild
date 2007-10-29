using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Reflection;

namespace QQn.TurtleUtils.IO
{
	static class NativeMethods
	{
		[DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
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

			if (!string.Equals(pathRoot, relRoot, StringComparison.OrdinalIgnoreCase))
				return path;

			const int FILE_ATTRIBUTE_FILE = 0x00000000;
			const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
			StringBuilder result = new StringBuilder(260); // 260 = MAX_PATH
			if (NativeMethods.PathRelativePathTo(result, relativeFrom, FILE_ATTRIBUTE_DIRECTORY, path, FILE_ATTRIBUTE_FILE))
			{
				string p = result.ToString();
				if (p.Length > 2 && p[0] == '.' && (p[1] == Path.DirectorySeparatorChar))
					p = p.Substring(2);

				return p;
			}

			return path;
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
				throw new ArgumentNullException("itemPath");

			itemPath = Path.GetFullPath(itemPath);
			originDirectory = Path.GetFullPath(originDirectory);

			if (originDirectory.Length > 3 && originDirectory[originDirectory.Length - 1] == Path.DirectorySeparatorChar)
				originDirectory = originDirectory.Substring(0, originDirectory.Length - 1);

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

			return Path.GetFullPath(Path.GetDirectoryName(path));
		}

		/// <summary>
		/// Combines the specified path parts to a complete directory
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="items">The items.</param>
		/// <returns></returns>
		public static string Combine(string path, params string[] items)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");
			else if (items == null)
				throw new ArgumentNullException("items");

			foreach (string part in items)
			{
				if (part == null)
					continue;

				path = Path.Combine(path, part);
			}

			return path;
		}

		/// <summary>
		/// Combines the specified path parts to a full path
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="items">The items.</param>
		/// <returns></returns>
		public static string CombineFullPath(string path, params string[] items)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");
			else if(items == null)
				throw new ArgumentNullException("items");


			foreach (string part in items)
			{
				if (part == null)
					continue;

				path = Path.Combine(path, part);
			}

			return Path.GetFullPath(path);
		}

		/// <summary>
		/// Combines the specified path parts to a full path
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="path2">The path2.</param>
		/// <returns></returns>
		public static string CombineFullPath(string path, string path2)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			return Path.GetFullPath(Path.Combine(path, path2));
		}

		/// <summary>
		/// Normalizes the path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="addEndSlash">if set to <c>true</c> adds a path separator at the end.</param>
		/// <returns></returns>
		public static string NormalizePath(string path, bool addEndSlash)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			if (Path.IsPathRooted(path))
			{
				path = Path.GetFullPath(path);
				string root = Path.GetPathRoot(path);

				path = root + RemoveDoubleSlash(path.Substring(root.Length), addEndSlash);
			}
			else
				path = RemoveDoubleSlash(path, addEndSlash);

			return path;
		}

		/// <summary>
		/// Normalizes the path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static string NormalizePath(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			return NormalizePath(path, false);
		}

		/// <summary>
		/// Removes the double slash.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="addEndSlash">if set to <c>true</c> adds a path separator at the end.</param>
		/// <returns></returns>
		static string RemoveDoubleSlash(string path, bool addEndSlash)
		{
			string doubleSlash = string.Concat(Path.DirectorySeparatorChar, Path.DirectorySeparatorChar);
			int n;
			while(0 <= (n = path.IndexOf(doubleSlash)))
				path = path.Remove(n,1); // Remove the first slash

			if (addEndSlash != (path[path.Length - 1] == Path.DirectorySeparatorChar))
			{
				if (addEndSlash)
					path += Path.DirectorySeparatorChar;
				else
					path = path.Substring(0, path.Length - 1);
			}

			return path;				
		}

		/// <summary>
		/// Normalizes the unix path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="addEndSlash">if set to <c>true</c> adds a path separator at the end.</param>
		/// <returns></returns>
		public static string NormalizeUnixPath(string path, bool addEndSlash)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			return NormalizePath(path, addEndSlash).Replace(Path.DirectorySeparatorChar, '/');
		}

		/// <summary>
		/// Normalizes the unix path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static string NormalizeUnixPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			return NormalizeUnixPath(path, false);
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
			if (from == null)
				throw new ArgumentNullException("from");
			else if (to == null)
				throw new ArgumentNullException("to");
			else if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "Buffersize must be greater than 0");
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
		/// <returns>The unmodified path, or the path as relative from the specified origin</returns>
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

		/// <summary>
		/// Finds the specified file in the provided path.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="pathList">The path list, separated by <see cref="Path.PathSeparator"/> characters</param>
		/// <returns></returns>
		public static string FindFileInPath(string file, string pathList)
		{
			if (string.IsNullOrEmpty(file))
				throw new ArgumentNullException("file");
			else if (string.IsNullOrEmpty(pathList))
				throw new ArgumentNullException("pathList");

			string[] paths = pathList.Split(Path.PathSeparator);

			foreach (string i in paths)
			{
				if (string.IsNullOrEmpty(i))
					continue;

				string fullPath = CombineFullPath(i, file);

				if (File.Exists(fullPath))
					return fullPath;
			}

			return null;
		}

		/// <summary>
		/// Finds the file in the system environment variable path, the current directory, or the directory containing the current application.
		/// </summary>
		/// <param name="file">the filename of the file to search</param>
		/// <returns>The full path of the file or <c>null</c> if the file is not found</returns>
		public static string FindFileInPath(string file)
		{
			if (string.IsNullOrEmpty(file))
				throw new ArgumentNullException("file");

			string path = Environment.GetEnvironmentVariable("PATH");

			string result;
			if (!string.IsNullOrEmpty(path))
			{
				result = FindFileInPath(file, path);

				if (!string.IsNullOrEmpty(result))
					return result;
			}
				
			result = FindFileInPath(file, ".");

			if (!string.IsNullOrEmpty(result))
				return result;

			Assembly asm = Assembly.GetEntryAssembly();

			if (asm == null)
				asm = Assembly.GetCallingAssembly();

			if(asm != null)
				result = FindFileNextToAssembly(file, asm);

			return result;
		}

		private static string FindFileNextToAssembly(string file, Assembly assembly)
		{
			if (string.IsNullOrEmpty(file))
				throw new ArgumentNullException("file");
			else if (assembly == null)
				throw new ArgumentNullException("assembly");

			if (assembly.CodeBase == null)
				return null;

			Uri uri = new Uri(assembly.CodeBase);

			if (uri.IsFile || uri.IsUnc)
				return FindFileInPath(file, Path.GetDirectoryName(uri.LocalPath));

			return null;
		}

		/// <summary>
		/// Gets a boolean indicating whether a filename might be of an assembly
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <returns>true for .exe and .dll files</returns>
		public static bool IsAssemblyFile(string filename)
		{
			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			string extension = Path.GetExtension(filename);

			if (string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Replaces the extension.
		/// </summary>
		/// <param name="targetFile">The target file.</param>
		/// <param name="newExtension">The new extension.</param>
		/// <returns></returns>
		public static string ReplaceExtension(string targetFile, string newExtension)
		{
			if (string.IsNullOrEmpty(targetFile))
				throw new ArgumentNullException("targetFile");
			else if (string.IsNullOrEmpty("newExtension"))
				throw new ArgumentNullException("newExtension");

			targetFile = NormalizePath(targetFile);

			if (newExtension[0] != '.')
				newExtension = '.' + newExtension;

			return Path.Combine(Path.GetDirectoryName(targetFile), Path.GetFileNameWithoutExtension(targetFile) + newExtension);
		}
	}
}
