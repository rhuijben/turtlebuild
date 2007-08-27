using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleBuildUtils.AttributeParsers
{
	class EcmaScriptAttributeParser : AttributeParser<EcmaScriptAttributeParser>
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
			return
				".js".Equals(extension, StringComparison.InvariantCultureIgnoreCase) ||
				".as".Equals(extension, StringComparison.InvariantCultureIgnoreCase) ||
				".hx".Equals(extension, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
