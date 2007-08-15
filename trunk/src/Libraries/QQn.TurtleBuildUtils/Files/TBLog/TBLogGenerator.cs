using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokenizer;
using System.ComponentModel;
using QQn.TurtleUtils.Tokenizer.Converters;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	public class TBLogGenerator
	{
		[Token("name")]
		public string Name;

		[Token("product")]
		public string Product;

		[Token("version", TypeConverter=typeof(VersionConverter))]
		public Version Version;
	}
}
