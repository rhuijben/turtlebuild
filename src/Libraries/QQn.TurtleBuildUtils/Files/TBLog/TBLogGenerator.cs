using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.ComponentModel;
using QQn.TurtleUtils.Tokens.Converters;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	public class TBLogGenerator
	{
		/// <summary>
		/// The name of the generator
		/// </summary>
		[Token("name")]
		public string Name;

		/// <summary>
		/// The product name of the generator
		/// </summary>
		[Token("product")]
		public string Product;

		/// <summary>
		/// The version of the generator
		/// </summary>
		[Token("version", TypeConverter=typeof(VersionConverter))]
		public Version Version;
	}
}
