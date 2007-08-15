using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using QQn.TurtleUtils.Tokenizer.Definitions;
using System.Xml;
using System.Collections;

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
							TokenItem ti;
							if (state.Definition.TryGetGroup(name, args.CaseSensitive, out group))
							{
								object value;

								if (!group.TryParseXml(nav, args, out value))
									return false;

								group.Member.SetValue(state, value);
							}
							else if (state.Definition.TryGetToken(name, args.CaseSensitive, out ti))
							{
								// Allow tokens as element
								ti.Evaluate(nav.Value, state);
							}
							else if (!args.SkipUnknownNamedItems)
								return false;
						}
						while (nav.MoveToNext(XPathNodeType.Element));
				}

				to = state.Instance;
				return true;
			}
		}

		internal static bool TryWrite(XmlWriter writer, T instance, TokenizerArgs args)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");
			else if (instance == null)
				throw new ArgumentNullException("instance");
			else if (args == null)
				throw new ArgumentNullException("args");

			Hashtable written = new Hashtable();

			using (TokenizerState<T> state = Tokenizer.NewState<T>(args, instance))
			{
				// Step 1: Try to write tokens as attributes
				foreach (TokenMember member in state.Definition.AllTokenMembers)
				{
					object[] values = member.GetValues(state);

					if (member.Tokens.Count > 0 && member.Groups.Count > 0)
						continue; // Write the members as element

					if((values == null) || (values.Length == 0) || values.Length > 1)
						continue;
					else if(member.Tokens.Count <= 0)
						continue;

					written[member] = member;
					
					foreach(object value in values)
					{
						if(value == null)
							continue;

						Type type = value.GetType();

						foreach (TokenItem ti in member.Tokens)
						{
							if (ti.Name == null)
								continue;

							if(ti.ValueType != null && !ti.ValueType.IsAssignableFrom(type))
								continue;
								
							// Will throw if multiple times written -> Definition bug, resolve there
							writer.WriteAttributeString(ti.Name, ti.GetStringValue(value, state));
							break;							
						}
					}
				}

				// Step 2: Write tokengroups and members with multiple values
				foreach (TokenMember member in state.Definition.AllTokenMembers)
				{
					if (written.Contains(member))
						continue;

					object[] values = member.GetValues(state);

					if ((values == null) || (values.Length == 0))
						continue;

					foreach (object value in values)
					{
						if (value == null)
							continue;

						Type type = value.GetType();

						bool writtenItem = false;

						foreach (TokenGroupItem tg in member.Groups)
						{
							if (tg.ValueType != null && !tg.ValueType.IsAssignableFrom(type))
								continue;

							writer.WriteStartElement(tg.Name);

							// Will throw if multiple times written -> Definition bug, resolve there
							if (!tg.TryWriteXml(writer, args, value))
								return false;							

							writer.WriteEndElement();
							writtenItem = true;
							break;
						}

						if (!writtenItem)
							foreach (TokenItem ti in member.Tokens)
							{
								if (ti.Name == null)
									continue;

								if (ti.ValueType != null && !ti.ValueType.IsAssignableFrom(type))
									continue;

								// Will throw if multiple times written -> Definition bug, resolve there
								writer.WriteElementString(ti.Name, ti.GetStringValue(value, state));
								break;
							}
					}
				}
			}

			return true;
		}
	}
}
