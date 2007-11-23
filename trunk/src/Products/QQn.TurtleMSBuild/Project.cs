using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using QQn.TurtleBuildUtils;
using QQn.TurtleUtils.IO;
using QQn.TurtleUtils.Tokens.Converters;
using System.Xml.XPath;
using QQn.TurtleUtils.Tokens;
using Microsoft.Build.Framework;

namespace QQn.TurtleMSBuild
{
	abstract class Project
	{
		readonly TurtleParameters _parameters;
		readonly string _projectFile;
		string _projectName;
		readonly ProjectOutputList _projectOutput = new ProjectOutputList();
		string _projectConfiguration;
		string _projectType;
		string _projectPlatform;
		string _buildEngineTargets;
		ReferenceList _references = new ReferenceList();

		/// <summary>
		/// Initializes a new instance of the <see cref="Project"/> class.
		/// </summary>
		/// <param name="projectFile">The project file.</param>
		/// <param name="parameters">The parameters.</param>
		public Project(string projectFile, TurtleParameters parameters)
			: this(projectFile, Path.GetFileNameWithoutExtension(projectFile), parameters)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Project"/> class.
		/// </summary>
		/// <param name="projectFile">The project file.</param>
		/// <param name="projectName">Name of the project.</param>
		/// <param name="parameters">The parameters.</param>
		public Project(string projectFile, string projectName, TurtleParameters parameters)
		{
			if (string.IsNullOrEmpty(projectFile))
				throw new ArgumentNullException("projectFile");
			else if (string.IsNullOrEmpty(projectName))
				throw new ArgumentNullException("projectFile");
			else if (parameters == null)
				throw new ArgumentNullException("parameters");
			
			_projectFile = projectFile;
			ProjectName = projectName;
			_parameters = parameters;

			_contentFiles = new SortedFileList();
			_scriptFiles = new SortedFileList();
			_contentFiles.BaseDirectory = ProjectPath;
			_scriptFiles.BaseDirectory = ProjectPath;
		}

		/// <summary>
		/// Gets the project file.
		/// </summary>
		/// <value>The project file.</value>
		public string ProjectFile
		{
			get { return _projectFile; }
		}

		/// <summary>
		/// Gets or sets the name of the project.
		/// </summary>
		/// <value>The name of the project.</value>
		public string ProjectName
		{
			get { return _projectName; }
			protected set { _projectName = value ?? Path.GetFileNameWithoutExtension(ProjectFile); }
		}

		/// <summary>
		/// Gets the parameters.
		/// </summary>
		/// <value>The parameters.</value>
		protected internal TurtleParameters Parameters
		{
			get { return _parameters; }
		}

		string _projectPath;

		/// <summary>
		/// Gets the project path.
		/// </summary>
		/// <value>The project path.</value>
		public string ProjectPath
		{
			get 
			{ 
				if(_projectPath == null)
					_projectPath = Path.GetDirectoryName(ProjectFile);

				return _projectPath;
			}
		}

		string _outputPath;		
		string _targetPlatform;
		string _processorArchitecture;

		/// <summary>
		/// Gets or sets the output path, relative from the project root
		/// </summary>
		/// <value>The output path.</value>
		public string OutputPath
		{
			get { return _outputPath; }
			protected set 
			{
				if (value == null)
					_outputPath = null;
				else
					_outputPath = QQnPath.EnsureRelativePath(ProjectPath, value).TrimEnd(Path.DirectorySeparatorChar);
			}
		}

		/// <summary>
		/// Gets or sets the project configuration.
		/// </summary>
		/// <value>The configuration.</value>
		public string ProjectConfiguration
		{
			get { return _projectConfiguration; }
			set { _projectConfiguration = value; }
		}

		/// <summary>
		/// Gets or sets the type of the project.
		/// </summary>
		/// <value>The type of the project.</value>
		public string ProjectType
		{
			get { return _projectType; }
			protected set { _projectType = value; }
		}

		/// <summary>
		/// Gets or sets the project platform.
		/// </summary>
		/// <value>The platform.</value>
		public string ProjectPlatform
		{
			get { return _projectPlatform; }
			protected set { _projectPlatform = value; }
		}

		/// <summary>
		/// Gets or sets the target platform.
		/// </summary>
		/// <value>The target platform.</value>
		public string TargetPlatform
		{
			get { return _targetPlatform; }
			protected set { _targetPlatform = value; }
		}

		/// <summary>
		/// Gets or sets the processor architecture.
		/// </summary>
		/// <value>The processor architecture.</value>
		public string ProcessorArchitecture
		{
			get { return _processorArchitecture; }
			protected set { _processorArchitecture = value; }
		}

