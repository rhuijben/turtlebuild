using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokenizer;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	public class TBLogProject
	{
		[Token("name")]
		public string Name;

		[Token("path")]
		public string Path;

		[Token("configuration")]
		public string Configuration;

		[Token("outputDir")]
		public string OutputDir;

		[Token("targetName")]
		public string TargetName;

		[Token("targetExt")]
		public string TargetExt;

		[Token("file")]
		public string File;
	}
}
