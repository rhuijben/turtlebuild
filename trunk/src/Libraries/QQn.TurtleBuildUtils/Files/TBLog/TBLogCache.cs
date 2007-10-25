using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using QQn.TurtleUtils.IO;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// A cache of <see cref="TBLogFile"/> instances within a directory
	/// </summary>
	public class TBLogCache : KeyedCollection<string, TBLogFile>
	{
		string _logPath;

		/// <summary>
		/// Initializes a new instance of the <see cref="TBLogCache"/> class.
		/// </summary>
		/// <param name="logPath">The log path.</param>
		public TBLogCache(string logPath)
			: base(StringComparer.InvariantCultureIgnoreCase)
		{
			if (string.IsNullOrEmpty("logPath"))
				throw new ArgumentNullException("logPath");

			_logPath = logPath;
		}

		/// <summary>
		/// Extracts the key from the specified element.
		/// </summary>
		/// <param name="item">The element from which to extract the key.</param>
		/// <returns>The key for the specified element.</returns>
		protected override string GetKeyForItem(TBLogFile item)
		{
			return item.Project.Name;
		}

		/// <summary>
		/// Loads all log files from the log directory
		/// </summary>
		public void LoadAll()
		{
			foreach (FileInfo file in new DirectoryInfo(_logPath).GetFiles("*.tbLog", SearchOption.TopDirectoryOnly))
			{
				GC.KeepAlive(Get(Path.GetFileNameWithoutExtension(file.Name)));
			}
		}

		/// <summary>
		/// Gets log file with the specified name
		/// </summary>
		/// <param name="name">The name of the project or the name of the logfile</param>
		/// <returns></returns>
		public TBLogFile Get(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			else if(!QQnPath.IsRelativeSubPath(name))
				throw new ArgumentException("Not a valid project name", "name");

			if (Contains(name))
				return this[name];

			TBLogFile logFile = TBLogFile.Load(Path.Combine(_logPath, name + ".tbLog"));
			Add(logFile);

			return logFile;
		}
	}
}
