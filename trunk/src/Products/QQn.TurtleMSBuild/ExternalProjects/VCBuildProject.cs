using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using QQn.TurtleBuildUtils;
using QQn.TurtleUtils.IO;
using QQn.TurtleUtils.Tags;
using System.Globalization;
using System.Reflection;

namespace QQn.TurtleMSBuild.ExternalProjects
{
	class VCBuildProject : ExternalProject
	{
		public VCBuildProject(Guid projectGuid, string projectFile, string projectName, TurtleParameters parameters)
			: base(projectGuid, projectFile, projectName, parameters)
		{
			ProjectType = "VCProj";
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

		List<string> _configs = new List<string>();
		protected internal override void AddBuildConfiguration(string configuration)
		{
			_configs.Add(configuration);
			base.AddBuildConfiguration(configuration);
		}

		public override void ParseBuildResult(Project parentProject)
		{
			base.ParseBuildResult(parentProject);

			ParseProjectFile(FullProjectConfiguration, parentProject);

			if (string.IsNullOrEmpty(TargetPath))
				return;

			if (IsAssembly)
			{
				Assembly asm = Assembly.ReflectionOnlyLoadFrom(GetFullPath(TargetPath));

				if (Parameters.UpdateVCVersionInfo)
				{
					string keyFile = KeyFile;

					if (!string.IsNullOrEmpty(keyFile))
						keyFile = GetFullPath(keyFile);

					// Make sure there are no relative paths remaining
					string targetFile = GetFullPath(TargetPath);

					if (!AssemblyUtils.RefreshVersionInfoFromAttributes(asm, keyFile, KeyContainer))
						Console.WriteLine("Refreshing attributes on {0} failed", targetFile);
				}

				TargetAssembly = new AssemblyReference(asm.FullName, ProjectOutput[TargetPath]);

				foreach (Module m in asm.GetModules(true))
				{
					string file = EnsureRelativePath(m.FullyQualifiedName);

					if (!ProjectOutput.Contains(file))
						ProjectOutput.Add(new TargetItem(file, file, TargetType.Item));
				}

				ResolveAdditionalOutput(asm);
			}
		}

		public override void PostParseBuildResult()
		{
			base.PostParseBuildResult();
		}

		string _targetPath;

		public override string TargetPath
		{
			get
			{
				return _targetPath ?? base.TargetPath;
			}
			internal set
			{
				_targetPath = value;
			}

		}

		bool _isAssembly;
		public bool IsAssembly
		{
			get { return _isAssembly; }
			internal set { _isAssembly = value; }
		}

		private void ParseProjectFile(string fullConfiguration, Project parentProject)
		{
			TagPropertyCollection tpc = new TagPropertyCollection();

			string[] items = fullConfiguration.Split('|');
			ProjectConfiguration = items[0];
			ProjectPlatform = items[1];

			tpc.Set("PlatformName", ProjectPlatform);
			tpc.Set("ConfigurationName", ProjectConfiguration);

			Solution solution = parentProject as Solution;
			if(solution != null)
			{
				tpc.Set("SolutionDir", QQnPath.NormalizePath(solution.ProjectPath, true));
				tpc.Set("SolutionPath", QQnPath.NormalizePath(solution.ProjectFile));
				tpc.Set("SolutionName", solution.ProjectName);
				tpc.Set("SolutionFileName", Path.GetFileName(ProjectFile));
			}

			tpc.Set("ProjectDir", QQnPath.NormalizePath(ProjectPath, true));
			tpc.Set("ProjectPath", QQnPath.NormalizePath(ProjectFile));
			tpc.Set("ProjectName", ProjectName);
			tpc.Set("ProjectFileName", Path.GetFileName(ProjectFile));
			tpc.Set("ProjectExt", Path.GetExtension(ProjectFile));			

			using (StreamReader sr = File.OpenText(ProjectFile))
			{
				XPathDocument doc = new XPathDocument(sr);

				XPathNavigator dn = doc.CreateNavigator();

				TargetName = dn.SelectSingleNode("/VisualStudioProject").GetAttribute("Name", "");
				tpc.Set("TargetName", TargetName);
				tpc.Set("ProjectName", TargetName);

				XPathNavigator config = dn.SelectSingleNode("//Configurations/Configuration[@Name='" + FullProjectConfiguration + "']");
				XPathNavigator compiler = config.SelectSingleNode("Tool[@Name='VCCLCompilerTool']");
				XPathNavigator linker = config.SelectSingleNode("Tool[@Name='VCLinkerTool']");

				if (string.IsNullOrEmpty(TargetName) || config == null || compiler == null)
				{
					TargetName = null;
					return; // No .Net assembly output
				}

				OutputPath = tpc.ExpandProperties(config.GetAttribute("OutputDirectory", ""));
				tpc.Set("OutDir", QQnPath.NormalizePath(GetFullPath(OutputPath), true));

				string intPath = tpc.ExpandProperties(config.GetAttribute("IntermediateDirectory", ""));
				intPath = GetFullPath(intPath);
				tpc.Set("IntDir", QQnPath.NormalizePath(intPath, true));

				ProjectOutput.BaseDirectory = ProjectPath;

				switch (int.Parse(config.GetAttribute("ConfigurationType", "").Trim(), NumberStyles.None))
				{
					case 1:
						TargetExt = ".exe";
						break;
					case 4:
						TargetExt = ".lib";
						linker = config.SelectSingleNode("Tool[@Name='VCLibrarianTool']");
						break;
					case 2:
					default:
						TargetExt = ".dll";
						break;	
				}

				tpc.Set("TargetExt", TargetExt);

				string tp = linker.GetAttribute("OutputFile", "");

				if (!string.IsNullOrEmpty(tp))
				{
					tp = tpc.ExpandProperties(tp);

					if (!string.IsNullOrEmpty(tp))
					{
						tp = EnsureRelativePath(tp);

						TargetName = Path.GetFileNameWithoutExtension(tp);
						TargetExt = Path.GetExtension(tp);
						tpc.Set("TargetExt", TargetExt);

						if(!string.Equals(TargetPath, tp, StringComparison.OrdinalIgnoreCase))
							TargetPath = tp; // Only set the override if the construction was not ok
					}
				}

				if (!File.Exists(GetFullPath(TargetPath)) || linker == null)
				{
					TargetName = null;
					return; // Something went wrong.. Check project format
				}

				string mc = config.GetAttribute("ManagedExtensions", "");
				if (!string.IsNullOrEmpty(mc))
				{
					switch (int.Parse(mc.Trim(), NumberStyles.None))
					{
						case 0:
							break; // No clr?
						case 1:
						default:
							IsAssembly = QQnPath.IsAssemblyFile(TargetPath);
							break;
					}
				}

				string value = TargetPath;
				ProjectOutput.Add(new TargetItem(value, value, TargetType.Item));

				if (string.Equals(compiler.GetAttribute("GenerateXMLDocumentationFiles", ""), "true", StringComparison.OrdinalIgnoreCase))
				{
					string xmlFile = Path.ChangeExtension(value, ".xml");

					if (File.Exists(GetFullPath(xmlFile)))
						ProjectOutput.Add(new TargetItem(xmlFile, xmlFile, TargetType.Item));
				}

				if(string.Equals(linker.GetAttribute("GenerateDebugInformation", ""), "true", StringComparison.OrdinalIgnoreCase))
				{
					string pdbFile = Path.ChangeExtension(value, ".pdb");

					if (File.Exists(GetFullPath(pdbFile)))
						ProjectOutput.Add(new TargetItem(pdbFile, pdbFile, TargetType.Item));
				}				

				if (!string.IsNullOrEmpty(value = linker.GetAttribute("KeyFile", "")))
				{
					KeyFile = EnsureRelativePath(tpc.ExpandProperties(value));
				}

				if (!string.IsNullOrEmpty(value = linker.GetAttribute("KeyContainer", "")))
				{
					KeyContainer = value;
				}

				FindContentAndScripts(doc);
			}
		}

		private void FindContentAndScripts(XPathDocument doc)
		{
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
		}
	}
}
