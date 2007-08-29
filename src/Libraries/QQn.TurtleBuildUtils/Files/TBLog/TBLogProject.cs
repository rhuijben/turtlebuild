using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	public class TBLogProject
	{
		string _name;
		string _path;
		string _configuration;
		string _outputDir;
		string _file;

		/// <summary>
		/// The name of the project (= file name of project without extension)
		/// </summary>
		[Token("name")]
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// The full path to the directory where the project was located when the log was generated
		/// </summary>
		[Token("path")]
		public string Path
		{
			get { return _path; }
			set { _path = value; }
		}

		/// <summary>
		/// The configuration used to build the project
		/// </summary>
		[Token("configuration")]
		public string Configuration
		{
			get { return _configuration; }
			set { _configuration = value; }
		}
		
		/// <summary>
		/// The project relative output directory used when building the project
		/// </summary>
		[Token("outputDir")]
		public string OutputDir
		{
			get { return _outputDir; }
			set { _outputDir = value; }
		}

		/// <summary>
		/// The full name of the project file (relative from the project path)
		/// </summary>
		[Token("file")]
		public string File
		{
			get { return _file; }
			set { _file = value; }
		}
	}
}