		string _targetName;
		/// <summary>
		/// Gets the name of the target.
		/// </summary>
		/// <value>The name of the target.</value>
		public string TargetName
		{
			get { return _targetName; }
			protected set { _targetName = value; }
		}

		string _targetExt;
		/// <summary>
		/// Gets or sets the target extension.
		/// </summary>
		/// <value>The target ext.</value>
		public string TargetExt
		{
			get { return _targetExt; }
			protected set { _targetExt = value; }
		}


		/// <summary>
		/// Gets the target path.
		/// </summary>
		/// <value>The target path.</value>
		public virtual string TargetPath
		{
			get 
			{
				if (OutputPath == null)
					return null;
				else if (TargetName == null)
					return null;
				else if (TargetExt == null)
					return null;

				return QQnPath.Combine(OutputPath, TargetName + TargetExt);
			}
			internal set
			{
				throw new InvalidOperationException();
			}
		}

		AssemblyReference _assembly;
		public AssemblyReference TargetAssembly
		{
			get { return _assembly; }
			protected set { _assembly = value; }
		}


		public ReferenceList References
		{
			get { return _references; }
		}

		readonly SortedFileList _contentFiles;
		protected SortedFileList ContentFiles
		{
			get { return _contentFiles; }
		}
		readonly SortedFileList _scriptFiles;
		protected SortedFileList ScriptFiles
		{
			get { return _scriptFiles; }
		}

		string _keyFile;
		string _keyContainer;
		string _dbgSrc;
		string _dbgId;

		public string KeyFile
		{
			get { return _keyFile; }
			set { _keyFile = value; }
		}

		public string KeyContainer
		{
			get { return _keyContainer; }
			set { _keyContainer = value; }
		}

		public string DebugSrc
		{
			get { return _dbgSrc; }
			set { _dbgSrc = value; }
		}

		public string DebugId
		{
			get { return _dbgId; }
			set { _dbgId = value; }
		}

		/// <summary>
		/// Parses the result.
		/// </summary>
		/// <param name="parentProject">The parent project.</param>
		public virtual void ParseBuildResult(Project parentProject)
		{
			
		}

		public virtual void PostParseBuildResult()
		{
			if (string.IsNullOrEmpty(TargetPath) || string.IsNullOrEmpty(ProjectPath))
				return;

			if ((DebugSrc == null) || (DebugId == null))
			{
				string targetFile = GetFullPath(TargetPath);

				if (QQnPath.IsAssemblyFile(targetFile) && File.Exists(targetFile))
				{
					DebugReference reference = AssemblyUtils.GetDebugReference(targetFile);

					if (reference != null)
					{
						string pdbSrc = EnsureRelativePath(QQnPath.Combine(ProjectPath, Path.GetDirectoryName(TargetPath), reference.PdbFile));
						FileInfo pdbTarget = new FileInfo(Path.GetFullPath(QQnPath.Combine(ProjectPath, Path.GetDirectoryName(TargetPath), Path.GetFileName(pdbSrc))));

						if (pdbTarget.Exists)
						{
							FileInfo pdbFrom = new FileInfo(GetFullPath(pdbSrc));

							if (!pdbFrom.Exists || ((pdbFrom.Length == pdbTarget.Length) && (pdbFrom.LastWriteTime == pdbTarget.LastWriteTime)))
								pdbSrc = EnsureRelativePath(pdbTarget.FullName);
						}

						DebugSrc = pdbSrc;
						DebugId = reference.DebugId;
					}
					else
					{
						string pdbFile = Path.ChangeExtension(targetFile, ".pdb");

						if (ProjectOutput.ContainsKey(pdbFile) && File.Exists(pdbFile))
						{
							pdbFile = EnsureRelativePath(pdbFile);

							DebugSrc = pdbFile;
						}
					}
				}
			}	
		}

		/// <summary>
		/// Makes a relative path from the path
		/// </summary>
		/// <param name="include">The include.</param>
		/// <returns></returns>
		public string EnsureRelativePath(string include)
		{
			return QQnPath.EnsureRelativePath(ProjectPath, include);
		}

		public string GetFullPath(string include)
		{
			return Path.GetFullPath(QQnPath.Combine(ProjectPath, include));
		}

		public virtual bool IsSolution
		{
			get { return false; }
		}

