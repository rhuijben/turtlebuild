using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.IO;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Collections;
using QQn.TurtleUtils.Tokens.Definitions;
using QQn.TurtleUtils.Tokens.Tokenizers;

namespace QQn.TurtleUtils.Tokens
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
	public static partial class Tokenizer
	{
		internal static void EnsureItems<T>(IEnumerable<T> items, string argumentName)
			where T : class
		{
			if (items == null)
				throw new ArgumentNullException("items");

			foreach (T i in items)
			{
				if (i == null)
					throw new ArgumentException(string.Format(TokenizerMessages.XInYIsNull, typeof(T).Name, argumentName), argumentName);
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

		internal static TokenizerState<T> NewState<T>(TokenizerArgs args, T instance)
			where T : class, new()
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			IHasTokenDefinition htd = instance as IHasTokenDefinition;
			TokenizerDefinition def = null;

			if (htd != null)
				def = htd.GetTokenizerDefinition(args);

			if (def == null)
				def = StaticTokenizerDefinition<T>.Definition;

			return new TokenizerState<T>(instance, def, args, false);
		}

		internal static TokenizerState<T> NewState<T>(TokenizerArgs args)
			where T : class, new()
		{
			T instance = new T();
			
			IHasTokenDefinition htd = instance as IHasTokenDefinition;
			TokenizerDefinition def = null;

			if (htd != null)
				def = htd.GetTokenizerDefinition(args);

			if (def == null)
				def = StaticTokenizerDefinition<T>.Definition;

			return new TokenizerState<T>(instance, def, args, true);
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