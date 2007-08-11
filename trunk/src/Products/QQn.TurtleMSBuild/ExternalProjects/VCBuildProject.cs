using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using System.IO;
using System.Xml;

namespace QQn.TurtleMSBuild.ExternalProjects
{
	class VCBuildProject : ExternalProject
	{
		public VCBuildProject(Guid projectGuid, string projectFile, string projectName, TurtleParameters parameters)
			: base(projectGuid, projectFile, projectName, parameters)
		{
		}

		Type _resolverType;
		Type ResolverType
		{
			get
			{
				if (_resolverType == null)
				{
					Type resolverType = Type.GetType("Microsoft.Build.Tasks.ResolveVCProjectOutput, Microsoft.Build.Tasks.v3.5", false, false);
					if (resolverType == null)
						resolverType = Type.GetType("Microsoft.Build.Tasks.ResolveVCProjectOutput, Microsoft.Build.Tasks", false, false);

					_resolverType = resolverType;
				}

				return _resolverType;
			}
		}

		ITask _resolverTask;
		ITask ResolverTask
		{
			get
			{
				if (_resolverTask == null && ResolverType != null)
				{
					_resolverTask = (ITask)Activator.CreateInstance(ResolverType);
					_resolverTask.BuildEngine = new SimpleBuildEngine();
				}
				return _resolverTask;
			}
		}

		public override void ParseBuildResult(Project parentProject)
		{
			base.ParseBuildResult(parentProject);

			if (ResolverType == null || ResolverTask == null)
				return;

			ITaskItem[] output = GetProjectOutput(parentProject.ProjectFile);

			if ((output == null) || (output.Length == 0))
				return;

			string targetFile = Path.GetFullPath(output[0].ItemSpec);
			OutDir = Path.GetDirectoryName(targetFile);
			TargetName = Path.GetFileNameWithoutExtension(targetFile);
			TargetExt = Path.GetExtension(targetFile);

			ResolveToOutput(output);

			XmlTextWriter xw = new XmlTextWriter(Console.Out);
			xw.Formatting = Formatting.Indented;

			ProjectOutput.WriteProjectOutput(xw, true);
		}		
		
		private ITaskItem[] GetProjectOutput(string solutionFile)
		{
			SetTaskParameter(ResolverTask, "Configuration", Configuration);
			SetTaskParameter(ResolverTask, "SolutionFile", new SimpleTaskItem(solutionFile));
			SetTaskParameter(ResolverTask, "ProjectReferences", new ITaskItem[] { new SimpleTaskItem(ProjectFile) });

			if (!ResolverTask.Execute())
				return null;

			ITaskItem[] resolvedOutputPaths = GetTaskParameter<ITaskItem[]>(ResolverTask, "ResolvedOutputPaths");
			ITaskItem[] resolvedImportLibraryPaths = GetTaskParameter<ITaskItem[]>(ResolverTask, "ResolvedImportLibraryPaths");

			if (resolvedOutputPaths == null || resolvedOutputPaths.Length == 0)
				return null;

			List<ITaskItem> assemblies = new List<ITaskItem>();
			assemblies.AddRange(resolvedOutputPaths);
			assemblies.AddRange(resolvedImportLibraryPaths);

			return assemblies.ToArray();
		}
	}
}
