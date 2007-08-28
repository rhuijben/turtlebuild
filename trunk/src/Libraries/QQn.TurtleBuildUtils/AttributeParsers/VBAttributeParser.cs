using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleBuildUtils.AttributeParsers
{
	/// <summary>
	/// 
	/// </summary>
	public class VBAttributeParser : AttributeParser<VBAttributeParser>
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
			return ".vb".Equals(extension, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
