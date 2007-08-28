using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleBuildUtils.AttributeParsers
{
	/// <summary>
	/// Semi-Attribute parser for ecmascript based scripting languages
	/// </summary>
	/// <remarks>Allows versioning scriptfiles with a semi-attribute syntax</remarks>
	public class EcmaScriptAttributeParser : AttributeParser<EcmaScriptAttributeParser>
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
				".js".Equals(extension, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Gets a value indicating whether this parser supports namespaces
		/// </summary>
		/// <value><c>true</c> if [supports namespaces]; otherwise, <c>false</c>.</value>
		public override bool SupportsNamespaces
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the string that starts an attribute definition
		/// </summary>
		/// <value>"var"</value>
		protected override string AttributeStartToken
		{
			get
			{
				return "var";
			}
		}

		/// <summary>
		/// Gets the value start token.
		/// </summary>
		/// <value>"= ["</value>
		protected override string ValuesStartToken
		{
			get { return "= ["; }
		}

		/// <summary>
		/// Gets the values end token.
		/// </summary>
		/// <value>"]"</value>
		protected override string ValuesEndToken
		{
			get { return "]"; }
		}

		/// <summary>
		/// Gets the before named item token.
		/// </summary>
		/// <value>"[ '"</value>
		protected override string BeforeNamedItemToken
		{
			get { return "[ '"; }
		}

		/// <summary>
		/// Gets the named value separator.
		/// </summary>
		/// <value>"' : '"</value>
		protected override string NamedValueSeparatorToken
		{
			get { return "' :"; }
		}

		/// <summary>
		/// Gets the before named item token.
		/// </summary>
		/// <value>"' ]"</value>
		protected override string AfterNamedItemToken
		{
			get { return "]"; }
		}

		/// <summary>
		/// Gets the string that ends an attribute definition
		/// </summary>
		/// <value>"]"</value>
		protected override string AttributeEndToken
		{
			get { return ";"; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance can append attributes at the end of file.
		/// </summary>
		/// <value><c>false</c></value>
		public override bool CanAppendAttributesAtEndOfFile
		{
			get { return false; }
		}

		/// <summary>
		/// Gets the split token whitespace regex.
		/// </summary>
		/// <value>"\s"</value>
		protected override string SplitTokenWhitespaceRegex
		{
			get
			{
				return "\\s*";
			}
		}

		/// <summary>
		/// Gets the alt string regex.
		/// </summary>
		/// <value>String regex with ' instead of "</value>
		public override string AltStringRegex
		{
			get { return base.StringRegex.Replace('\"', '\''); }
		}

		/// <summary>
		/// Gets the char regex.
		/// </summary>
		/// <value><c>null</c></value>
		protected override string CharRegex
		{
			get { return null; }
		}
	}
}
