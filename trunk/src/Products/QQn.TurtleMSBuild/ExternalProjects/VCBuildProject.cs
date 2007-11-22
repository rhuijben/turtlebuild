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

			if (IsAssembly && Parameters.UpdateVCVersionInfo)
			{
				string keyFile = KeyFile;

				if (!string.IsNullOrEmpty(keyFile))
					keyFile = QQnPath.Combine(ProjectPath, keyFile);

				// Make sure there are no relative paths remaining
				string targetFile = QQnPath.NormalizePath(QQnPath.Combine(ProjectPath, TargetPath));

				if (!AssemblyUtils.RefreshVersionInfoFromAttributes(targetFile, keyFile, KeyContainer))
					Console.WriteLine("Refreshing attributes on {0} failed", targetFile);
			}

			ResolveAdditionalOutput();
		}

		public override void PostParseBuildResult()
		{
			base.PostParseBuildResult();
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
			
			

			// tpc.Set("TargetDir", QQnPath.NormalizePath(..., true));
			// tpc.Set("TargetPath", QQnPath.NormalizePath(...));
			// tpc.Set("TargetName", ...);
			// tpc.Set("TargetFileName", QQnPath.NormalizePath(...));
			// tpc.Set("TargetExt", QQnPath.NormalizePath(...));

			return tpc;
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

				XPathNavigator config = dn.SelectSingleNode("//Configurations/Configuration[@Name='" + FullProjectConfiguration + "']");
				XPathNavigator linker = config.SelectSingleNode("Tool[@Name='VCLinkerTool']");

				if (string.IsNullOrEmpty(TargetName) || config == null || linker == null)
					return; // No .Net assembly output

				OutputPath = tpc.ExpandProperties(config.GetAttribute("OutputDirectory", ""));
				tpc.Set("OutDir", QQnPath.NormalizePath(QQnPath.Combine(ProjectPath, OutputPath), true));

				string intPath = tpc.ExpandProperties(config.GetAttribute("IntermediateDirectory", ""));
				intPath = QQnPath.Combine(ProjectPath, intPath);
				tpc.Set("IntDir", QQnPath.NormalizePath(intPath, true));

				ProjectOutput.BaseDirectory = ProjectPath;

				switch (int.Parse(config.GetAttribute("ConfigurationType", "").Trim(), NumberStyles.None))
				{
					case 1:
						TargetExt = ".exe";
						break;
					case 4:
						TargetExt = ".lib";
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

						TargetPath = tp;
					}
				}

				if (!File.Exists(QQnPath.Combine(ProjectPath, TargetPath)))
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
							IsAssembly = true;
							break;
					}
				}

				string targetFile = TargetPath;
				ProjectOutput.Add(new TargetItem(targetFile, targetFile, TargetType.Item));

				XPathNavigator n = config.SelectSingleNode("Tool[@Name='VCCLCompilerTool']");

				if (n != null)
				{
					if (string.Equals(n.GetAttribute("GenerateXMLDocumentationFiles", ""), "true", StringComparison.OrdinalIgnoreCase))
					{
						string xmlFile = Path.ChangeExtension(targetFile, ".xml");

						if (File.Exists(QQnPath.Combine(ProjectPath, xmlFile)))
							ProjectOutput.Add(new TargetItem(xmlFile, xmlFile, TargetType.Item));
					}
				}

				FindContentAndScripts(doc);

				

				string value = linker.GetAttribute("KeyFile", "");

				if(!string.IsNullOrEmpty(value))
				{
					KeyFile = EnsureRelativePath(tpc.ExpandProperties(value));
				}

				value = linker.GetAttribute("KeyContainer", "");
				if (!string.IsNullOrEmpty(value))
				{
					KeyContainer = value;
				}
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
