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

		[Token("ScriptItem")]
		public string[] ScriptItems;

		[Token("SharedItem")]
		public string[] SharedItems;

		[Token("CopyItem")]
		public string[] CopyItems;

		[Token("LocalItem")]
		public string[] LocalItems;
	}
}
