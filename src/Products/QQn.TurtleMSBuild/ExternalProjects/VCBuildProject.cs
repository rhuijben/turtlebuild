using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using QQn.TurtleBuildUtils;

namespace QQn.TurtleMSBuild.ExternalProjects
{
	class VCBuildProject : ExternalProject
	{
		public VCBuildProject(Guid projectGuid, string projectFile, string projectName, TurtleParameters parameters)
			: base(projectGuid, projectFile, projectName, parameters)
		{
			ProjectType = "VCProj";
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
			OutputPath = Path.GetDirectoryName(targetFile);
			TargetName = Path.GetFileNameWithoutExtension(targetFile);
			TargetExt = Path.GetExtension(targetFile);

			ResolveToOutput(output);

			ParseProjectFile();

			if (Parameters.UpdateVCVersionInfo)
			{
                string keyFile = KeyFile;

                if(!string.IsNullOrEmpty(keyFile))
                    keyFile = Path.Combine(ProjectPath, keyFile);

				AssemblyUtils.RefreshVersionInfoFromAttributes(targetFile, keyFile, KeyContainer);
			}
		}		

		private void ParseProjectFile()
		{
			using (StreamReader sr = File.OpenText(ProjectFile))
			{
				XPathDocument doc = new XPathDocument(sr);

				SortedList<string, string> extensions = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);

				if(Parameters.ScriptExtensions != null)
				foreach (string extension in Parameters.ScriptExtensions)
				{
					string ext = extension;

					if (!ext.StartsWith("."))
						ext = '.' + ext;

					if (!extensions.ContainsKey(ext))
						extensions.Add(ext, "Item");
				}

				foreach (XPathNavigator nav in doc.CreateNavigator().Select("//File[@RelativePath]"))
				{
					string file = nav.GetAttribute("RelativePath", "");
					bool deploymentContent = ("true" == nav.GetAttribute("DeploymentContent", ""));

					if (deploymentContent)
						ContentFiles.AddUnique(file);

					if (extensions.ContainsKey(Path.GetExtension(file)))
						ScriptFiles.AddUnique(file);
				}

				XPathNavigator n = doc.CreateNavigator().SelectSingleNode("//Tool[ancestor::Configuration[@Name='" + ProjectConfiguration + "'] and @Name='VCLinkerTool' and @KeyFile!='']");

				if(n != null)
				{
					KeyFile = EnsureRelativePath(n.GetAttribute("KeyFile", ""));
				}

				n = doc.CreateNavigator().SelectSingleNode("//Tool[ancestor::Configuration[@Name='" + ProjectConfiguration + "'] and @Name='VCLinkerTool' and @KeyContainer!='']");

				if (n != null)
				{
					KeyContainer = n.GetAttribute("KeyContainer", "");
				}

				// TODO: Find processor architecture				
			}
		}		
		
		private ITaskItem[] GetProjectOutput(string solutionFile)
		{
			SetTaskParameter(ResolverTask, "Configuration", ProjectConfiguration);
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
