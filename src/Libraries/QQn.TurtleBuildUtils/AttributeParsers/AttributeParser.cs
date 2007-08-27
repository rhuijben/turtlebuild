using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace QQn.TurtleBuildUtils.AttributeParsers
{
	/// <summary>
	/// Generic attribute parser, for parsing assemblyinfo files
	/// </summary>
	public abstract class AttributeParser
	{
		/// <summary>
		/// Gets an <see cref="AttributeParser"/> for the specified file
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns>An <see cref="AttributeParser"/></returns>
		public static AttributeParser Get(string file)
		{
			if (string.IsNullOrEmpty(file))
				throw new ArgumentNullException("file");

			string extension = Path.GetExtension(file);

			if (CSharpAttributeParser.Default.HandlesExtension(extension))
				return CSharpAttributeParser.Default;
			else if (VBAttributeParser.Default.HandlesExtension(extension))
				return VBAttributeParser.Default;
			else if (MCppAttributeParser.Default.HandlesExtension(extension))
				return MCppAttributeParser.Default;
			else if (JSharpAttributeParser.Default.HandlesExtension(extension))
				return JSharpAttributeParser.Default;
			else if (EcmaScriptAttributeParser.Default.HandlesExtension(extension))
				return EcmaScriptAttributeParser.Default;

			return null;
		}


		Regex _attributeRegex;
		/// <summary>
		/// Gets the attribute parser regex
		/// </summary>
		/// <value>The attribute regex.</value>
		public Regex AttributeRegex
		{
			get { return _attributeRegex ?? (_attributeRegex = BuildAttributeRegex()); }
		}

		/// <summary>
		/// Gets the comment start string.
		/// </summary>
		/// <value>The string that starts a comment for the rest of the line (e.g. "//" for c#)</value>
		public virtual string CommentStartToken
		{
			get { return "//"; }
		}

		/// <summary>
		/// Gets the string that starts an attribute definition
		/// </summary>
		/// <value>The attribute start string (e.g. "[assembly:" for c#)</value>
		public virtual string AttributeStartToken
		{
			get { return "[assembly:"; }
		}

		/// <summary>
		/// Gets the string that ends an attribute definition
		/// </summary>
		/// <value>The attribute end string (e.g. "]" for c#)</value>
		public virtual string AttributeEndToken
		{
			get { return "]"; }
		}

		/// <summary>
		/// Gets the string that must be placed after an attribute definition as separator
		/// </summary>
		/// <value>The attribute end separator (e.g. <c>null</c> for c#; ";" for c++)</value>
		public virtual string AttributeEndSeparatorToken
		{
			get { return null; }
		}

		/// <summary>
		/// Gets the attribute optional suffix.
		/// </summary>
		/// <value>The attribute optional suffix (e.g. "Attribute" for C# and VB.Net)</value>
		public virtual string AttributeOptionalSuffix
		{
			get { return "Attribute"; }
		}

		/// <summary>
		/// Gets the attribute value separator.
		/// </summary>
		/// <value>The attribute value separator (e.g. "," in almost every language)</value>
		public virtual string AttributeValueSeparatorToken
		{
			get { return ","; }
		}

		/// <summary>
		/// Gets the named value separator.
		/// </summary>
		/// <value>The named value separator (e.g. "=" in C#)</value>
		public virtual string NamedValueSeparatorToken
		{
			get { return "="; }
		}

		/// <summary>
		/// Gets the split token regex.
		/// </summary>
		/// <value>The split token regex. "\s*"</value>
		public virtual string SplitTokenRegex
		{
			get { return "\\s*"; }
		}

		/// <summary>
		/// Gets the split token whitespace regex.
		/// </summary>
		/// <value>The split token whitespace regex. "\s"</value>
		public virtual string SplitTokenWhitespaceRegex
		{
			get { return "\\s"; }
		}

		/// <summary>
		/// Gets the value start token.
		/// </summary>
		/// <value>The value start token.</value>
		public string ValuesStartToken
		{
			get { return "("; }
		}

		/// <summary>
		/// Gets the values end token.
		/// </summary>
		/// <value>The values end token.</value>
		public string ValuesEndToken
		{
			get { return ")"; }
		}

		/// <summary>
		/// Tokenizer for the tokens specified in the properties; allows whitespace at breakingpoints between alphanumerical characters and symbols
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual string EscapeAndSplitToken(string value)
		{
			if (value.Length == 0)
				return "";

			int i = 0;
			for (; i < value.Length; i++)
				if (!char.IsLetterOrDigit(value, i) && value[i] != '_')
					break;

			if (i == value.Length)
				return Regex.Escape(value);
			else if ((i > 0) && (i < value.Length))
				return Regex.Escape(value.Substring(0, i)) + SplitTokenRegex + EscapeAndSplitToken(value.Substring(i));

			for (; i < value.Length; i++)
				if (!char.IsWhiteSpace(value, i))
					break;

			if (i == value.Length)
				return SplitTokenWhitespaceRegex;
			else if (i > 0)
				return SplitTokenWhitespaceRegex + SplitTokenRegex + EscapeAndSplitToken(value.Substring(i));

			i = 1;
			for (; i < value.Length; i++)
				if (!char.IsSymbol(value, i))
					break;

			if (i == value.Length)
				return Regex.Escape(value);
			else
				return Regex.Escape(value.Substring(0, i)) + SplitTokenRegex + EscapeAndSplitToken(value.Substring(i));
		}

		/// <summary>
		/// Gets the line start regex.
		/// </summary>
		/// <value>The line start regex ("\\s*")</value>
		public virtual string LineStartRegex
		{
			get { return "^\\s*"; }
		}

		// <summary>
		/// Gets the line start regex.
		/// </summary>
		/// <value>The line start regex ("\\s*")</value>
		public virtual string LineEndRegex
		{
			get { return "\\s*$"; }
		}

		/// <summary>
		/// Gets the identifier regex.
		/// </summary>
		/// <value>The identifier regex.</value>
		public virtual string IdentifierRegex
		{
			get { return "[A-Za-z_][A-Za-z0-9_]*"; }
		}

		/// <summary>
		/// Gets the namespace separator token.
		/// </summary>
		/// <value>The namespace separator token (e.g. "." for C#, "::" for C++)</value>
		public virtual string NamespaceSeparatorToken
		{
			get { return "."; }
		}

		/// <summary>
		/// Gets the namespace prefix regex.
		/// </summary>
		/// <value>The namespace prefix regex.</value>
		public string NamespacePrefixRegex
		{
			get { return "(?:" + IdentifierRegex + SplitTokenRegex + EscapeAndSplitToken(NamespaceSeparatorToken) + SplitTokenRegex + ")*"; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string AttributeNameRegex
		{
			get { return "(?<nsPrefix>" + NamespacePrefixRegex + SplitTokenRegex + ")(?<attribute>" + IdentifierRegex + ")"; }
		}

		/// <summary>
		/// Gets the value regex.
		/// </summary>
		/// <value>The value regex.</value>
		public string ValueRegex
		{
			get
			{
				const string stringRegex = "\"(?:[^\\\\\"]|\\\\.)*\"";
				const string charRegex = "'[^'\\\\]|\\\\.'";
				const string numberRegex = "(?:0x[0-9a-fA-F]+)|(?:-?[0-9]+([eE][0-9]+)?)";
				const string enumRegex = "[a-zA-Z_][0-9a-zA-Z_\\.]*";

				return "(?:(?:" + stringRegex + ")|(?:" + charRegex + ")|(?:" + numberRegex + ")|(?:" + enumRegex + "))";
			}
		}

		/// <summary>
		/// Builds the attribute regex.
		/// </summary>
		/// <returns></returns>
		protected virtual Regex BuildAttributeRegex()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(LineStartRegex);
			sb.Append(EscapeAndSplitToken(AttributeStartToken));

			sb.Append(SplitTokenRegex);
			sb.Append(AttributeNameRegex);
			sb.Append(SplitTokenRegex);

			sb.Append(EscapeAndSplitToken(ValuesStartToken));
			sb.Append(SplitTokenRegex);

			sb.Append("(?:(?:(?<arg>");
			// Normal arguments, optionally followed by named arguments
			sb.Append(ValueRegex);
			sb.Append(")"); // /<arg>
			sb.Append(SplitTokenRegex);

			sb.Append("(?:");
			sb.Append(EscapeAndSplitToken(AttributeValueSeparatorToken));
			sb.Append(SplitTokenRegex);
			sb.Append("(?<arg>");
			sb.Append(ValueRegex);
			sb.Append(")"); // /<arg>
			sb.Append(SplitTokenRegex);
			sb.Append(")*");

			StringBuilder sbNvFollow = null;
			if (NamedValueSeparatorToken != null)
			{
				sbNvFollow = new StringBuilder();
				sbNvFollow.Append("(?:");
				sbNvFollow.Append(EscapeAndSplitToken(AttributeValueSeparatorToken));
				sbNvFollow.Append(SplitTokenRegex);
				sbNvFollow.Append("(?<name>");
				sbNvFollow.Append(IdentifierRegex);
				sbNvFollow.Append(")");
				sbNvFollow.Append(SplitTokenRegex);
				sbNvFollow.Append(EscapeAndSplitToken(NamedValueSeparatorToken));
				sbNvFollow.Append(SplitTokenRegex);
				sbNvFollow.Append("(?<value>");
				sbNvFollow.Append(ValueRegex);
				sbNvFollow.Append("))*");

				sbNvFollow.Append(SplitTokenRegex);

				sb.Append(sbNvFollow);
			}
			// Named arguments only
			if (NamedValueSeparatorToken != null)
			{
				sb.Append(")|(?:");

				sb.Append("(?<name>");
				sb.Append(IdentifierRegex);
				sb.Append(")");
				sb.Append(SplitTokenRegex);
				sb.Append(EscapeAndSplitToken(NamedValueSeparatorToken));
				sb.Append(SplitTokenRegex);
				sb.Append("(?<value>");
				sb.Append(ValueRegex);
				sb.Append(")");

				sb.Append(sbNvFollow);
			}
			sb.Append("))?");

			sb.Append(SplitTokenRegex);
			sb.Append(EscapeAndSplitToken(ValuesEndToken));
			sb.Append(SplitTokenRegex);

			sb.Append(EscapeAndSplitToken(AttributeEndToken));
			sb.Append(SplitTokenRegex);

			if (AttributeEndSeparatorToken != null)
			{
				sb.Append(EscapeAndSplitToken(AttributeEndSeparatorToken));
				sb.Append(SplitTokenRegex);
			}

			if (CommentStartToken != null)
			{
				sb.Append(EscapeAndSplitToken(CommentStartToken));
				sb.Append(SplitTokenRegex);
				sb.Append("(?<comment>");
				sb.Append(".*");
				sb.Append(")");
			}

			sb.Append(LineEndRegex);

			return new Regex(sb.ToString());
		}
	}
}
