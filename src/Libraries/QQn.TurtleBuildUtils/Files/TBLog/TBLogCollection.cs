using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

using QQn.TurtleUtils.IO;

[module: SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "QQn.TurtleBuildUtils.Files.TBLog.TBLogCollection")]
[module: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope = "type", Target = "QQn.TurtleBuildUtils.Files.TBLog.TBLogCollection")]

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// A cache of <see cref="TBLogFile"/> instances within a directory
	/// </summary>
	public class TBLogCache : SortedFileList<TBLogFile>
	{
		string _logPath;

		/// <summary>
		/// Initializes a new instance of the <see cref="TBLogCache"/> class.
		/// </summary>
		/// <param name="logPath">The log path.</param>
		public TBLogCache(string logPath)
		{
			if (string.IsNullOrEmpty("logPath"))
				throw new ArgumentNullException("logPath");

			BaseDirectory = logPath;
			_logPath = logPath;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TBLogCache"/> class.
		/// </summary>
		public TBLogCache()
		{
		}

		/// <summary>
		/// Ensures the specified key is relative
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		protected override string EnsureRelative(string key)
		{
			return QQnPath.EnsureExtension(base.EnsureRelative(key), ".tbLog");
		}

		/// <summary>
		/// Loads all log files from the log directory
		/// </summary>
		public void LoadAll()
		{
			if (_logPath == null)
				throw new InvalidOperationException("Logpath must be set to use loadalll");

			foreach (FileInfo file in new DirectoryInfo(_logPath).GetFiles("*.tbLog", SearchOption.TopDirectoryOnly))
			{
				GC.KeepAlive(Get(file.Name));
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

			name = QQnPath.EnsureExtension(name, ".tbLog");

			if (ContainsKey(name))
				return this[name];

			if (!string.IsNullOrEmpty(_logPath))
				name = QQnPath.Combine(_logPath, name);

			TBLogFile logFile = TBLogFile.Load(name);
			Add(name, logFile);

			return logFile;
		}
	}
}
