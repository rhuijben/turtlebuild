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

		/// <summary>
		/// Gets the string that starts an attribute definition
		/// </summary>
		/// <value>"&lt;assembly:"</value>
		protected override string AttributeStartToken
		{
			get
			{
				return "<assembly:";
			}
		}

		/// <summary>
		/// Gets the string that ends an attribute definition
		/// </summary>
		/// <value>"&gt;"</value>
		protected override string AttributeEndToken
		{
			get
			{
				return ">";
			}
		}

		/// <summary>
		/// Gets the char regex.
		/// </summary>
		/// <value><c>null</c></value>
		protected override string CharRegex
		{
			get { return null; }
		}

		/// <summary>
		/// Gets the string regex.
		/// </summary>
		/// <value>The string regex.</value>
		protected override string StringRegex
		{
			get { return "\"([^\"]|\"\")*\""; }
		}

		/// <summary>
		/// Gets the comment start string.
		/// </summary>
		/// <value>"'"</value>
		protected override string CommentStartToken
		{
			get { return "'"; }
			
		}
	}
}
