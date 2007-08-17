using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens.Tokenizers;
using QQn.TurtleUtils.Tokens;

namespace QQn.TurtleUtils.Tokens
{
	public static partial class Tokenizer
	{
		/// <summary>
		/// Tries to parse a command line.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="commandLineArguments">The command line arguments.</param>
		/// <param name="to">To.</param>
		/// <returns></returns>
		public static bool TryParseCommandLine<T>(IList<string> commandLineArguments, out T to)
			where T : class, new()
		{
			if (commandLineArguments == null)
				throw new ArgumentNullException("args");

			EnsureItems(commandLineArguments, "commandLineArguments");

			return CommandLineTokenizer<T>.TryParse(commandLineArguments, new TokenizerArgs(), out to);
		}

		/// <summary>
		/// Tries to parse a command line.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="commandLine">The command line.</param>
		/// <param name="to">To.</param>
		/// <returns></returns>
		public static bool TryParseCommandLine<T>(string commandLine, out T to)
			where T : class, new()
		{
			if (commandLine == null)
				throw new ArgumentNullException("commandLine");

			return CommandLineTokenizer<T>.TryParse(GetCommandlineWords(commandLine), new TokenizerArgs(), out to);
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
			else if (args == null)
				throw new ArgumentNullException("args");

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

			EnsureItems(commandLineArguments, "commandLineArguments");

			return CommandLineTokenizer<T>.TryParse(commandLineArguments, args, out to);
		}

		/// <summary>
		/// Gets the commandline words.
		/// </summary>
		/// <param name="commandLine">The command line.</param>
		/// <returns></returns>
		public static IList<string> GetCommandlineWords(string commandLine)
		{
			return GetWords(commandLine, new string[] { "\"\"" }, '\\', EscapeMode.EscapeGroupOnly, null);
		}		
	}
}