		XPathDocument _previousLog;
		public void WriteTBLog()
		{
			if (TargetName == null)
				return;

			string outDir = OutputPath;

			outDir = Parameters.OutputPath ?? GetFullPath(OutputPath);

			if (!Directory.Exists(outDir))
				Directory.CreateDirectory(outDir);

			string atPath = QQnPath.Combine(outDir, ProjectName + ".tbLog");

			if (File.Exists(atPath))
			{
				using (FileStream fs = File.OpenRead(atPath))
				{
					_previousLog = new XPathDocument(fs);
				}
			}

			using (StreamWriter sw = new StreamWriter(atPath, false, Encoding.UTF8))
			{
				XmlWriterSettings xs = new XmlWriterSettings();

				xs.Indent = Parameters.Indent;

				using (XmlWriter xw = XmlWriter.Create(sw, xs))
				{
					WriteTBLog(xw, xs.Indent);
				}
			}
		}

		public void WriteTBLog(XmlWriter xw, bool forReadability)
		{
			xw.WriteStartDocument();
			xw.WriteStartElement(null, "TurtleBuildData", Ns);

			WriteGenerator(xw, forReadability);

			xw.WriteStartElement("Project");
			WriteProjectInfo(xw, forReadability);
			xw.WriteEndElement();

			WriteOldConfigurations(xw);

			xw.WriteStartElement("Configuration");
			WriteConfigurationInfo(xw, forReadability);


			xw.WriteStartElement("Target");
			WriteTargetInfo(xw, forReadability);
			xw.WriteEndElement();

			WriteAssemblyInfo(xw, forReadability);

			xw.WriteStartElement("References");
			WriteAssemblyReferences(xw, forReadability);
			WriteProjectReferences(xw, forReadability);
			xw.WriteEndElement();			

			WriteProjectOutput(xw, forReadability);
			WriteContent(xw, forReadability);

			xw.WriteEndElement();

			WriteScripts(xw, forReadability);

			xw.WriteEndElement();
			xw.WriteEndDocument();
		}

		private void WriteOldConfigurations(XmlWriter xw)
		{
			if(_previousLog != null)
			{
				TokenNamespaceResolver resolver = new TokenNamespaceResolver();
				resolver.AddNamespace("tb", Ns);
				foreach (XPathNavigator n in _previousLog.CreateNavigator().Select("/tb:TurtleBuildData/tb:Configuration", resolver))
				{
					if (n.GetAttribute("name", "") == ProjectConfiguration)
					{
						if ((n.GetAttribute("platform", "") == ProjectPlatform) && (n.GetAttribute("type", "") == ProjectType))
							continue; // Remove duplicate outputs
					}

					n.WriteSubtree(xw);
				}
			}
		}

		static T GetAttribute<T>(ICustomAttributeProvider provider)
			where T : Attribute
		{
			object[] attrs = provider.GetCustomAttributes(typeof(T), false);

			if (attrs != null && attrs.Length > 0)
				return (T)attrs[0];
			else
				return null;
		}

		protected void WriteGenerator(XmlWriter xw, bool forReadability)
		{
			xw.WriteStartElement("Generator");
			Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

			string product = GetAttribute<AssemblyProductAttribute>(assembly).Product;
			string version = assembly.GetName().Version.ToString(4);

			xw.WriteAttributeString("name", typeof(MSBuildLogger).FullName);
			xw.WriteAttributeString("product", product);
			xw.WriteAttributeString("version", version);
			xw.WriteAttributeString("date", UtcDateTimeConverter.ToString(DateTime.UtcNow));

			xw.WriteEndElement();
		}

		protected virtual void WriteProjectInfo(XmlWriter xw, bool forReadability)
		{
			xw.WriteAttributeString("name", ProjectName);
			xw.WriteAttributeString("path", ProjectPath);

			xw.WriteAttributeString("file", Path.GetFileName(ProjectFile));
		}

		private void WriteConfigurationInfo(XmlWriter xw, bool forReadability)
		{
			xw.WriteAttributeString("name", ProjectConfiguration);
			xw.WriteAttributeString("platform", ProjectPlatform);
			xw.WriteAttributeString("basePath", ProjectPath);
			xw.WriteAttributeString("outputPath", OutputPath);

			xw.WriteAttributeString("type", ProjectType); // Add an extra type field to allow solutions with the name of a project
		}

		protected virtual void WriteTargetInfo(XmlWriter xw, bool forReadability)
		{
			xw.WriteAttributeString("src", TargetPath);

			xw.WriteAttributeString("name", TargetName);
			xw.WriteAttributeString("ext", TargetExt);			

			if (!string.IsNullOrEmpty(KeyFile))
				xw.WriteAttributeString("keySrc", KeyFile);
			else if (!string.IsNullOrEmpty(KeyContainer))
				xw.WriteElementString("keyContainer", KeyContainer);

			xw.WriteAttributeString("processorArchitecture", ProcessorArchitecture);
			xw.WriteAttributeString("platform", TargetPlatform);

			if (DebugSrc != null)
			{
				xw.WriteAttributeString("debugSrc", DebugSrc);

				if (DebugId != null)
					xw.WriteAttributeString("debugId", DebugId);
			}						
		}

