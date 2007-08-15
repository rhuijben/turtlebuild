using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using QQn.TurtleUtils.Tokenizer.Definitions;

namespace QQn.TurtleUtils.Tokenizer.Tokenizers
{
	static class XmlTokenizer<T>
		where T : class, new()
	{
		internal static bool TryParse(IXPathNavigable element, TokenizerArgs args, out T to)
		{
			XPathNavigator nav = element.CreateNavigator();

			to = null;
			using (TokenizerState<T> state = Tokenizer.NewState<T>(args))
			{
				if (nav.MoveToFirstAttribute())
				{
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
					nav.MoveToParent();
				}

				if (nav.HasChildren)
				{
					if (nav.MoveToFirstChild())
						do
						{
							string name = nav.LocalName;

							TokenGroupItem group;
							if (state.Definition.TryGetGroup(name, args.CaseSensitive, out group))
							{
								object value;

								if (!group.TryParseXml(nav, args, out value))
								{
									if (args.SkipUnknownNamedItems)
										continue;
									else
										return false;
								}

								group.Member.SetValue(state, value);
							}
						}
						while (nav.MoveToNext(XPathNodeType.Element));
				}

				to = state.Instance;
				return true;
			}
		}
	}
}
