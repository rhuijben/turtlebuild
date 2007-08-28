using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using QQn.TurtleBuildUtils;
using QQn.TurtleUtils.IO;

namespace QQn.TurtleMSBuild
{
	abstract class Project
	{
		readonly TurtleParameters _parameters;
		readonly string _projectFile;
		string _projectName;
		readonly ProjectOutputList _projectOutput = new ProjectOutputList();
		string _configuration;
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

		string _outDir;

		/// <summary>
		/// Gets or sets the out dir.
		/// </summary>
		/// <value>The out dir.</value>
		public string OutDir
		{
			get { return _outDir; }
			protected set 
			{
				if (value == null)
					_outDir = null;
				else
					_outDir = QQnPath.EnsureRelativePath(ProjectPath, value).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar; 
			}
		}

		/// <summary>
		/// Gets the configuration.
		/// </summary>
		/// <value>The configuration.</value>
		public string Configuration
		{
			get { return _configuration; }
			set { _configuration = value; }
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
		public string TargetPath
		{
			get 
			{
				if (OutDir == null)
					return null;
				else if (TargetName == null)
					return null;
				else if (TargetExt == null)
					return null;
 
				return Path.Combine(OutDir, TargetName + TargetExt);
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


		/// <summary>
		/// Parses the result.
		/// </summary>
		/// <param name="parentProject">The parent project.</param>
		public virtual void ParseBuildResult(Project parentProject)
		{
			
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

		public virtual bool IsSolution
		{
			get { return false; }
		}

		public void WriteTBLog()
		{
			string outDir = OutDir;

			if (outDir == null)
				return;

			outDir = Parameters.OutputDir ?? Path.Combine(ProjectPath, OutDir);

			if (!Directory.Exists(outDir))
				Directory.CreateDirectory(outDir);

			string atPath = Path.Combine(outDir, ProjectName + ".tbLog");

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
			WriteScripts(xw, forReadability);


			xw.WriteEndElement();
			xw.WriteEndDocument();
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

			xw.WriteEndElement();
		}

		protected virtual void WriteProjectInfo(XmlWriter xw, bool forReadability)
		{
			xw.WriteAttributeString("name", ProjectName);
			xw.WriteAttributeString("path", ProjectPath);
			xw.WriteAttributeString("configuration", Configuration);
			xw.WriteAttributeString("outputDir", OutDir);

			xw.WriteAttributeString("targetName", TargetName);
			xw.WriteAttributeString("targetExt", TargetExt);
			xw.WriteAttributeString("file", Path.GetFileName(ProjectFile));
		}

		protected virtual void WriteTargetInfo(XmlWriter xw, bool forReadability)
		{
			xw.WriteAttributeString("src", TargetPath);

			if (!string.IsNullOrEmpty(KeyFile))
				xw.WriteAttributeString("keySrc", KeyFile);
			else if (!string.IsNullOrEmpty(KeyContainer))
				xw.WriteElementString("keyContainer", KeyContainer);

			DebugReference reference = AssemblyUtils.GetDebugReference(Path.Combine(ProjectPath, TargetPath));

			if (reference != null)
			{
				string pdbSrc = EnsureRelativePath(QQnPath.Combine(ProjectPath, Path.GetDirectoryName(TargetPath), reference.PdbFile));

				FileInfo pdbTarget = new FileInfo(Path.GetFullPath(QQnPath.Combine(ProjectPath, OutDir, Path.GetFileName(pdbSrc))));

				if(pdbTarget.Exists)
				{
					FileInfo pdbFrom = new FileInfo(Path.Combine(ProjectPath, pdbSrc));

					if(!pdbFrom.Exists || ((pdbFrom.Length == pdbTarget.Length) && (pdbFrom.LastWriteTime == pdbTarget.LastWriteTime)))
						pdbSrc = EnsureRelativePath(pdbTarget.FullName);

					if (pdbSrc.StartsWith("\\"))
						xw.WriteAttributeString("dbgTmp-1", pdbTarget.FullName);
				}
				else
					if (pdbSrc.StartsWith("\\"))
						xw.WriteAttributeString("dbgTmp-2", QQnPath.Combine(ProjectPath, Path.GetDirectoryName(TargetPath), reference.PdbFile));

				xw.WriteAttributeString("debugSrc", pdbSrc);

				

				xw.WriteAttributeString("debugId", reference.DebugId);
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
	}
}
