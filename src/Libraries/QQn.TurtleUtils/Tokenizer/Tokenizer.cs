using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using QQn.TurtleUtils.Tokenizer.Definitions;
using System.IO;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Collections;

namespace QQn.TurtleUtils.Tokenizer
{
	/// <summary>
	/// 
	/// </summary>
	public enum EscapeMode
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,
		/// <summary>
		/// 
		/// </summary>
		DoubleItem = 1,
		/// <summary>
		/// 
		/// </summary>
		EscapeCharacter = 2,
		/// <summary>
		/// 
		/// </summary>
		EscapeGroupOnly = 3,
	}

	/// <summary>
	/// 
	/// </summary>
	public static class Tokenizer
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="args"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static bool TryParseCommandLine<T>(IList<string> args, out T to)
			where T : class, new()
		{
			return TryParseCommandLine<T>(args, new TokenizerArgs(), out to);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="commandLine"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static bool TryParseCommandLine<T>(string commandLine, out T to)
			where T : class, new()
		{
			return TryParseCommandLine<T>(commandLine, new TokenizerArgs(), out to);
		}

		/// <summary>
		/// Tries to parse a command line
		/// </summary>
		/// <typeparam name="T">The type to parse to</typeparam>
		/// <param name="commandLine">The command line.</param>
		/// <param name="args">The args.</param>
		/// <param name="to">If successfull the result of the parsed commandline</param>
		/// <returns></returns>
		public static bool TryParseCommandLine<T>(string commandLine, TokenizerArgs args, out T to)
			where T : class, new()
		{
			if (commandLine == null)
				throw new ArgumentNullException("commandLine");

			return TryParseCommandLine<T>(GetCommandlineWords(commandLine), args, out to);
		}

		/// <summary>
		/// Tries to parse a command line
		/// </summary>
		/// <typeparam name="T">The type to parse to</typeparam>
		/// <param name="commandLineArguments">The command line arguments.</param>
		/// <param name="args">Arguments to customize the parsing rules</param>
		/// <param name="to">If successfull the result of the parsed commandline</param>
		/// <returns></returns>
		public static bool TryParseCommandLine<T>(IList<string> commandLineArguments, TokenizerArgs args, out T to)
			where T : class, new()
		{
			if (commandLineArguments == null)
				throw new ArgumentNullException("commandLineArgs");
			else if (args == null)
				throw new ArgumentNullException("args");

			foreach (string s in commandLineArguments)
			{
				if (s == null)
					throw new ArgumentException("Can't contain null", "commandLineArgs");
			}

			List<string> cArgs = new List<string>(commandLineArguments);

			T items;
			using (TokenizerState<T> state = NewState<T>(args, out items))
			{
				TokenizerDefinition definition = state.Definition;
				to = null;

				int i;

				bool atEnd = false;
				char[] checkChars = args.PlusMinSuffixArguments ? new char[] { args.ArgumentValueSeparator, '+', '-' } : new char[] { args.ArgumentValueSeparator };

				int nPlaced = 0;
				for (i = 0; i < cArgs.Count; i++)
				{
					string a = cArgs[i];

					if (!atEnd && (a.Length > 1) && args.CommandLineChars.Contains(a[0]))
					{
						bool twoStart = a[0] == a[1];
						if (a.Length == 2 && twoStart)
						{
							if (!definition.HasPlacedArguments)
							{
								args.ErrorMessage = TokenizerMessages.NoPlacedArgumentsDefined;
								return false;
							}

							atEnd = true;
						}
						else
						{
							int aFrom = twoStart ? 2 : 1;
							int aTo = args.AllowDirectArgs ? a.IndexOfAny(checkChars, aFrom) : -1;
							char cTo = (aTo > 0) ? a[aTo] : '\0';

							string item = (aTo > 0) ? a.Substring(aFrom, aTo - aFrom) : a.Substring(aFrom);

							TokenItem token;
							string value = null;

							if (definition.TryGetToken(item, args.CaseSensitive, out token))
							{
								if (token.RequiresValue)
								{
									if (i + 1 < cArgs.Count)
										token.Evaluate(cArgs[++i], state);
									else
									{
										args.ErrorMessage = TokenizerMessages.RequiredArgumentValueIsMissing;
										return false;
									}
								}
								else
									token.Evaluate(null, state);
							}
							else
							{
								// Look for a shorter argument
								for (int ii = item.Length - 1; ii > 0; ii--)
								{
									if (definition.TryGetToken(item.Substring(0, ii), args.CaseSensitive, out token)
										&& token.AllowDirectValue(item.Substring(ii), state))
									{
										token.EvaluateDirect(item.Substring(ii), state);
										break;
									}
									else
										token = null;
								}
							}

							if (token == null)
							{
								args.ErrorMessage = string.Format(TokenizerMessages.UnknownArgumentX, a);
								return false;
							}

							if (token.RequiresValue && value == null)
							{
								if (i < commandLineArguments.Count - 1)
									value = commandLineArguments[i++];
								else
								{
									args.ErrorMessage = string.Format(TokenizerMessages.ValueExpectedForArgumentX, a);
									return false;
								}
							}
							continue;
						}
					}
					else if (!atEnd && args.AllowResponseFile && a.Length > 1 && a[0] == '@')
					{
						string file = a.Substring(1);

						if (!File.Exists(file))
						{
							args.ErrorMessage = string.Format(TokenizerMessages.ResponseFileXNotFound, file);
							return false;
						}
						using (StreamReader sr = File.OpenText(a.Substring(1)))
						{
							string line;
							int n = i + 1;
							while (null != (line = sr.ReadLine()))
							{
								line = line.TrimStart();

								if (line.Length > 1)
								{
									if (line[0] != '#')
									{
										foreach (string word in GetCommandlineTokens(line))
										{
											cArgs.Insert(n++, word);
										}
									}
								}
							}
						}

						// TODO: Insert contents of response file in cArgs at position i+1

						continue;
					}
					else if (!args.AllowNamedBetweenPlaced)
						atEnd = true;

					if (state.Definition.HasPlacedArguments)
					{
						if (nPlaced < state.Definition.PlacedItems.Count)
						{
							state.Definition.PlacedItems[nPlaced].Evaluate(cArgs[i], state);
							nPlaced++;
						}
						else if (state.Definition.RestToken != null)
						{
							state.Definition.RestToken.Evaluate(cArgs[i], state);
						}
						else
						{
							args.ErrorMessage = string.Format(TokenizerMessages.UnknownArgumentX, cArgs[i]);
							return false;
						}
					}
				}

				if (!state.IsComplete)
					return false;

				to = state.Instance;
				return true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static IList<string> GetCommandlineTokens(string line)
		{
			// TODO: Implement "-splitting arguments
			List<string> words = new List<string>(line.Trim().Split(new char[] { ' ', '\r', '\n', '\t' }));

			for (int i = 0; i < words.Count; i++)
			{
				if (words[i].Length == 0)
					words.RemoveAt(i--);
			}

			return words.AsReadOnly();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connectionString"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static bool TryParseConnectionString<T>(string connectionString, out T to)
			where T : class, new()
		{
			return TryParseConnectionString<T>(connectionString, new TokenizerArgs(), out to);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connectionString"></param>
		/// <param name="args"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static bool TryParseConnectionString<T>(string connectionString, TokenizerArgs args, out T to)
			where T : class, new()
		{
			if (connectionString == null)
				throw new ArgumentNullException("connectionString");
			else if (args == null)
				throw new ArgumentNullException("args");

			to = null;
			T items;
			using (TokenizerState<T> state = NewState<T>(args, out items))
			{

				IList<string> groups = GetWords(connectionString, new string[] { "\"\"", "\'\'" }, '\0', EscapeMode.DoubleItem, ";".ToCharArray());

				foreach (string group in groups)
				{
					IList<string> parts = GetWords(group, new string[] { "\"\"", "\'\'" }, '\0', EscapeMode.DoubleItem, "=".ToCharArray());

					TokenItem token;
					if ((parts.Count == 2) && state.Definition.TryGetToken(parts[0], args.CaseSensitive, out token))
					{
						token.Evaluate(parts[1], state);
					}
					else if (args.SkipUnknownNamedItems)
						continue;
					else
						return false;
				}
				// TODO: Parse connectionstring using definition

				to = state.Instance;
				return true;
			}
		}

		/// <summary>
		/// Tries to parse the name value collection.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection">The collection.</param>
		/// <param name="args">The args.</param>
		/// <param name="to">To.</param>
		/// <returns></returns>
		public static bool TryParseNameValueCollection<T>(NameValueCollection collection, TokenizerArgs args, out T to)
			where T : class, new()
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			else if (args == null)
				throw new ArgumentNullException("args");

			to = null;
			T items;
			using (TokenizerState<T> state = NewState<T>(args, out items))
			{

				for (int i = 0; i < collection.Count; i++)
				{
					TokenItem ti;

					if (!state.Definition.TryGetToken(collection.Keys[i], args.CaseSensitive, out ti))
					{
						if (args.SkipUnknownNamedItems)
							continue;
						else
							return false;
					}

					ti.Evaluate(collection[i], state);
				}

				to = state.Instance;
				return true;
			}
		}

		/// <summary>
		/// Tries to parse the name value collection.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection">The collection.</param>
		/// <param name="args">The args.</param>
		/// <param name="to">To.</param>
		/// <returns></returns>
		public static bool TryParseNameValueCollection<T>(IEnumerable<KeyValuePair<string,string>> collection, TokenizerArgs args, out T to)
			where T : class, new()
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			else if (args == null)
				throw new ArgumentNullException("args");

			to = null;
			T items;
			using (TokenizerState<T> state = NewState<T>(args, out items))
			{

				foreach (KeyValuePair<string, string> kvp in collection)
				{
					TokenItem ti;

					if (!state.Definition.TryGetToken(kvp.Key, args.CaseSensitive, out ti))
					{
						if (args.SkipUnknownNamedItems)
							continue;
						else
							return false;
					}

					ti.Evaluate(kvp.Value, state);
				}

				to = state.Instance;
				return true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="from"></param>
		/// <returns></returns>
		public static string GenerateConnectionString<T>(T from)
			where T : class, new()
		{
			return GenerateConnectionString<T>(from, new TokenizerArgs());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="from"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string GenerateConnectionString<T>(T from, TokenizerArgs args)
			where T : class, new()
		{
			if (from == null)
				throw new ArgumentNullException("from");
			else if (args == null)
				throw new ArgumentNullException("args");

			return "";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="element"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static bool TryParseXmlAttributes<T>(IXPathNavigable element, out T to)
			where T : class, new()
		{
			return TryParseXmlAttributes<T>(element, new TokenizerArgs(), out to);
		}

		/// <summary>
		/// Tries to parse the XML attributes of an element to a T instance; Unkown attributes are ignored
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="element">The element.</param>
		/// <param name="args">The args.</param>
		/// <param name="to">To.</param>
		/// <returns></returns>
		public static bool TryParseXmlAttributes<T>(IXPathNavigable element, TokenizerArgs args, out T to)
			where T : class, new()
		{
			if (element == null)
				throw new ArgumentNullException("element");

			XPathNavigator nav = element.CreateNavigator();

			to = null;
			T items;
			using (TokenizerState<T> state = NewState<T>(args, out items))
			{

				if (nav.MoveToFirstAttribute())
					do
					{
						TokenItem ti;

						if (!state.Definition.TryGetToken(nav.LocalName, args.CaseSensitive, out ti))
						{
							if (args.SkipUnknownNamedItems)
								continue;
							else
								return false;
						}

						ti.Evaluate(nav.Value, state);
					}
					while (nav.MoveToNextAttribute());

				to = state.Instance;
				return true;
			}
		}

		static TokenizerState<T> NewState<T>(TokenizerArgs args, out T instance)
			where T : class, new()
		{
			instance = new T();
			
			IHasTokenDefinition htd = instance as IHasTokenDefinition;
			TokenizerDefinition def = null;

			if (htd != null)
				def = htd.GetTokenizerDefinition(args);

			if (def == null)
				def = StaticTokenizerDefinition<T>.Definition;

			return new TokenizerState<T>(instance, def, args);
		}

		static bool IsSeparator(char c, char[] wordSeparators)
		{
			if (wordSeparators != null)
				return Array.IndexOf(wordSeparators, c) >= 0;
			else
				return char.IsWhiteSpace(c);
		}

		/// <summary>
		/// Gets the words.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="groups">The groups.</param>
		/// <param name="escapeCharacter">The escape character.</param>
		/// <param name="mode">The mode.</param>
		/// <param name="wordSeparators">The word separators.</param>
		/// <returns></returns>
		public static IList<string> GetWords(string input, string[] groups, char escapeCharacter, EscapeMode mode, char[] wordSeparators)
		{
			if (input == null)
				throw new ArgumentNullException("input");

			Dictionary<char, char> _groups = new Dictionary<char, char>();
			if (groups != null)
				foreach (string g in groups)
				{
					if (g.Length != 2)
						throw new ArgumentException("Groups must be list of strings with 2 characters");
					else
						_groups.Add(g[0], g[1]);
				}

			if (wordSeparators != null && wordSeparators.Length == 0)
				wordSeparators = null;

			List<string> words = new List<string>();

			char groupEnd = '\0';
			bool inGroup = false;
			int start = -1;
			for (int i = 0; i < input.Length; i++)
			{
				char c = input[i];
				bool hasNext = i + 1 < input.Length;

				bool isSeparator = IsSeparator(c, wordSeparators);
				if (isSeparator && !inGroup)
				{
					if (hasNext && (mode == EscapeMode.DoubleItem) && input[i + 1] == c)
					{
						if (start < 0)
							start = i;

						i++;
						continue;
					}
					if (start >= 0)
					{
						words.Add(UnEscape(input.Substring(start, i - start), _groups, groups, escapeCharacter, mode, wordSeparators));
						start = -1;
						continue;
					}
				}

				if (start < 0)
					start = i;

				if (!inGroup && _groups.TryGetValue(input[i], out groupEnd))
				{
					inGroup = true;
				}
				else if (inGroup && input[i] == groupEnd)
				{
					if ((mode == EscapeMode.DoubleItem) && hasNext && input[i + 1] == groupEnd)
					{
						// Escaped end of group; will be removed in UnEscape
						i++;
						continue;
					}
					else
					{
						inGroup = false;
					}
				}

				if ((c == escapeCharacter) && hasNext)
				{
					switch (mode)
					{
						case EscapeMode.EscapeCharacter:
							i++; // Escaped character; will be removed in UnEscape
							continue;
						case EscapeMode.EscapeGroupOnly:
							char cn = input[i + 1];
							if (inGroup ? (cn == groupEnd) : _groups.ContainsKey(cn))
								i++; // Escaped character; will be removed in UnEscape
							// else: Just ignore the escape
							break;
					}
				}
			}

			if (start >= 0)
				words.Add(UnEscape(input.Substring(start), _groups, groups, escapeCharacter, mode, wordSeparators));

			return words;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>
		public static IList<string> GetCommandlineWords(string commandLine)
		{
			return GetWords(commandLine, new string[] { "\"\"" }, '\\', EscapeMode.EscapeGroupOnly, null);
		}

		static string UnEscape(string p, Dictionary<char, char> _groups, string[] groups, char escapeCharacter, EscapeMode mode, char[] wordSeparators)
		{
			if (mode == EscapeMode.DoubleItem)
			{
				if (wordSeparators != null)
					foreach (char c in wordSeparators)
					{
						string cs = c.ToString();
						p = p.Replace(cs + cs, cs);
					}
				else
				{
					for (int i = 0; i < p.Length; i++)
					{
						if (char.IsWhiteSpace(p, i))
						{
							int j = i + 1;
							while (j < p.Length && char.IsWhiteSpace(p, j))
								j++;

							p = p.Remove(i + 1, j - i - 1);
						}
					}
				}
			}
			return p;
		}

		/// <summary>
		/// Tries the expand file list.
		/// </summary>
		/// <param name="fileList">The file list.</param>
		/// <param name="args">The args.</param>
		/// <returns></returns>
		public static bool TryExpandFileList(IList<string> fileList, FileExpandArgs args)
		{
			if (fileList == null)
				throw new ArgumentNullException("fileList");
			else if (args == null)
				throw new ArgumentNullException("args");

			for (int i = 0; i < fileList.Count; i++)
			{
				string word = fileList[i];

				int firstMask = word.IndexOfAny(new char[] { '*', '?' });
				if (firstMask >= 0)
				{
					word = word.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

					DirectoryInfo rootDir;
					string rest = null;
					int lastSlash = word.LastIndexOf(Path.DirectorySeparatorChar, firstMask);
					if (lastSlash < 0)
					{
						rootDir = new DirectoryInfo(args.BaseDirectory);
						rest = word;
					}
					else
					{
						string dir = word.Substring(0, lastSlash);
						rootDir = new DirectoryInfo(Path.Combine(args.BaseDirectory, dir));
						rest = word.Substring(lastSlash + 1);
					}

					Regex fileRegex = ParseFileEx(rootDir, rest, args.FileExpandMode);

					SearchOption si = (args.FileExpandMode == FileExpandMode.DirectoryWildCards) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
					bool foundOne = true;

					if (args.MatchDirectories)
						foreach (DirectoryInfo fsi in rootDir.GetDirectories("*", si))
						{
							if (fileRegex.Match(fsi.FullName).Success)
							{
								fileList.Insert(i++, fsi.FullName);
								foundOne = true;
							}
						}
					if (args.MatchFiles)
						foreach (FileInfo fsi in rootDir.GetFiles("*", si))
						{
							if (fileRegex.Match(fsi.FullName).Success)
							{
								fileList.Insert(i++, fsi.FullName);
								foundOne = true;
							}
						}

					if (!foundOne && !args.RemoveNonExistingFiles)
						return false;

					fileList.RemoveAt(i--);
				}
				else if (args.RemoveNonExistingFiles && !(args.MatchDirectories ? Directory.Exists(word) : File.Exists(word)))
				{
					return false;
				}
			}
			return true;
		}

		private static Regex ParseFileEx(DirectoryInfo rootDir, string rest, FileExpandMode fileExpandMode)
		{
			StringBuilder sb = new StringBuilder();
			string escapedPathSeparator = Regex.Escape(Path.DirectorySeparatorChar.ToString());

			sb.AppendFormat("^{0}{1}", Regex.Escape(rootDir.FullName), escapedPathSeparator);

			for (int i = 0; i < rest.Length; i++)
			{
				char c = rest[i];
				switch (c)
				{
					case '?':
						sb.AppendFormat("[^{0}]", escapedPathSeparator);
						break;
					case '*':
						if (fileExpandMode == FileExpandMode.DirectoryWildCards && (i + 1 < rest.Length) && rest[i + 1] == '*')
						{
							i++;

							if ((i + 1 < rest.Length) && rest[i + 1] == Path.DirectorySeparatorChar)
							{
								sb.AppendFormat("(.+{0})?", escapedPathSeparator);
								i++;
							}
							else
								sb.Append(".*");
						}
						else
							sb.AppendFormat("[^{0}]*", escapedPathSeparator);
						break;					
					default:
						if (c == Path.DirectorySeparatorChar)
							sb.Append(escapedPathSeparator);
						else
							sb.Append(Regex.Escape(c.ToString()));
						break;
				}
			}
			sb.Append("$");
			return new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}
	}
}