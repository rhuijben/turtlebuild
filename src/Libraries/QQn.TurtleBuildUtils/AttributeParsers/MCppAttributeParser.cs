using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleBuildUtils.AttributeParsers
{
	/// <summary>
	/// 
	/// </summary>
	public class MCppAttributeParser : AttributeParser<MCppAttributeParser>
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
			return ".cpp".Equals(extension, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Gets the string that must be placed after an attribute definition as separator
		/// </summary>
		/// <value>";" for C++</value>
		protected override string AttributeEndSeparatorToken
		{
			get
			{	// In C++ attribute definitions must contain a closing ;
				return ";";
			}
		}

		/// <summary>
		/// Gets the namespace separator token.
		/// </summary>
		/// <value>"::" for C++</value>
		protected override string NamespaceSeparatorToken
		{
			get
			{
				return "::";
			}
		}

		/// <summary>
		/// Gets the enum separator token.
		/// </summary>
		/// <value>"::" for C++</value>
		protected override string EnumSeparatorToken
		{
			get
			{
				return "::";
			}
		}

		/// <summary>
		/// Gets the string regex.
		/// </summary>
		/// <value>The string regex; which allows static concatenation between #defines and strings</value>
		protected override string StringRegex
		{
			get
			{
				string part = "((" + base.StringRegex + ")|(" + base.IdentifierRegex + "))";
				return part + "(\\s+" + part +")*";
			}
		}
	}
}
