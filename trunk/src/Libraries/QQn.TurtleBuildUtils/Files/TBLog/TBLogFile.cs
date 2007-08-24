using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Xml;
using System.Xml.XPath;
using QQn.TurtleUtils.Tokens.Definitions;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	public class TBLogFile : IHasFullPath, ITokenizerInitialize
	{
		[TokenGroup("Generator")]
		public TBLogGenerator Generator = new TBLogGenerator();

		[TokenGroup("Project")]
		public TBLogProject Project = new TBLogProject();

		[TokenGroup("References")]
		public TBLogReferences References = new TBLogReferences();

		[TokenGroup("ProjectOutput")]
		public TBLogProjectOutput ProjectOutput = new TBLogProjectOutput();

		[TokenGroup("Content")]
		public TBLogContent Content = new TBLogContent();

		[TokenGroup("Scripts")]
		public TBLogScripts Scripts = new TBLogScripts();

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
	}
}
