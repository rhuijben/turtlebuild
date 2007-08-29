using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using QQn.TurtleUtils.Tokens.Tokenizers;

namespace QQn.TurtleUtils.Tokens.Definitions
{
	/// <summary>
	/// 
	/// </summary>
	public class TokenGroupItem : TokenItemBase
	{
		readonly string _name;
		readonly Type _groupType;
		readonly TokenMember _member;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenGroupItem"/> class
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="member">The member.</param>
		/// <param name="valueType">Type of the value.</param>
		public TokenGroupItem(string name, TokenMember member, Type valueType)
			: base(valueType)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			else if(member == null)
				throw new ArgumentNullException("member");

			_name = name;
			_groupType = valueType ?? member.DataType;
			_member = member;

			ConstructorInfo ci = _groupType.GetConstructor(Type.EmptyTypes);
			if (ci == null || !ci.IsPublic || _groupType.IsAbstract)
				throw new ArgumentException(string.Format(TokenizerMessages.CantUseTypeXAsTokenGroupBecauseItHasNoPublicParameterlessConstructor, valueType.FullName), "memberType");

			if (valueType != null && !_member.DataType.IsAssignableFrom(valueType))
				throw new ArgumentException("valueType must be assignable to datatype of member", "valueType");
		}

		/// <summary>
		/// Gets the aliases.
		/// </summary>
		/// <value>The aliases.</value>
		public string[] Aliases
		{
			get { return new string[] { _name }; }
		}

		internal bool TryParseXml(System.Xml.XPath.XPathNavigator nav, TokenizerArgs args, out object value)
		{
			Type xmlTokenizer = typeof(XmlTokenizer<>).MakeGenericType(_groupType);

			object[] arguments = new object[] { nav, args, null };

			bool ok = (bool)xmlTokenizer.InvokeMember("TryParse", BindingFlags.Static | BindingFlags.InvokeMethod |BindingFlags.NonPublic, null, null, arguments);

			value = arguments[2];

			return ok;
		}

		internal bool TryWriteXml(XmlWriter writer, TokenizerArgs args, object value)
		{
			Type xmlTokenizer = typeof(XmlTokenizer<>).MakeGenericType(_groupType);

			MethodInfo[] mi = xmlTokenizer.GetMethods(BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
			object[] arguments = new object[] { writer, value, args };

			return (bool)xmlTokenizer.InvokeMember("TryWrite", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, null, arguments);
		}

		/// <summary>
		/// Gets the member.
		/// </summary>
		/// <value>The member.</value>
		protected internal TokenMember Member
		{
			get { return _member; }
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return _name; }
		}
	}
}
