using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace QQn.TurtleBuildUtils.AttributeParsers
{
	/// <summary>
	/// 
	/// </summary>
	public class CSharpAttributeParser : AttributeParser<CSharpAttributeParser>
	{
		/// <summary>
		/// Gets a boolean indicating whether the parser can handle the specified fileextension
		/// </summary>
		/// <param name="extension">The extension.</param>
		/// <returns>
		/// true if the parser knows how to handle the extension
		/// </returns>
		public override bool HandlesExtension(string extension)
		{
			return ".cs".Equals(extension, StringComparison.InvariantCultureIgnoreCase);
		}

		// The csharp parser is +- the default parser implemented in AttributeParser, so almost no overrides are necessary

		/// <summary>
		/// Gets the alt string regex.
		/// </summary>
		/// <value>"@\"([^\"]|\"\")*\""</value>
		public override string AltStringRegex
		{
			get { return "\\@\"([^\"]|\"\")*\""; }
		}

		/// <summary>
		/// Gets the identifier regex.
		/// </summary>
		/// <value>The identifier regex.</value>
		protected override string IdentifierRegex
		{
			get { return "\\@?" + base.IdentifierRegex; }
		}
	}
}
