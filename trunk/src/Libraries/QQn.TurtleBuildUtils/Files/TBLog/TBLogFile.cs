using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Xml;
using System.Xml.XPath;
using QQn.TurtleUtils.Tokens.Definitions;
using QQn.TurtleUtils.IO;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	public class TBLogFile : IHasFullPath, ITokenizerInitialize
	{
		TBLogGenerator _generator;
		TBLogProject _project;
		TBLogTarget _target;
		TBLogReferences _references;
		TBLogProjectOutput _projectOutput;
		TBLogContent _content;
		TBLogScripts _scripts;
		bool _completed;


		/// <summary>
		/// Gets "http://schemas.qqn.nl/2007/TurtleBuild/BuildResult"
		/// </summary>
		public const string Namespace = "http://schemas.qqn.nl/2007/TurtleBuild/BuildResult";

		void EnsureWritable()
		{
			if(_completed)
				throw new InvalidOperationException();
		}

		/// <summary>
		/// Gets information about the log generator
		/// </summary>
		[TokenGroup("Generator")]
		public TBLogGenerator Generator
		{
			get { return _generator ?? (_generator = new TBLogGenerator()); }
			set { EnsureWritable(); _generator = value; }
		}

		/// <summary>
		/// Gets project information
		/// </summary>
		[TokenGroup("Project")]
		public TBLogProject Project
		{
			get { return _project ?? (_project = new TBLogProject()); }
			set { EnsureWritable(); _project = value; }
		}

		/// <summary>
		/// Gets target information
		/// </summary>
		[TokenGroup("Target")]
		public TBLogTarget Target
		{
			get { return _target ?? (_target = new TBLogTarget()); }
			set { EnsureWritable(); _target = value; }
		}

		/// <summary>
		/// gets reference information
		/// </summary>
		[TokenGroup("References")]
		public TBLogReferences References
		{
			get { return _references ?? (_references = new TBLogReferences()); }
			set { EnsureWritable(); _references = value; }
		}

		/// <summary>
		/// Get project output
		/// </summary>
		[TokenGroup("ProjectOutput")]
		public TBLogProjectOutput ProjectOutput
		{
			get { return _projectOutput ?? (_projectOutput = new TBLogProjectOutput()); }
			set { EnsureWritable(); _projectOutput = value; }
		}

		/// <summary>
		/// Gets project content
		/// </summary>
		[TokenGroup("Content")]
		public TBLogContent Content
		{
			get { return _content ?? (_content = new TBLogContent()); }
			set { EnsureWritable(); _content = value; }
		}

		/// <summary>
		/// Gets scripts
		/// </summary>
		[TokenGroup("Scripts")]
		public TBLogScripts Scripts
		{
			get { return _scripts ?? (_scripts = new TBLogScripts()); }
			set { EnsureWritable(); _scripts = value; }
		}

		/// <summary>
		/// Loads the logfile at the specified path and parses it into a <see cref="TBLogFile"/> instance
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static TBLogFile Load(string path)
		{
			using (XmlReader xr = XmlReader.Create(path))
			{
				TBLogFile file;

				XPathDocument doc = new XPathDocument(xr);

				XPathNavigator nav = doc.CreateNavigator();
				nav.MoveToRoot();
				nav.MoveToFirstChild();

				if ((nav.NamespaceURI != Namespace) || (nav.LocalName != "TurtleBuildData"))
					return null;

				TokenizerArgs args = new TokenizerArgs();
				args.SkipUnknownNamedItems = true;

				if (Tokenizer.TryParseXml(nav, args, out file))
				{
					return file;
				}
				else
					return null;
			}
		}

		#region ITokenizerInitialize Members

		#endregion

		#region IHasFullPath Members

		string _fullPath;
		string IHasFullPath.FullPath
		{
			get { return _fullPath; }
		}

		/// <summary>
		/// Gets the project path.
		/// </summary>
		/// <value>The project path.</value>
		public string ProjectPath
		{
			get { return _fullPath; }
		}

		#endregion

		#region ITokenizerInitialize Members

		void ITokenizerInitialize.BeginInitialize(TokenizerEventArgs e)
		{
			
		}

		void ITokenizerInitialize.EndInitialize(TokenizerEventArgs e)
		{
			_completed = true;
			_fullPath = Project.Path;
			ProjectOutput.Parent = this;
			Content.Parent = this;
			Scripts.Parent = this;
		}

		#endregion

		/// <summary>
		/// Gets the full name of the target output
		/// </summary>
		/// <value>The full name of the target.</value>
		public string TargetFullName
		{
			get { return QQnPath.Combine(ProjectPath, Target.Src); }
		}
	}
}
