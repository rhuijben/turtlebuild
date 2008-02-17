using System;
using System.Collections.Generic;
using System.IO;
using QQn.TurtleUtils.Tokens;
using QQn.TurtleMSBuild.ExternalProjects;
using Microsoft.Build.Framework;
using System.Xml;
using QQn.TurtleUtils.IO;

namespace QQn.TurtleMSBuild
{
	class ExternalBuildHandler
	{
		internal static void HandleProject(Solution solution)
		{
			foreach (ExternalProject ep in solution.ExternalProjects.Values)
			{
				string prefix = "Project_" + ep.ProjectGuid.ToString().ToUpperInvariant() + "_";

				foreach (ProjectItem pi in solution.BuildItems)
				{
					if (pi.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
					{
						ep.BuildItems.Add(pi);
					}
				}
			}

			foreach (ExternalProject ep in solution.ExternalProjects.Values)
			{
				ep.ParseBuildResult(solution);
				ep.PostParseBuildResult();
				ep.WriteTBLog();
			}
		}
	}
}
