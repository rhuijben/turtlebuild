using System;
using System.Collections.Generic;
using QQn.TurtleBuildUtils.Files.TBLog;
using QQn.TurtleUtils.IO;
using QQn.TurtleUtils.Cryptography;
using System.IO;
using System.Diagnostics;

namespace QQn.TurtlePackage
{
	/// <summary>
	/// 
	/// </summary>
	public static class PackUtils
	{
		/// <summary>
		/// Tries the create pack.
		/// </summary>
		/// <param name="project">The project.</param>
		/// <param name="pack">The pack.</param>
		/// <returns></returns>
		public static bool TryCreatePack(TBLogFile project, out Pack pack)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			TBLogFile log = project;

			string projectDir = project.Project.Path;

			Pack p = new Pack();
			p.BaseDir = projectDir;

			PackContainer projectOutput = p.Containers.AddItem("#ProjectOutput");

			TBLogConfiguration config = log.Configurations[0];
			

			if(!string.IsNullOrEmpty(config.OutputPath))
			{
				projectOutput.ContainerDir = config.OutputPath;
				projectOutput.BaseDir = config.OutputPath;
			}

			foreach (TBLogItem item in config.ProjectOutput.Items)
			{
				if (item.IsShared)
					continue;

				PackFile pf = projectOutput.Files.AddItem(QQnPath.MakeRelativePath(projectOutput.BaseDir, Path.Combine(projectDir, item.Src)));
			}

			PackContainer projectContent = p.Containers.AddItem("#ProjectContent");

			if (!string.IsNullOrEmpty(config.OutputPath))
			{
				projectContent.ContainerDir = "content/" + log.Project.Name;
				projectContent.BaseDir = log.ProjectPath;
			}

			foreach (TBLogItem item in config.Content.Items)
			{
				PackFile pf = projectContent.Files.AddItem(QQnPath.MakeRelativePath(projectContent.BaseDir, Path.Combine(projectDir,item.Src)));
			}

			PackContainer projectScripts = p.Containers.AddItem("#ProjectScripts");

			if (!string.IsNullOrEmpty(config.OutputPath))
			{
				projectScripts.ContainerDir = "scripts/" + log.Project.Name;
				projectScripts.BaseDir = log.Project.Path;
			}

			foreach (TBLogItem item in config.Content.Items)
			{
				PackFile pf = projectContent.Files.AddItem(QQnPath.MakeRelativePath(projectContent.BaseDir, Path.Combine(projectDir, item.Src)));
			}

			if (config.Target.KeySrc != null)
				p.StrongNameKey = StrongNameKey.LoadFrom(Path.Combine(log.Project.Path, config.Target.KeySrc));
			else if (config.Target.KeyContainer != null)
				p.StrongNameKey = StrongNameKey.LoadFromContainer(config.Target.KeyContainer, false);


			foreach (PackContainer pc in p.Containers)
			{
				foreach (PackFile pf in pc.Files)
				{
					VerifyUtils.UpdateFile(pf.BaseDir, pf);
				}
			}

			pack = p;

			return true;
		}
	}
}
