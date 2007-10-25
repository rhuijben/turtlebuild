using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace QQn.TurtleUtils.Tokens
{
	/// <summary>
	/// Minimalistic IXmlNamespaceResolver implementation
	/// </summary>
	public sealed class TokenNamespaceResolver : IXmlNamespaceResolver
	{
		Dictionary<string, string> _prefixToNs = new Dictionary<string, string>();
		Dictionary<string, string> _nsToPrefix = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenNamespaceResolver"/> class.
		/// </summary>
		public TokenNamespaceResolver()
		{
		}

		/// <summary>
		/// Adds the namespace.
		/// </summary>
		/// <param name="prefix">The prefix.</param>
		/// <param name="nameSpace">The name space.</param>
		public void AddNamespace(string prefix, string nameSpace)
		{
			if (_prefixToNs.ContainsKey(prefix) || _nsToPrefix.ContainsKey(nameSpace))
				throw new ArgumentException();

			_prefixToNs.Add(prefix, nameSpace);
			_nsToPrefix.Add(nameSpace, prefix);
		}

		#region IXmlNamespaceResolver Members

		/// <summary>
		/// Gets a collection of defined prefix-namespace mappings that are currently in scope.
		/// </summary>
		/// <param name="scope">An <see cref="T:System.Xml.XmlNamespaceScope"/> value that specifies the type of namespace nodes to return.</param>
		/// <returns>
		/// An <see cref="T:System.Collections.IDictionary"/> that contains the current in-scope namespaces.
		/// </returns>
		public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
		{
			return _prefixToNs;
		}

		/// <summary>
		/// Gets the namespace URI mapped to the specified prefix.
		/// </summary>
		/// <param name="prefix">The prefix whose namespace URI you wish to find.</param>
		/// <returns>
		/// The namespace URI that is mapped to the prefix; null if the prefix is not mapped to a namespace URI.
		/// </returns>
		public string LookupNamespace(string prefix)
		{
			string r;

			_prefixToNs.TryGetValue(prefix, out r);
			return r;
		}

		/// <summary>
		/// Gets the prefix that is mapped to the specified namespace URI.
		/// </summary>
		/// <param name="namespaceName">The namespace URI whose prefix you wish to find.</param>
		/// <returns>
		/// The prefix that is mapped to the namespace URI; null if the namespace URI is not mapped to a prefix.
		/// </returns>
		public string LookupPrefix(string namespaceName)
		{
			string r;

			_nsToPrefix.TryGetValue(namespaceName, out r);
			return r;
		}

		#endregion
	}
}
