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
			else if (ActionScriptAttributeParser.Default.HandlesExtension(extension))
				return ActionScriptAttributeParser.Default;

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
		protected Regex AttributeRegex
		{
			get { return _attributeRegex ?? (_attributeRegex = BuildAttributeRegex()); }
		}

		/// <summary>
		/// Gets the comment start string.
		/// </summary>
		/// <value>"//"</value>
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
		protected virtual string ValuesStartToken
		{
			get { return "("; }
		}

		/// <summary>
		/// Gets the values end token.
		/// </summary>
		/// <value>")"</value>
		protected virtual string ValuesEndToken
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
			get
			{
				string suffix = "(?<attribute>" + IdentifierRegex + ")";
				if (SupportsNamespaces)
					return "(?<nsPrefix>" + NamespacePrefixRegex + SplitTokenRegex + ")" + suffix;
				else
					return suffix;
			}
		}

		/// <summary>
		/// Gets the alt string regex.
		/// </summary>
		/// <value>The alt string regex.</value>
		public virtual string AltStringRegex
		{
			get { return null; }
		}

		/// <summary>
		/// Gets the string regex.
		/// </summary>
		/// <value>The string regex.</value>
		protected virtual string StringRegex
		{
			get { return "\"([^\\\\\"]|\\\\.)*\""; }
		}

		/// <summary>
		/// Gets the char regex.
		/// </summary>
		/// <value>The char regex.</value>
		protected virtual string CharRegex
		{
			get { return "'([^'\\\\]|\\\\.)'"; }
		}

		/// <summary>
		/// Gets the number regex.
		/// </summary>
		/// <value>The number regex.</value>
		protected virtual string NumberRegex
		{
			get { return "(0x[0-9a-fA-F]+)|(-?[0-9]+(\\.[0-9]*)?([eE][0-9]+)?)[UuLl]?"; }
		}

		/// <summary>
		/// Gets the enum regex.
		/// </summary>
		/// <value>The enum regex.</value>
		protected virtual string EnumRegex
		{
			get { return "(((" + NamespacePrefixRegex + ")?" + IdentifierRegex + EscapeAndSplitToken(EnumSeparatorToken) + ")+)?" + IdentifierRegex; }
		}

		string _valueRegex;
		/// <summary>
		/// Gets the value regex.
		/// </summary>
		/// <value>The value regex.</value>
		/// <remarks>The regex is generated in <see cref="BuildValueRegex"/></remarks>
		public string ValueRegex
		{
			get
			{
				if (_valueRegex == null)
				{
					StringBuilder sb = new StringBuilder();
					sb.Append("(");
					BuildValueRegex(sb);
					sb.Append(")");
					_valueRegex = sb.ToString();
				}
				return _valueRegex;
			}
		}

		/// <summary>
		/// Builds the value regex.
		/// </summary>
		/// <param name="builder">The builder.</param>
		protected virtual void BuildValueRegex(StringBuilder builder)
		{
			if (builder == null)
				throw new ArgumentNullException("builder");

			builder.Append("(");
			builder.Append(StringRegex);
			builder.Append(")");

			AppendRegexIfNotNull(builder, AltStringRegex);
			AppendRegexIfNotNull(builder, CharRegex);
			AppendRegexIfNotNull(builder, NumberRegex);
			AppendRegexIfNotNull(builder, EnumRegex);
		}

		internal static void AppendRegexIfNotNull(StringBuilder builder, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				builder.Append("|(");
				builder.Append(value);
				builder.Append(")");
			}
		}

		/// <summary>
		/// Gets the before named item token.
		/// </summary>
		/// <value><c>null</c></value>
		protected virtual string BeforeNamedItemToken
		{
			get { return null; }
		}

		/// <summary>
		/// Gets the before named item token.
		/// </summary>
		/// <value><c>null</c></value>
		protected virtual string AfterNamedItemToken
		{
			get { return null; }
		}

		/// <summary>
		/// Builds the attribute regex.
		/// </summary>
		/// <returns></returns>
		protected virtual Regex BuildAttributeRegex()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(LineStartRegex);
			sb.Append(SplitTokenRegex);
			sb.Append(EscapeAndSplitToken(AttributeStartToken));

			sb.Append(SplitTokenRegex);
			sb.Append(AttributeNameRegex);
			sb.Append(SplitTokenRegex);

			sb.Append(EscapeAndSplitToken(ValuesStartToken));
			sb.Append(SplitTokenRegex);

			string arg = "((?<arg>" + ValueRegex + ")|(" + NameValuePairRegex + "))";

			sb.Append("(");
			sb.Append(arg);
			sb.Append(SplitTokenRegex);
			sb.Append("(");			
			sb.Append(AttributeValueSeparatorToken);
			sb.Append(SplitTokenRegex);
			sb.Append(arg);
			sb.Append(SplitTokenRegex);
			sb.Append(")*)?");

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

				if (CommentEndToken != null)
					sb.Append(EscapeAndSplitToken(CommentEndToken));
				sb.Append(")?");
			}

			sb.Append(LineEndRegex);

			return new Regex(sb.ToString(), RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.CultureInvariant);
		}

		string _nameValuePair;

		/// <summary>
		/// Gets the name value pair regex.
		/// </summary>
		/// <value>The name value pair regex.</value>
		protected virtual string NameValuePairRegex
		{
			get
			{
				if (_nameValuePair == null)
				{
					StringBuilder nvPair = new StringBuilder();

					nvPair.Append("(");

					if (BeforeNamedItemToken != null)
					{
						nvPair.Append(EscapeAndSplitToken(BeforeNamedItemToken));
						nvPair.Append(SplitTokenRegex);
					}
					nvPair.Append("(?<name>");
					nvPair.Append(IdentifierRegex);
					nvPair.Append(")");
					nvPair.Append(SplitTokenRegex);
					nvPair.Append(EscapeAndSplitToken(NamedValueSeparatorToken));
					nvPair.Append(SplitTokenRegex);
					nvPair.Append("(?<value>");
					nvPair.Append(ValueRegex);
					nvPair.Append(")");

					if (AfterNamedItemToken != null)
					{
						nvPair.Append(SplitTokenRegex);
						nvPair.Append(AfterNamedItemToken);
					}

					nvPair.Append(")");

					_nameValuePair = nvPair.ToString();
				}
				return _nameValuePair;
			}
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
		/// Gets a value indicating whether this parser supports namespaces
		/// </summary>
		/// <value><c>true</c> if [supports namespaces]; otherwise, <c>false</c>.</value>
		public virtual bool SupportsNamespaces
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
			if (line == null)
				throw new ArgumentNullException("line");
			else if (line.Length < 5)
			{
				// 5 = [A()]
				return null;
			}

			Match m = AttributeRegex.Match(line);

			if (!m.Success)
				return null;

			AttributeDefinition ad = new AttributeDefinition();

			if (SupportsNamespaces)
			{
				ad.NamespacePrefix = m.Groups["nsPrefix"].Value;

				if (ad.NamespacePrefix != null)
					ad.NamespacePrefix = ad.NamespacePrefix.Replace(NamespaceSeparatorToken, ".").TrimEnd('.');
			}

			ad.Name = m.Groups["attribute"].Value;

			foreach (Capture c in m.Groups["arg"].Captures)
			{
				ad.Arguments.Add(c.Value);
			}

			foreach (Capture c in m.Groups["name"].Captures)
			{
				ad.NamedArguments.Add(new NamedAttributeArgument(c.Value));
			}

			CaptureCollection values = m.Groups["value"].Captures;
			for (int i = 0; i < values.Count; i++)
			{
				ad.NamedArguments[i].Value = values[i].Value;
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

			if (SupportsNamespaces && !string.IsNullOrEmpty(definition.NamespacePrefix))
			{
				sb.Append(definition.NamespacePrefix.Replace(".", NamespaceSeparatorToken));
				sb.Append(NamespaceSeparatorToken);
			}

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

					if (BeforeNamedItemToken != null)
						sb.Append(BeforeNamedItemToken);

					sb.Append(definition.NamedArguments[i].Name);
					sb.Append(NamedValueSeparatorToken);
					sb.Append(definition.NamedArguments[i].Value);

					if (AfterNamedItemToken != null)
						sb.Append(AfterNamedItemToken);
				}
			}

			sb.Append(ValuesEndToken);
			sb.Append(AttributeEndToken);

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
