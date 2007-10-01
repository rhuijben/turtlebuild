using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleBuildUtils.AttributeParsers
{
	/// <summary>
	/// 
	/// </summary>
	public class ActionScriptAttributeParser : EcmaScriptAttributeParser
	{
		static ActionScriptAttributeParser _instance;

		/// <summary>
		/// Gets the default.
		/// </summary>
		/// <value>The default.</value>
		public static new ActionScriptAttributeParser Default
		{
			get { return _instance ?? (_instance = new ActionScriptAttributeParser()); }
		}

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
				".as".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
				".as2".Equals(extension, StringComparison.OrdinalIgnoreCase) ||
				".hx".Equals(extension, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Gets the string that starts an attribute definition
		/// </summary>
		/// <value>"static var"</value>
		protected override string AttributeStartToken
		{
			get { return "static var";  }
		}
	}
}
