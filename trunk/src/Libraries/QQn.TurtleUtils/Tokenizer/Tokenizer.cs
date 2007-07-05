using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using QQn.TurtleUtils.Tokenizer.Definitions;
using System.IO;
using System.Collections.Specialized;

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
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="commandLine"></param>
		/// <param name="args"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static bool TryParseCommandLine<T>(string commandLine, TokenizerArgs args, out T to)
			where T : class, new()
		{
			if (commandLine == null)
				throw new ArgumentNullException("commandLine");

			return TryParseCommandLine<T>(GetCommandlineWords(commandLine), args, out to);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="commandLineArgs"></param>
		/// <param name="args"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static bool TryParseCommandLine<T>(IList<string> commandLineArgs, TokenizerArgs args, out T to)
			where T : class, new()
		{
			if (commandLineArgs == null)
				throw new ArgumentNullException("commandLineArgs");
			else if (args == null)
				throw new ArgumentNullException("args");

			foreach (string s in commandLineArgs)
			{
				if (s == null)
					throw new ArgumentException("Can't contain null", "commandLineArgs");
			}

			List<string> cArgs = new List<string>(commandLineArgs);

			T items;
			TokenizerState<T> state = NewState<T>(args, out items);
			TokenizerDefinition definition = state.Definition;
			to = null;

			// TODO: Parse commandline using definition
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
									&& token.AllowDirectValue(item.Substring(ii)))
								{
									token.EvaluateDirect(item.Substring(ii));
									break;
								}
								else
									token = null;
							}
						}

						if (token == null)
						{
							return false;
						}

						if (token.RequiresValue && value == null)
						{
							if (i < commandLineArgs.Count - 1)
								value = commandLineArgs[i++];
							else
							{
							}
						}
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
							foreach (string word in GetCommandlineTokens(line))
							{
								cArgs.Insert(n++, word);
							}
						}
					}

					// TODO: Insert contents of response file in cArgs at position i+1

					continue;
				}
				else if (!args.AllowNamedBetweenPlaced)
					atEnd = true;
				else if (state.Definition.HasPlacedArguments)
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
			TokenizerState<T> state = NewState<T>(args, out items);

			IList<string> groups = GetWords(connectionString, new string[] { "\"\"", "\'\'" }, '\0', EscapeMode.DoubleItem, ";".ToCharArray());

			foreach (string group in groups)
			{
				IList<string> parts = GetWords(group, new string[] { "\"\"", "\'\'" }, '\0', EscapeMode.DoubleItem, "=".ToCharArray());

				TokenItem token;
				if ((parts.Count == 2) && state.Definition.TryGetToken(parts[0], args.CaseSensitive, out token))
				{
					token.Evaluate(parts[1], state);
				}
				else
					return false;
			}
			// TODO: Parse connectionstring using definition

			to = state.Instance;
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <param name="args"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static bool TryParseConnectionString<T>(NameValueCollection collection, TokenizerArgs args, out T to)
			where T : class, new()
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			else if (args == null)
				throw new ArgumentNullException("args");

			T items;
			TokenizerState<T> state = NewState<T>(args, out items);

			// TODO: Parse connectionstring using definition

			to = null;
			return false;
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
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="element"></param>
		/// <param name="args"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static bool TryParseXmlAttributes<T>(IXPathNavigable element, TokenizerArgs args, out T to)
			where T : class, new()
		{

			T items;
			TokenizerState<T> state = NewState<T>(args, out items);

			// TODO: Parse xml element using definition

			to = null;
			return false;
		}

		static Dictionary<Type, TokenizerDefinition> _definitions = new Dictionary<Type, TokenizerDefinition>();

		static TokenizerState<T> NewState<T>(TokenizerArgs args, out T instance)
			where T : class, new()
		{
			instance = new T();

			TokenizerDefinition def = null;

			IHasTokenDefinition htd = instance as IHasTokenDefinition;

			if (htd != null)
				def = htd.GetTokenizerDefinition(args);

			if (def != null)
				return new TokenizerState<T>(instance, def, args);

			lock (_definitions)
			{
				if (_definitions.TryGetValue(typeof(T), out def))
					return new TokenizerState<T>(instance, def, args);
			}

			def = new StaticTokenizerDefinition<T>();
			if (args.CacheDefinition)
				lock (_definitions)
				{
					_definitions[typeof(T)] = def;
				}

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
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="groups"></param>
		/// <param name="escapeCharacter"></param>
		/// <param name="mode"></param>
		/// <param name="wordSeparators"></param>
		/// <returns></returns>
		public static IList<string> GetWords(string input, string[] groups, char escapeCharacter, EscapeMode mode, char[] wordSeparators)
		{
			if (input == null)
				throw new ArgumentNullException("input");

			Dictionary<char, char> _groups = new Dictionary<char, char>();
			if(groups != null)
				foreach (string g in groups)
				{
					if (g.Length != 2)
						throw new ArgumentException("Groups must be list of strings with 2 characters");
					else
						_groups.Add(g[0], g[1]);
				}

			if(wordSeparators != null && wordSeparators.Length == 0)
				wordSeparators = null;

			List<string> words = new List<string>();

			char groupEnd = '\0';
			bool inGroup = false;
			int start = -1;
			for(int i = 0; i < input.Length; i++)
			{
				char c = input[i];
				bool hasNext = i+1 < input.Length;

				bool isSeparator = IsSeparator(c, wordSeparators);
				if(isSeparator && !inGroup)
				{
					if (hasNext && (mode == EscapeMode.DoubleItem) && input[i + 1] == c)
					{
						if (start < 0)
							start = i;

						i++;
						continue;
					}
					if(start >= 0)
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

			if(start >= 0)
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
	}
}