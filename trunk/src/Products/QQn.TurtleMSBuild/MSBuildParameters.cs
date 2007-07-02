using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokenizer;

namespace QQn.TurtleLogger
{
	public class BuildParameters
	{
		[Token("OutputDir")]
		public string OutputDir;

		[Token("Indent")]
		public bool Indent;

		[Token("ScriptExtension")]
		public string[] ScriptExtensions;
	}
}
