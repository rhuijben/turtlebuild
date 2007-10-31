using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleBuildUtils.Files.TBLog;
using System.Collections.ObjectModel;
using QQn.TurtlePackager.Origins;

namespace QQn.TurtlePackager
{
	class PackageState
	{
		readonly TBLogCache _logCache;
		readonly Collection<TBLogFile> _logFiles = new Collection<TBLogFile>();
		readonly Collection<Origin> _origins = new Collection<Origin>();
		readonly FileDataList _files = new FileDataList();
		readonly string _buildRoot;
		readonly bool _dontUseProjectDependencies;

		public PackageState(PackageArgs args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			_logCache = args.LogCache;

			foreach (string file in args.ProjectsToPackage.KeysAsFullPaths)
			{
				Logs.Add(LogCache.Get(file));
			}

			_buildRoot = args.BuildRoot;
			_dontUseProjectDependencies = !args.UseProjectDependencies;

			if (!string.IsNullOrEmpty(BuildRoot))
			{
				Files.BaseDirectory = args.BuildRoot;
			}
		}

		public string BuildRoot
		{
			get { return _buildRoot; }
		}

		public TBLogCache LogCache
		{
			get { return _logCache; }
		}

		public Collection<TBLogFile> Logs
		{
			get { return _logFiles; }
		}

		public Collection<Origin> Origins
		{
			get { return _origins; }
		}

		public FileDataList Files
		{
			get { return _files; }
		}

		public bool DontUseProjectDependencies
		{
			get { return _dontUseProjectDependencies; }
		} 

		public void CreateBuildOrigins()
		{
			foreach (TBLogFile log in Logs)
			{
				Origins.Add(new BuildOrigin(log));
			}

			foreach (Origin o in Origins)
			{
				o.PublishOriginalFiles(this);
			}
		}

		public void AddRequirements()
		{
			foreach (Origin o in Origins)
			{
				o.PublishRequiredFiles(this);
			}
		}

		public void CalculateDependencies()
		{
			foreach (Origin o in Origins)
			{
				o.ApplyProjectDependencies(this);
			}
		}
	}
}
