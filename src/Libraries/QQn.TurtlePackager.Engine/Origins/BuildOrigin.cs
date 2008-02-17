using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleBuildUtils.Files.TBLog;
using System.IO;
using QQn.TurtleUtils.IO;
using System.Diagnostics;

namespace QQn.TurtlePackager.Origins
{
	[DebuggerDisplay("Type=MSBuild Project={ProjectName}")]
	class BuildOrigin : Origin
	{
		readonly TBLogFile _log;
		readonly string _projectFile;

		public BuildOrigin(TBLogFile logFile)
		{
			if (logFile == null)
				throw new ArgumentNullException("logFile");

			_log = logFile;
			_projectFile = QQnPath.Combine(logFile.ProjectPath, logFile.Project.File);
		}

		public TBLogFile LogFile
		{
			get { return _log; }
		}

		public string ProjectName
		{
			get { return _log.Project.Name; }
		}

		public override string ToString()
		{
			return "BuildOrigin: " + ProjectName;
		}

		public override void PublishOriginalFiles(PackageState state)
		{
			foreach (TBLogItem item in LogFile.AllProjectOutput)
			{
				if (item.IsShared)
					continue;

				FileData fd = new FileData(item.FullSrc, state.Files);

				if (!string.IsNullOrEmpty(item.FromSrc))
					fd.CopiedFrom = item.FullFromSrc;

				fd.Origin = this;

				state.Files.Add(fd);
			}

			foreach (TBLogItem item in LogFile.AllContents)
			{
				if (item.IsShared)
					continue;

				FileData fd = new FileData(item.FullSrc, state.Files);

				if (!string.IsNullOrEmpty(item.FromSrc))
					fd.CopiedFrom = item.FullFromSrc;

				fd.Origin = this;

				state.Files.AddUnique(fd);
			}
		}

		public override void PublishRequiredFiles(PackageState state)
		{
			foreach (TBLogItem item in LogFile.AllPublishItems)
			{
				if (!item.IsShared)
					continue;

				FileData fd = new FileData(item.FullSrc, state.Files);

				if (!string.IsNullOrEmpty(item.FromSrc))
					fd.CopiedFrom = item.FullFromSrc;

				fd.FindOrigin = true;
				fd.Origin = this;

				state.Files.Add(fd); // Output files must be unique
			}
		}

		public override void ApplyProjectDependencies(PackageState state)
		{
			if (!state.DontUseProjectDependencies) // Allow disabling for testing
			{
				// Add an initial set of dependencies directly from the project files
				foreach (TBLogConfiguration config in LogFile.Configurations)
				{
					foreach (TBLogItem project in config.References.Projects)
					{
						string src = QQnPath.NormalizePath(project.FullSrc);

						foreach (Origin o in state.Origins)
						{
							BuildOrigin bo = o as BuildOrigin;
							if (bo == null)
								continue;

							if (QQnPath.Equals(bo.ProjectFile, src) && !Dependencies.ContainsKey(o))
								EnsureDependency(o, DependencyType.LinkedTo);
						}
					}
				}
			}

			foreach (TBLogItem item in LogFile.AllProjectOutput)
			{
				FileData fd = state.Files[item.FullSrc];

				if (!string.IsNullOrEmpty(fd.CopiedFrom))
				{
					FileData src;

					if (state.Files.TryGetValue(fd.CopiedFrom, out src))
					{
						if (src.Origin != this)
						{
							EnsureDependency(src.Origin, item.IsCopy ? DependencyType.Required : DependencyType.LinkedTo);
						}
					}
				}
			}

			foreach (TBLogItem item in LogFile.AllContents)
			{
				FileData fd = state.Files[item.FullSrc];

				if (!string.IsNullOrEmpty(fd.CopiedFrom))
				{
					FileData src;

					if (state.Files.TryGetValue(fd.CopiedFrom, out src))
					{
						if (src.Origin != this)
						{
							EnsureDependency(src.Origin, DependencyType.Required);
						}
					}
				}
			}
		}

		private void EnsureDependency(Origin origin, DependencyType dtSet)
		{
			DependencyType dt;
			if (Dependencies.TryGetValue(origin, out dt))
			{
				if (dtSet >= dt)
					return;
			}

			Dependencies[origin] = dtSet;
		}

		public string ProjectFile
		{
			get { return _projectFile; }
		}


	}
}
