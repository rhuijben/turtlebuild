using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using QQn.TurtleBuildUtils;
using QQn.TurtleUtils.IO;
using QQn.TurtleUtils.Tags;

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

		public override string FullProjectConfiguration
		{
			get { return ProjectConfiguration + '|' + ProjectPlatform; }
			set
			{
				string[] parts = value.Split('|');

				ProjectConfiguration = parts[0];
				if (parts.Length > 1)
					ProjectPlatform = parts[1];
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

		List<string> _configs = new List<string>();
		protected internal override void AddBuildConfiguration(string configuration)
		{
			_configs.Add(configuration);
			base.AddBuildConfiguration(configuration);
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

			ProjectOutput.BaseDirectory = ProjectPath;

			ResolveToOutput(output);

			ParseProjectFile();
		}

		public override void PostParseBuildResult()
		{
			base.PostParseBuildResult();

			if (Parameters.UpdateVCVersionInfo)
			{
				string keyFile = KeyFile;

				if (!string.IsNullOrEmpty(keyFile))
					keyFile = QQnPath.Combine(ProjectPath, keyFile);

				string targetFile = QQnPath.Combine(ProjectPath, TargetPath);

				if (!AssemblyUtils.RefreshVersionInfoFromAttributes(targetFile, keyFile, KeyContainer))
					throw new InvalidOperationException("Failed to refresh attributes");
			}
		}

		TagPropertyCollection _props;

		public TagPropertyCollection Properties
		{
			get { return _props ?? (_props = CreatePropertyCollection()); }
		}

		private TagPropertyCollection CreatePropertyCollection()
		{
			TagPropertyCollection tpc = new TagPropertyCollection();
			tpc.LoadEnvironmentVariables();

			tpc.Set("ConfigurationName", ProjectConfiguration);
			tpc.Set("PlatformName", ProjectPlatform);

			// tpc.Set("IntDir", QQnPath.NormalizePath(..., true));
			// tpc.Set("OutDir", QQnPath.NormalizePath(..., true));
			tpc.Set("ProjectDir", QQnPath.NormalizePath(Path.GetDirectoryName(ProjectFile)));
			tpc.Set("ProjectPath", QQnPath.NormalizePath(ProjectFile));
			tpc.Set("ProjectName", ProjectName);
			tpc.Set("ProjectFileName", Path.GetFileName(ProjectFile));
			tpc.Set("ProjectExt", Path.GetExtension(ProjectFile));
			// tpc.Set("SolutionDir", QQnPath.NormalizePath(..., true));
			// tpc.Set("SolutionPath", QQnPath.NormalizePath(...));
			// tpc.Set("SolutionName", ...);
			// tpc.Set("SolutionFileName", QQnPath.NormalizePath(...));

			// tpc.Set("TargetDir", QQnPath.NormalizePath(..., true));
			// tpc.Set("TargetPath", QQnPath.NormalizePath(...));
			// tpc.Set("TargetName", ...);
			// tpc.Set("TargetFileName", QQnPath.NormalizePath(...));
			// tpc.Set("TargetExt", QQnPath.NormalizePath(...));

			return tpc;
		}

		string ExpandProperties(string value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			if (value.Contains("$("))
				return Properties.ExpandProperties(value);

			return value;
		}

		private void ParseProjectFile()
		{
			using (StreamReader sr = File.OpenText(ProjectFile))
			{
				XPathDocument doc = new XPathDocument(sr);

				SortedList<string, string> extensions = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);

				if (Parameters.ScriptExtensions != null)
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

				XPathNavigator n = doc.CreateNavigator().SelectSingleNode("//Tool[ancestor::Configuration[@Name='" + FullProjectConfiguration + "'] and @Name='VCLinkerTool' and @KeyFile!='']");

				if (n != null)
				{
					KeyFile = EnsureRelativePath(ExpandProperties(n.GetAttribute("KeyFile", "")));
				}

				n = doc.CreateNavigator().SelectSingleNode("//Tool[ancestor::Configuration[@Name='" + FullProjectConfiguration + "'] and @Name='VCLinkerTool' and @KeyContainer!='']");

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
