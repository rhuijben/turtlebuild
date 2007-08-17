using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using QQn.TurtleUtils.Tokens.Tokenizers;
using System.Xml;

namespace QQn.TurtleUtils.Tokens
{
	public static partial class Tokenizer
	{
		/// <summary>
		/// Tries to parse the Xml.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="element">The element.</param>
		/// <param name="to">To.</param>
		/// <returns></returns>
		public static bool TryParseXml<T>(IXPathNavigable element, out T to)
			where T : class, new()
		{
			if (element == null)
				throw new ArgumentNullException("element");

			return XmlTokenizer<T>.TryParse(element, new TokenizerArgs(), out to);
		}

		/// <summary>
		/// Tries to parse the XML attributes of an element to a T instance; Unkown attributes are ignored
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="element">The element.</param>
		/// <param name="args">The args.</param>
		/// <param name="to">To.</param>
		/// <returns></returns>
		public static bool TryParseXml<T>(IXPathNavigable element, TokenizerArgs args, out T to)
			where T : class, new()
		{
			if (element == null)
				throw new ArgumentNullException("element");
			else if (args == null)
				throw new ArgumentNullException("args");

			return XmlTokenizer<T>.TryParse(element, args, out to);
		}

		/// <summary>
		/// Tries to write XML.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static bool TryWriteXml<T>(XmlWriter xmlWriter, T value)
			where T : class, new()
		{
			return XmlTokenizer<T>.TryWrite(xmlWriter, value, new TokenizerArgs());			
		}

		/// <summary>
		/// Tries to write XML.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="value">The value.</param>
		/// <param name="args">The args.</param>
		/// <returns></returns>
		public static bool TryWriteXml<T>(XmlWriter xmlWriter, T value, TokenizerArgs args)
			where T : class, new()
		{
			return XmlTokenizer<T>.TryWrite(xmlWriter, value, args);
		}
	}
}
