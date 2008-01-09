using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	/// <summary>
	/// 
	/// </summary>
	public static class TagExpander
	{
		const RegexOptions _reOptions = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.CultureInvariant |RegexOptions.ExplicitCapture | RegexOptions.Compiled;
		// Regexes have an extra paren around them for allowing combinations
		const string _itemRegex = @"(@\( \s* (?<ITEM>[A-Z_][A-Z0-9_-]*]*) \s* (-\> \s* '(?<TRANSFORM>[^']*)' \s* )? (\, \s* '(?<ITEM_SEPARATOR>[^']*)' \s*)? \))";
		const string _keyRegex = @"(%\( \s* ((?<ITEM_PREFIX>[A-Z_][A-Z0-9-]*) \s* \. \s*)? (?<KEY>[A-Z_][A-Z0-9]*) \s* \))";
		const string _propertyRegex = @"(\$\( \s* (?<PROPERTY>[A-Z_][A-Z0-9_-]*) \s* \))";

		static Regex _itemR;
		static Regex _keyR;
		static Regex _propertyR;
		static Regex _ItemKeyOrPropertyR;
		static Regex _KeyOrPropertyR;

		#region Public Regex definitions
		/// <summary>
		/// The name of the Item regex result in <see cref="ItemRegex"/>
		/// </summary>
		public const string RegexGroupItem = "ITEM";
		/// <summary>
		/// The name of the Item prefix regex result in <see cref="KeyRegex"/>
		/// </summary>
		public const string RegexGroupItemPrefix = "ITEM_PREFIX";
		/// <summary>
		/// The name of the Transform regex group in <see cref="ItemRegex"/>
		/// </summary>
		public const string RegexGroupTransform = "TRANSFORM";
		/// <summary>
		/// The name of the Separator regex group in <see cref="ItemRegex"/>
		/// </summary>
		public const string RegexGroupSeparator = "ITEM_SEPARATOR";		
		/// <summary>
		/// The name of the Key regex group in <see cref="KeyRegex"/>
		/// </summary>
		public const string RegexGroupKey = "KEY";
		/// <summary>
		/// The name of the Property regex group in <see cref="PropertyRegex"/>
		/// </summary>
		public const string RegexGroupProperty = "PROPERTY";
		
		/// <summary>
		/// Gets the item regex.
		/// </summary>
		/// <value>The item regex.</value>
		/// <remarks>Declares the groups <see cref="RegexGroupItem"/>, <see cref="RegexGroupSeparator"/> and <see cref="RegexGroupTransform"/></remarks>
		public static Regex ItemRegex
		{
			get { return _itemR ?? (_itemR = new Regex(_itemRegex, _reOptions)); }
		}

		/// <summary>
		/// Gets the key regex.
		/// </summary>
		/// <value>The key regex.</value>
		/// <remarks>Declares the groups <see cref="RegexGroupKey"/> and <see cref="RegexGroupItemPrefix"/></remarks>
		public static Regex KeyRegex
		{
			get { return _keyR ?? (_keyR = new Regex(_keyRegex, _reOptions)); }
		}

		/// <summary>
		/// Gets the property regex.
		/// </summary>
		/// <value>The property regex.</value>
		/// <remarks>Declares the group <see cref="RegexGroupProperty"/></remarks>
		public static Regex PropertyRegex
		{
			get { return _propertyR ?? (_propertyR = new Regex(_propertyRegex, _reOptions)); }
		}

		/// <summary>
		/// Gets the item key or property regex.
		/// </summary>
		/// <value>The item key or property regex.</value>
		/// <remarks>Or-ed combination of <see cref="ItemRegex"/>, <see cref="KeyRegex"/> and <see cref="PropertyRegex"/></remarks>
		internal static Regex ItemKeyOrPropertyRegex
		{
			get { return _ItemKeyOrPropertyR ?? (_ItemKeyOrPropertyR = new Regex(_itemRegex + "|" + _keyRegex + "|" + _propertyRegex, _reOptions)); }
		}

		/// <summary>
		/// Gets the key or property regex.
		/// </summary>
		/// <value>The key or property regex.</value>
		/// <remarks>Or-ed combination of <see cref="KeyRegex"/> and <see cref="PropertyRegex"/></remarks>
		internal static Regex KeyOrPropertyRegex
		{
			get { return _KeyOrPropertyR ?? (_KeyOrPropertyR = new Regex(_keyRegex + "|" + _propertyRegex, _reOptions)); }
		}

		#endregion

		/// <summary>
		/// Unescapes all %AA sequences in the string where A is a valid hexadecimal value
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <remarks>Skips all invalid escape sequences</remarks>
		public static string Unescape(string value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			int start = 0;
			int n;
			while (0 <= (n = value.IndexOf('%', start)))
			{
				start = n + 1;
				if (n > value.Length - 3)
					continue;

				string hex = value.Substring(n + 1, 2);
				if ((char.IsDigit(hex, 0) || ((hex[0] >= 'a') && hex[0] <= 'f') || ((hex[0] >= 'A') && hex[0] <= 'F')) &&
					(char.IsDigit(hex, 1) || ((hex[1] >= 'a') && hex[1] <= 'f') || ((hex[1] >= 'A') && hex[1] <= 'F')))
				{
					value = value.Substring(0, n) + (char)int.Parse(hex, NumberStyles.HexNumber) + value.Substring(n + 3);
				}
			}

			return value;
		}

		/// <summary>
		/// Unescapes the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="skipMeta">if set to <c>true</c> [skip meta].</param>
		/// <returns></returns>
		public static string Unescape(string value, bool skipMeta)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			if (!skipMeta)
				return Unescape(value);

			Regex re = ItemKeyOrPropertyRegex;

			StringBuilder sb = new StringBuilder();
			int start = 0;

			while (true)
			{
				Match m = re.Match(value, start);
				if (!m.Success)
					break;

				sb.Append(Unescape(value.Substring(start, m.Index)));
				sb.Append(m.Value);
				start = m.Index + m.Length;				
			}
			
			sb.Append(Unescape(value.Substring(start)));

			return sb.ToString();
		}

		/// <summary>
		/// Escapes the specified value.
		/// </summary>
		/// <param name="value">The string to escape.</param>
		/// <param name="escapeGlobs">if set to <c>true</c> escape '*' and '?' characters.</param>
		/// <param name="escapeMeta">if set to <c>true</c> escape '@(', '$(' and '%('.</param>
		/// <returns></returns>
		public static string Escape(string value, bool escapeGlobs, bool escapeMeta)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			for (int i = 0; i < value.Length; i++)
			{
				char next = (i < value.Length - 1) ? value[i+1] : '\0';
				switch (value[i])
				{
					case '%':
						if (next == '(' && escapeMeta)
							break;
						if (!(char.IsDigit(next) || (next >= 'a' && next <= 'f') || (next >= 'A' && next <= 'F')))
							continue;
						break;
					case '@':
					case '$':
						if (next != '(' || !escapeMeta)
							continue;
						break;
					case '*':
					case '?':
						if (!escapeGlobs)
							continue;
						break;
					case '\'':
					case ';':
						break;
					default:
						continue;
				}

				value = value.Substring(0, i) + "%" + ((byte)value[i]).ToString("X2", CultureInfo.InvariantCulture) + value.Substring(i+1);
				i += 2;
			}

			return value;
		}

		/// <summary>
		/// Escapes the specified value.
		/// </summary>
		/// <param name="value">The string to escape.</param>
		/// <param name="escapeGlobs">if set to <c>true</c> escape '*' and '?' characters.</param>
		/// <returns></returns>
		public static string Escape(string value, bool escapeGlobs)
		{
			return Escape(value, escapeGlobs, true);
		}

		/// <summary>
		/// Escapes the specified value.
		/// </summary>
		/// <param name="value">The string to escape.</param>
		/// <returns></returns>
		public static string Escape(string value)
		{
			return Escape(value, true, true);
		}
	}
}