		protected virtual void WriteManifest(XmlWriter xw, bool forReadability)
		{
			if (!string.IsNullOrEmpty(Parameters.BuildManifest))
			{
				if (!File.Exists(Parameters.BuildManifest))
					throw new FileNotFoundException("BuildManifest specified in the BuildManifestProperty not found", Parameters.BuildManifest);

				XmlDocument doc = new XmlDocument();
				doc.Load(Parameters.BuildManifest);

				xw.WriteStartElement("BuildManifest");

				doc.DocumentElement.WriteContentTo(xw);

				xw.WriteEndElement();
			}
		}

		/// <summary>
		/// Writes the assembly info.
		/// </summary>
		/// <param name="xw">The xw.</param>
		/// <param name="forReadability">if set to <c>true</c> [for readability].</param>
		protected virtual void WriteAssemblyInfo(XmlWriter xw, bool forReadability)
		{
			if (TargetAssembly != null)
			{
				xw.WriteStartElement("Assembly");
				TargetAssembly.WriteAttributes(xw);

				xw.WriteEndElement();
			}
		}

		protected virtual void WriteAssemblyReferences(XmlWriter xw, bool forReadability)
		{
			References.WriteReferences(xw, forReadability);
		}

		protected virtual void WriteProjectReferences(XmlWriter xw, bool forReadability)
		{
			
		}

		/// <summary>
		/// Writes the project output.
		/// </summary>
		/// <param name="xw">The xw.</param>
		/// <param name="forReadability">if set to <c>true</c> [for readability].</param>
		protected virtual void WriteProjectOutput(XmlWriter xw, bool forReadability)
		{
			ProjectOutput.WriteProjectOutput(xw, forReadability);
		}

		/// <summary>
		/// Writes the content.
		/// </summary>
		/// <param name="xw">The xw.</param>
		/// <param name="forReadability">if set to <c>true</c> [for readability].</param>
		protected virtual void WriteContent(XmlWriter xw, bool forReadability)
		{
			xw.WriteStartElement("Content");
			foreach (string file in ContentFiles)
			{
				xw.WriteStartElement("Item");
				xw.WriteAttributeString("src", file);

				xw.WriteEndElement();
			}
			xw.WriteEndElement();
		}

		/// <summary>
		/// Writes the scripts.
		/// </summary>
		/// <param name="xw">The xw.</param>
		/// <param name="forReadability">if set to <c>true</c> [for readability].</param>
		protected virtual void WriteScripts(XmlWriter xw, bool forReadability)
		{
			xw.WriteStartElement("Scripts");
			foreach (string file in ScriptFiles)
			{
				xw.WriteStartElement("Item");
				xw.WriteAttributeString("src", file);

				xw.WriteEndElement();
			}
			xw.WriteEndElement();
		}


		/// <summary>
		/// Gets "http://schemas.qqn.nl/2007/TurtleBuild/BuildResult"
		/// </summary>
		public const string Ns = QQn.TurtleBuildUtils.Files.TBLog.TBLogFile.Namespace;

		/// <summary>
		/// Gets the project output.
		/// </summary>
		/// <value>The project output.</value>
		public ProjectOutputList ProjectOutput
		{
			get { return _projectOutput; }
		}

		/// <summary>
		/// Gets or sets the build engine targets.
		/// </summary>
		/// <value>The build engine targets.</value>
		internal string BuildEngineTargets
		{
			get { return _buildEngineTargets; }
			set { _buildEngineTargets = value; }
		}
		
		/// <summary>
		/// Calculates the target.
		/// </summary>
		/// <param name="pi">The pi.</param>
		/// <returns></returns>
		protected string CalculateTarget(ITaskItem pi)
		{
			string target = pi.GetMetadata("TargetPath");
			if (!string.IsNullOrEmpty(target))
				return QQnPath.Combine(OutputPath, target);

			string destinationSubDirectory = pi.GetMetadata("DestinationSubDirectory");
			string filename = Path.GetFileName(pi.ItemSpec);

			if (!string.IsNullOrEmpty(destinationSubDirectory))
				return QQnPath.Combine(OutputPath, destinationSubDirectory, filename);
			else
				return QQnPath.Combine(OutputPath, filename);
		}
	}
}
