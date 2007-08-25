using System;
using System.Collections.Generic;
using System.Text;
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
	public class PackUtils
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

			

			if(!string.IsNullOrEmpty(log.Project.OutputDir))
			{
				projectOutput.ContainerDir = log.Project.OutputDir;
				projectOutput.BaseDir = log.Project.OutputDir;
			}

			foreach (TBLogItem item in log.ProjectOutput.Items)
			{
				if (item.IsShared)
					continue;

				PackFile pf = projectOutput.Files.AddItem(QQnPath.MakeRelativePath(projectOutput.BaseDir, Path.Combine(projectDir, item.Src)));
			}

			PackContainer projectContent = p.Containers.AddItem("#ProjectContent");

			if (!string.IsNullOrEmpty(log.Project.OutputDir))
			{
				projectContent.ContainerDir = "content/" + log.Project.Name;
				projectContent.BaseDir = log.ProjectPath;
			}

			foreach (TBLogItem item in log.Content.Items)
			{
				PackFile pf = projectContent.Files.AddItem(QQnPath.MakeRelativePath(projectContent.BaseDir, Path.Combine(projectDir,item.Src)));
			}

			PackContainer projectScripts = p.Containers.AddItem("#ProjectScripts");

			if (!string.IsNullOrEmpty(log.Project.OutputDir))
			{
				projectScripts.ContainerDir = "scripts/" + log.Project.Name;
				projectScripts.BaseDir = log.Project.Path;
			}

			foreach (TBLogItem item in log.Content.Items)
			{
				PackFile pf = projectContent.Files.AddItem(QQnPath.MakeRelativePath(projectContent.BaseDir, Path.Combine(projectDir, item.Src)));
			}

			if (log.Project.KeyFile != null)
				p.StrongNameKey = StrongNameKey.LoadFrom(Path.Combine(log.Project.Path, log.Project.KeyFile));
			else if (log.Project.KeyContainer != null)
				p.StrongNameKey = StrongNameKey.LoadFromContainer(log.Project.KeyContainer, false);


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
