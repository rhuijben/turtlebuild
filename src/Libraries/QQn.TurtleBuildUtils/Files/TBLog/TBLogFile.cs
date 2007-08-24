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
		/// <summary>
		/// 
		/// </summary>
		[TokenGroup("Generator")]
		public TBLogGenerator Generator = new TBLogGenerator();

		/// <summary>
		/// 
		/// </summary>
		[TokenGroup("Project")]
		public TBLogProject Project = new TBLogProject();

		/// <summary>
		/// 
		/// </summary>
		[TokenGroup("References")]
		public TBLogReferences References = new TBLogReferences();

		/// <summary>
		/// 
		/// </summary>
		[TokenGroup("ProjectOutput")]
		public TBLogProjectOutput ProjectOutput = new TBLogProjectOutput();

		/// <summary>
		/// 
		/// </summary>
		[TokenGroup("Content")]
		public TBLogContent Content = new TBLogContent();

		/// <summary>
		/// 
		/// </summary>
		[TokenGroup("Scripts")]
		public TBLogScripts Scripts = new TBLogScripts();

		/// <summary>
		/// Loads the logfile at the specified path.
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
			get { return QQnPath.Combine(ProjectPath, Project.OutputDir, Project.TargetName + Project.TargetExt); }
		}
	}
}
