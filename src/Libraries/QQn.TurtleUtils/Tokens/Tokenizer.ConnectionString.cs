using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using QQn.TurtleUtils.Tokens;
using QQn.TurtleUtils.Tokens.Definitions;

namespace QQn.TurtleUtils.Tokens
{
	public static partial class Tokenizer
	{
		/// <summary>
		/// Tries the parse connection string.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="to">To.</param>
		/// <returns></returns>
		public static bool TryParseConnectionString<T>(string connectionString, out T to)
			where T : class, new()
		{
			return TryParseConnectionString<T>(connectionString, new TokenizerArgs(), out to);
		}

		/// <summary>
		/// Tries the parse connection string.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="args">The args.</param>
		/// <param name="to">To.</param>
		/// <returns></returns>
		public static bool TryParseConnectionString<T>(string connectionString, TokenizerArgs args, out T to)
			where T : class, new()
		{
			if (connectionString == null)
				throw new ArgumentNullException("connectionString");
			else if (args == null)
				throw new ArgumentNullException("args");

			to = null;
			using (TokenizerState<T> state = NewState<T>(args))
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
			using (TokenizerState<T> state = NewState<T>(args))
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
		public static bool TryParseNameValueCollection<T>(IDictionary<string, string> collection, TokenizerArgs args, out T to)
			where T : class, new()
		{
			if (collection == null)
				throw new ArgumentNullException("collection");
			else if (args == null)
				throw new ArgumentNullException("args");

			to = null;
			using (TokenizerState<T> state = NewState<T>(args))
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
	}
}
