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
		/// <summary>
		/// The name of the project (= file name of project without extension)
		/// </summary>
		[Token("name")]
		public string Name;

		/// <summary>
		/// The full path to the directory where the project was located when the log was generated
		/// </summary>
		[Token("path")]
		public string Path;

		/// <summary>
		/// The configuration used to build the project
		/// </summary>
		[Token("configuration")]
		public string Configuration;

		/// <summary>
		/// The project relative output directory used when building the project
		/// </summary>
		[Token("outputDir")]
		public string OutputDir;

		/// <summary>
		/// The full name of the project file (relative from the project path)
		/// </summary>
		[Token("file")]
		public string File;
	}
}
