using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using QQn.TurtleUtils.Tokens.Definitions;

namespace QQn.TurtleUtils.Tokens.Tokenizers
{
	static class CommandLineTokenizer<T>
		where T : class, new()
	{
		internal static bool TryParse(IList<string> items, TokenizerArgs args, out T to)			
		{
			if (items == null)
				throw new ArgumentNullException("items");
			else if (args == null)
				throw new ArgumentNullException("args");

			List<string> cArgs = new List<string>(items);

			using (TokenizerState<T> state = Tokenizer.NewState<T>(args))
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

								continue;
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
								if (i < cArgs.Count - 1)
									value = cArgs[i++];
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
							args.ErrorMessage = string.Format(CultureInfo.InvariantCulture, TokenizerMessages.ResponseFileXNotFound, file);
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
										foreach (string word in Tokenizer.GetCommandlineWords(line))
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
	}
}
