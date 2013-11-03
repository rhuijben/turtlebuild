using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;
using System.Xml;
using System.Xml.XPath;
using QQn.TurtleUtils.Tokens;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// Container of file instances in a map and its submaps
	/// </summary>
	public class DirectoryMap : IDisposable
	{
		//readonly DirectoryInfo _dirInfo;
		readonly string _directory;
		DirectoryMapData _data;

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryMap"/> class.
		/// </summary>
		/// <param name="dirInfo">The dir info.</param>
		protected DirectoryMap(DirectoryInfo dirInfo)
		{
			if (dirInfo == null)
				throw new ArgumentNullException("dirInfo");
			else if (!dirInfo.Exists)
				throw new DirectoryNotFoundException("Directory does not exist");

			//_dirInfo = dirInfo;
			_directory = dirInfo.FullName;

			string mapFile = QQnPath.Combine(dirInfo.FullName, DirectoryMapData.DirMapFile);

			DirectoryMapData mapData = null;
			if (File.Exists(mapFile))
			{
				mapData = DirectoryMapData.Load(dirInfo.FullName);
			}

			_data = mapData ?? new DirectoryMapData(dirInfo.FullName);
		}

		/// <summary>
		/// Gets the DirectoryMap for the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static DirectoryMap Get(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			DirectoryInfo dirInfo = new DirectoryInfo(path);
			if (!dirInfo.Exists)
				return null;

			return new DirectoryMap(dirInfo);
		}

		/// <summary>
		/// Checks if a DirMap exists for the specified path
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static bool Exists(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			path = Path.GetFullPath(path);

			return Directory.Exists(path) && File.Exists(QQnPath.Combine(path, ".tDirMap"));
		}

		DirectoryMapFile DoGetFile(string name)
		{
			if (_data.Files.Contains(name))
				return (DirectoryMapFile)_data.Files[name];

			DirectoryMapFile dmf = new DirectoryMapFile(name);

			_data.Files.Add(dmf);
			_data.Dirty = true;
			return dmf;
		}

		DirectoryMapDirectory DoGetDirectory(string name)
		{
			if (_data.Directories.Contains(name))
				return (DirectoryMapDirectory)_data.Files[name];

			DirectoryMapDirectory dmf = new DirectoryMapDirectory(name);

			_data.Directories.Add(dmf);
			_data.Dirty = true;
			return dmf;
		}


		/// <summary>
		/// Creates the file.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="fileMode">The file mode.</param>
		/// <param name="hash">The hash.</param>
		/// <param name="size">The size.</param>
		/// <param name="allowExternal">if set to <c>true</c> [allow external].</param>
		/// <returns></returns>
		public Stream OpenFile(string path, FileMode fileMode, string hash, long size, bool allowExternal)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			string fullPath = QQnPath.CombineFullPath(_directory, path);
			path = QQnPath.MakeRelativePath(_directory, fullPath);

			switch(fileMode)
			{
				case FileMode.CreateNew:
				case FileMode.Create:
					if (!allowExternal && File.Exists(fullPath))
						throw new IOException(string.Format(CultureInfo.InvariantCulture, "Unmanaged file {0} exists", path));
					break;
				case FileMode.Open:
				case FileMode.Truncate:
					if(!_data.Files.Contains(path) && (!allowExternal || !File.Exists(path)))
						throw new IOException("File does not exist");
					break;
				default:
					throw new ArgumentException("The specified mode is not supported");
			}

			if (size < 0)
				size = -1;

			CreateDirectory(Path.GetDirectoryName(fullPath));

			DirectoryMapFile file = DoGetFile(path);

			if (file.ToBeDeleted && (fileMode == FileMode.Create || fileMode == FileMode.CreateNew))
				file.ToBeDeleted = false;

			return new DirectoryMapStream(File.Open(fullPath, fileMode), file, fileMode, hash, string.IsNullOrEmpty(hash) ? -1 : size, _data.HashType);
		}

		/// <summary>
		/// Creates the file.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="fileMode">The file mode.</param>
		/// <returns></returns>
		public Stream OpenFile(string path, FileMode fileMode)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			return OpenFile(path, fileMode, null, -1, false);
		}

		/// <summary>
		/// Opens a file for reading
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public Stream OpenRead(string path)
		{
			return OpenFile(path, FileMode.Open);
		}

		/// <summary>
		/// Opens a file for reading
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public Stream CreateFile(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			return OpenFile(path, FileMode.Create);
		}

		/// <summary>
		/// Opens a file for reading
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="hash">The hash.</param>
		/// <param name="size">The size.</param>
		/// <returns></returns>
		public Stream CreateFile(string path, string hash, long size)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			return OpenFile(path, FileMode.Create, hash, size, false);
		}

		/// <summary>
		/// Ensures the specified directory exists
		/// </summary>
		/// <param name="directory">The directory.</param>
		public void CreateDirectory(string directory)
		{
			if(string.IsNullOrEmpty(directory))
				throw new ArgumentNullException("directory");

			directory = Path.GetFullPath(QQnPath.Combine(_directory, directory)); // Gets directory with separator

			if(!directory.StartsWith(_directory))
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Directory {0} is not below root", directory), "directory");

			if(Directory.Exists(directory))
				return;

			CreateDirectory(QQnPath.GetParentDirectory(directory));
			Directory.CreateDirectory(directory);

			DirectoryMapDirectory item = DoGetDirectory(QQnPath.GetRelativePath(directory, _directory));
			
			using(item.Updater())
			{
				item.Created = true;
			}
		}

		/// <summary>
		/// Deletes the specified file if it is within the DirectoryMap
		/// </summary>
		/// <param name="path"></param>
		public void DeleteFile(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			path = QQnPath.EnsureRelativePath(_directory, path);

			if(!_data.Files.Contains(path))
				return; // Not in DirectoryMap
			
			DirectoryMapFile file = DoGetFile(path);

			if (file.Exists)
				File.Delete(file.FullName);

			_data.Files.Remove(path);
		}

		/// <summary>
		/// Schedules the delete.
		/// </summary>
		/// <param name="path">The path.</param>
		public void ScheduleDelete(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			path = QQnPath.EnsureRelativePath(_directory, path);

			if (!_data.Files.Contains(path))
				return; // Not in DirectoryMap

			DirectoryMapFile file = DoGetFile(path);

			file.ToBeDeleted = true;
		}

		/// <summary>
		/// Unschedules the delete.
		/// </summary>
		/// <param name="path">The path.</param>
		public void UnscheduleDelete(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			path = QQnPath.EnsureRelativePath(_directory, path);

			if (!_data.Files.Contains(path))
				return; // Not in DirectoryMap

			DirectoryMapFile file = DoGetFile(path);

			file.ToBeDeleted = false;
		}

		/// <summary>
		/// Schedules a clear action after the <see cref="DirectoryMap"/> is closed
		/// </summary>
		/// <remarks>Marks all files to be deleted on dispose</remarks>
		public void ScheduleClear()
		{
			foreach (DirectoryMapFile file in _data.Files)
				file.ToBeDeleted = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Releases uresources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				List<DirectoryMapFile> toDelete = null;

				foreach(DirectoryMapFile file in _data.Files)
				{
					if(file.ToBeDeleted)
					{
						if(toDelete == null)
							toDelete = new List<DirectoryMapFile>();

						toDelete.Add(file);
					}
				}

				if (toDelete != null)
					foreach (DirectoryMapFile file in toDelete)
						DeleteFile(file.Filename);
					
				Flush();
			}
		}

		/// <summary>
		/// Flushes this instance.
		/// </summary>
		public void Flush()
		{
			if (_data.Dirty)
			{
				_data.Dirty = false;
				_data.Write();
			}
		}

		/// <summary>
		/// Gets the file.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="addExistingFile">if set to <c>true</c> [add existing file].</param>
		/// <returns></returns>
		public DirectoryMapFile GetFile(string path, bool addExistingFile)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");
			
			string fullPath = QQnPath.Combine(_directory, path);
			path = QQnPath.MakeRelativePath(_directory, fullPath);

			if (_data.Files.Contains(path))
				return DoGetFile(path);
			else if (addExistingFile && File.Exists(fullPath))
				return AddFile(path);
			else
				return null;	
		}

		/// <summary>
		/// Gets the file.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public DirectoryMapFile GetFile(string path)
		{
			return GetFile(path, false);
		}

		/// <summary>
		/// Adds the file.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public DirectoryMapFile AddFile(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			string fullPath = QQnPath.Combine(_directory, path);
			path = QQnPath.MakeRelativePath(_directory, fullPath);

			if (_data.Files.Contains(path) || File.Exists(fullPath))
			{
				DirectoryMapFile dmf = DoGetFile(path);
				dmf.ToBeDeleted = false;

				return dmf;
			}
			else
				throw new FileNotFoundException("File not found", fullPath);
		}


		/// <summary>
		/// Gets the full name of the root of the <see cref="DirectoryMap"/>
		/// </summary>
		/// <value>The full name.</value>
		public string FullName
		{
			get { return _directory; }
		}

        /// <summary>
        /// Adds an annotation to <paramref name="path"/>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Annotate(string path, string key, string value)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            else if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            DirectoryMapItem item = GetFile(path);

            if (item == null)
                return; // TODO: Add directory support?

            if (item.Annotations.Contains(key))
            {
                if (value != null)
                    item.Annotations[key].Value = value;
                else
                    item.Annotations.Remove(key);
            }
            else if(value != null)
                item.Annotations.Add(new DirectoryMapAnnotation(key, value));
        }

        /// <summary>
        /// Gets the annotation.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAnnotation(string path, string key)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            else if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            DirectoryMapItem item = GetFile(path);

            if (item == null)
                return null; // TODO: Add directory support?

            if (item.Annotations.Contains(key))
                return item.Annotations[key].Value;
            else
                return null;
        }
	}
}
