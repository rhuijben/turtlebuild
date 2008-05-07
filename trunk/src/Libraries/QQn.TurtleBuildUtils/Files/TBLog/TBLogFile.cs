using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Xml;
using System.Xml.XPath;
using QQn.TurtleUtils.Tokens.Definitions;
using QQn.TurtleUtils.IO;
using System.IO;
using System.Diagnostics;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
    [DebuggerDisplay("Project={ProjectName}")]
	public class TBLogFile : ITokenizerInitialize
	{
		TBLogGenerator _generator;
		TBLogProject _project;
		bool _completed;
		TBLogScripts _scripts;
		TBLogConfigurationCollection _configurations;

		/// <summary>
		/// Gets "http://schemas.qqn.nl/2007/TurtleBuild/BuildResult"
		/// </summary>
		public const string Namespace = "http://schemas.qqn.nl/2007/TurtleBuild/BuildResult";

		void EnsureWritable()
		{
			if (_completed)
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
		/// Gets the configurations.
		/// </summary>
		/// <value>The configurations.</value>
		[TokenGroup("Configuration", typeof(TBLogConfiguration))]
		public TBLogConfigurationCollection Configurations
		{
			get { return _configurations ?? (_configurations = new TBLogConfigurationCollection()); }
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

        // Used from debugger
        string ProjectName
        {
            get { return Project.Name; }
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

		#region IHasFullPath Members

		string _fullPath;
		internal string BasePath
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

		void ITokenizerInitialize.OnBeginInitialize(TokenizerEventArgs e)
		{
			OnBeginInitialize(e);

		}

		/// <summary>
		/// Called when initialization via the tokenizer starts
		/// </summary>
		/// <param name="e">The <see cref="QQn.TurtleUtils.Tokens.TokenizerEventArgs"/> instance containing the event data.</param>
		protected virtual void OnBeginInitialize(TokenizerEventArgs e)
		{
		}

		void ITokenizerInitialize.OnEndInitialize(TokenizerEventArgs e)
		{
			OnEndInitialize(e);
			_completed = true;
		}

		/// <summary>
		/// Called when initialization via the tokenizer completed
		/// </summary>
		/// <param name="e">The <see cref="QQn.TurtleUtils.Tokens.TokenizerEventArgs"/> instance containing the event data.</param>
		protected virtual void OnEndInitialize(TokenizerEventArgs e)
		{
			_fullPath = Project.Path;
			Configurations.LogFile = this;
			Scripts.LogFile = this;
		}

		#endregion

		/// <summary>
		/// Gets all project output from all configurations
		/// </summary>
		/// <value>All project output.</value>
		/// <remarks>Project output should not contain duplicates</remarks>
		public IEnumerable<TBLogItem> AllProjectOutput
		{
			get
			{
				foreach (TBLogConfiguration config in Configurations)
				{
					foreach (TBLogItem item in config.ProjectOutput.Items)
						yield return item;
				}
			}
		}

		/// <summary>
		/// Gets all contents from all configurations (Filtered union)
		/// </summary>
		/// <value>All contents.</value>
		/// <remarks>List is filtered for duplicates</remarks>
		public IEnumerable<TBLogItem> AllContents
		{
			get
			{
				SortedFileList files = new SortedFileList();
				files.BaseDirectory = ProjectPath;

				foreach (TBLogConfiguration config in Configurations)
				{
					foreach (TBLogItem item in config.Content.Items)
					{
						if (files.Contains(item.Src))
							continue;
						
						files.Add(item.Src);
						yield return item;
					}
				}
			}
		}

		/// <summary>
		/// Gets all publish items (Filtered union of all ProjectOutput and Contents)
		/// </summary>
		/// <value>All publication items.</value>
		public IEnumerable<TBLogItem> AllPublishItems
		{
			get
			{
				SortedFileList files = new SortedFileList();
				files.BaseDirectory = ProjectPath;

				foreach (TBLogConfiguration config in Configurations)
				{
					foreach (TBLogItem item in config.ProjectOutput.Items)
					{
						if (files.Contains(item.Src))
							continue;

						files.Add(item.Src);
						yield return item;
					}

					foreach (TBLogItem item in config.Content.Items)
					{
						if (files.Contains(item.Src))
							continue;

						files.Add(item.Src);
						yield return item;
					}
				}
			}
		}

        /// <summary>
        /// Gets the last write time of any of the non shared files in the log
        /// </summary>
        /// <returns></returns>
        public DateTime GetLastWriteTime()
        {
            DateTime last = File.GetLastWriteTime(Project.File);

            foreach (TBLogItem item in AllPublishItems)
            {
                string src = item.FullSrc;

                if(!item.IsShared && File.Exists(src))
                {
                    DateTime dt = File.GetLastWriteTime(src);

                    if (dt > last)
                        last = dt;
                }
            }

            foreach (TBLogItem item in Scripts.Items)
            {
                string src = item.FullSrc;

                if (File.Exists(src))
                {
                    DateTime dt = File.GetLastWriteTime(src);

                    if (dt > last)
                        last = dt;
                }
            }

            return last;
        }
    }
}
