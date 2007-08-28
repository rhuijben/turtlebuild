using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleBuildUtils.AttributeParsers
{
	/// <summary>
	/// 
	/// </summary>
	public class JSharpAttributeParser : AttributeParser<JSharpAttributeParser>
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
			return ".jsl".Equals(extension, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Gets the string that starts an attribute definition
		/// </summary>
		/// <value>"/** @assembly "</value>
		protected override string AttributeStartToken
		{
			get { return "/** @assembly "; }
		}

		/// <summary>
		/// Gets the string that ends an attribute definition
		/// </summary>
		/// <value>"*/"</value>
		protected override string AttributeEndToken
		{
			get { return "*/"; }
		}
	}
}
