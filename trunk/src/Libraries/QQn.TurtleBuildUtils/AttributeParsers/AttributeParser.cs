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
		/// <remarks>The following groups are defined by this regex
		/// <list type="ordered">
		/// <item>'nsPrefix': The namespace prefix</item>
		/// <item>'attribute': The name of the attribute</item>
		/// <item>'arg': The attribute parameters</item>
		/// <item>'name': The names of the named parameters</item>
		/// <item>'value': The values of the named parameters</item>
		/// <item>'comment': The comment following the attribute definition</item>
		/// </list></remarks>
		public Regex AttributeRegex
		{
			get { return _attributeRegex ?? (_attributeRegex = BuildAttributeRegex()); }
		}

		/// <summary>
		/// Gets the comment start string.
		/// </summary>
		/// <value>The string that starts a comment for the rest of the line (e.g. "//" for c#)</value>
		protected virtual string CommentStartToken
		{
			get { return "//"; }
		}

		/// <summary>
		/// Gets the comment end token.
		/// </summary>
		/// <value>The comment end token.</value>
		protected virtual string CommentEndToken
		{
			get { return null; }
		}

		/// <summary>
		/// Gets the string that starts an attribute definition
		/// </summary>
		/// <value>"[assembly:"</value>
		protected virtual string AttributeStartToken
		{
			get { return "[assembly:"; }
		}

		/// <summary>
		/// Gets the string that ends an attribute definition
		/// </summary>
		/// <value>"]"</value>
		protected virtual string AttributeEndToken
		{
			get { return "]"; }
		}

		/// <summary>
		/// Gets the string that must be placed after an attribute definition as separator
		/// </summary>
		/// <value><c>null</c></value>
		protected virtual string AttributeEndSeparatorToken
		{
			get { return null; }
		}

		/// <summary>
		/// Gets the attribute optional suffix.
		/// </summary>
		/// <value>"Attribute"</value>
		protected virtual string AttributeOptionalSuffix
		{
			get { return "Attribute"; }
		}

		/// <summary>
		/// Gets the attribute value separator.
		/// </summary>
		/// <value>","</value>
		protected virtual string AttributeValueSeparatorToken
		{
			get { return ","; }
		}

		/// <summary>
		/// Gets the named value separator.
		/// </summary>
		/// <value>"="</value>
		protected virtual string NamedValueSeparatorToken
		{
			get { return "="; }
		}

		/// <summary>
		/// Gets the split token regex.
		/// </summary>
		/// <value>"\s*"</value>
		protected virtual string SplitTokenRegex
		{
			get { return "\\s*"; }
		}

		/// <summary>
		/// Gets the split token whitespace regex.
		/// </summary>
		/// <value>"\s"</value>
		protected virtual string SplitTokenWhitespaceRegex
		{
			get { return "\\s"; }
		}

		/// <summary>
		/// Gets the value start token.
		/// </summary>
		/// <value>"("</value>
		protected string ValuesStartToken
		{
			get { return "("; }
		}

		/// <summary>
		/// Gets the values end token.
		/// </summary>
		/// <value>")"</value>
		protected string ValuesEndToken
		{
			get { return ")"; }
		}

		/// <summary>
		/// Tokenizer for the tokens specified in the properties; allows whitespace at breakingpoints between alphanumerical characters and symbols
		/// </summary>
		/// <param name="value"></param>
		/// <returns>This escaped and split string</returns>
		protected virtual string EscapeAndSplitToken(string value)
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
		/// <value>"^\\s*"</value>
		protected virtual string LineStartRegex
		{
			get { return "^\\s*"; }
		}

		/// <summary>
		/// Gets the line start regex.
		/// </summary>
		/// <value>"\\s*$</value>
		protected virtual string LineEndRegex
		{
			get { return "\\s*$"; }
		}

		/// <summary>
		/// Gets the identifier regex.
		/// </summary>
		/// <value>The identifier regex.</value>
		protected virtual string IdentifierRegex
		{
			get { return "[A-Za-z_][A-Za-z0-9_]*"; }
		}

		/// <summary>
		/// Gets the namespace separator token.
		/// </summary>
		/// <value>The namespace separator token (e.g. "." for C#, "::" for C++)</value>
		protected virtual string NamespaceSeparatorToken
		{
			get { return "."; }
		}

		/// <summary>
		/// Gets the enum separator token.
		/// </summary>
		/// <value>"."</value>
		protected virtual string EnumSeparatorToken
		{
			get { return "."; }
		}

		/// <summary>
		/// Gets the namespace prefix regex.
		/// </summary>
		/// <value>The namespace prefix regex.</value>
		protected virtual string NamespacePrefixRegex
		{
			get { return "(" + IdentifierRegex + SplitTokenRegex + EscapeAndSplitToken(NamespaceSeparatorToken) + SplitTokenRegex + ")*"; }
		}

		/// <summary>
		/// Gets the attribute name regex.
		/// </summary>
		/// <value>The attribute name regex.</value>
		protected virtual string AttributeNameRegex
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
				const string stringRegex = "\"([^\\\\\"]|\\\\.)*\"";
				const string charRegex = "'[^'\\\\]|\\\\.'";
				const string numberRegex = "(0x[0-9a-fA-F]+)|(-?[0-9]+([eE][0-9]+)?)";
				string enumRegex = "(((" + NamespacePrefixRegex +")?" + IdentifierRegex + EscapeAndSplitToken(EnumSeparatorToken) + ")+)?" + IdentifierRegex;

				return "((" + stringRegex + ")|(" + charRegex + ")|(" + numberRegex + ")|(" + enumRegex + "))";
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

			sb.Append("(((?<arg>");
			// Normal arguments, optionally followed by named arguments
			sb.Append(ValueRegex);
			sb.Append(")"); // /<arg>
			sb.Append(SplitTokenRegex);

			sb.Append("(");
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
				sbNvFollow.Append("(");
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
				sb.Append(")|(");

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
				sb.Append("(");
				sb.Append(EscapeAndSplitToken(CommentStartToken));
				sb.Append(SplitTokenRegex);
				sb.Append("(?<comment>");
				sb.Append(".*");
				sb.Append(")");

				if(CommentEndToken != null)
					sb.Append(EscapeAndSplitToken(CommentEndToken));
				sb.Append(")?");
			}

			sb.Append(LineEndRegex);

			return new Regex(sb.ToString(), RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.CultureInvariant);
		}

		/// <summary>
		/// Gets a value indicating whether this instance can append attributes at the end of file.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can append attributes at end of file; otherwise, <c>false</c>.
		/// </value>
		public virtual bool CanAppendAttributesAtEndOfFile
		{
			get { return true; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance can insert attributes between existing attributes
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can insert attributes; otherwise, <c>false</c>.
		/// </value>
		public virtual bool CanInsertAttributes
		{
			get { return true; }
		}

		/// <summary>
		/// Parses the attribute on the line
		/// </summary>
		/// <param name="line">The line to parse</param>
		/// <returns>The <see cref="AttributeDefinition"/> of the found attribute, or <c>null</c> if the line does not contain an attribute</returns>
		public AttributeDefinition ParseLine(string line)
		{
			if(line == null)
				throw new ArgumentNullException("line");
			else if(line.Length < 5)
			{
				// 5 = [A()]
				return null;
			}

			Match m = AttributeRegex.Match(line);

			if (!m.Success)
				return null;

			AttributeDefinition ad = new AttributeDefinition();
			
			ad.NamespacePrefix = m.Groups["nsPrefix"].Value;

			if(ad.NamespacePrefix != null)
				ad.NamespacePrefix.Replace(NamespaceSeparatorToken, ".");

			ad.Name = m.Groups["name"].Value;

			foreach(Capture c in m.Groups["arg"].Captures)
			{
				ad.Arguments.Add(c.Value);
			}

			foreach(Capture c in m.Groups["name"].Captures)
			{
				ad.NamedArguments.Add(new NamedAttributeArgument(c.Value));
			}

			CaptureCollection values = m.Groups["value"].Captures;
			for(int i = 0; i < values.Count; i++)
			{
				ad.NamedArguments[i++].Value = values[i].Value;
			}

			ad.Comment = m.Groups["comment"].Value;

			return ad;
		}

		/// <summary>
		/// Generates the attribute.
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <returns></returns>
		public string GenerateAttributeLine(AttributeDefinition definition)
		{
			if (definition == null)
				throw new ArgumentNullException("definition");

			StringBuilder sb = new StringBuilder();
			sb.Append(AttributeStartToken);

			if(!string.IsNullOrEmpty(definition.NamespacePrefix))
				sb.Append(definition.NamespacePrefix.Replace(".", NamespaceSeparatorToken));

			sb.Append(definition.Name);
			sb.Append(ValuesStartToken);
			for (int i = 0; i < definition.Arguments.Count; i++)
			{
				if (i > 0)
					sb.Append(AttributeValueSeparatorToken);

				sb.Append(definition.Arguments[i]);
			}

			if (definition.NamedArguments.Count > 0)
			{
				for (int i = 0; i < definition.NamedArguments.Count; i++)
				{
					if (i > 0 || (definition.Arguments.Count > 0))
						sb.Append(AttributeValueSeparatorToken);

					sb.Append(definition.NamedArguments[i].Name);
					sb.Append(NamedValueSeparatorToken);
					sb.Append(definition.NamedArguments[i++].Value);
				}
			}

			sb.Append(ValuesEndToken);
			if (AttributeEndSeparatorToken != null)
				sb.Append(AttributeEndSeparatorToken);


			if (CommentStartToken != null && definition.Comment != null)
			{
				sb.Append(CommentStartToken);
				sb.Append(definition.Comment);
				if (CommentEndToken != null)
					sb.Append(CommentEndToken);
			}

			return sb.ToString();
		}
	}
}
