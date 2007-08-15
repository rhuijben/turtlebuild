using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using QQn.TurtleUtils.Tokenizer.Tokenizers;

namespace QQn.TurtleUtils.Tokenizer.Definitions
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
		/// Initializes a new instance of the <see cref="TokenGroupItem"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="memberType">Type of the member.</param>
		/// <param name="member">The member.</param>
		public TokenGroupItem(string name, Type memberType, TokenMember member)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			else if(member == null)
				throw new ArgumentNullException("member");

			_name = name;
			_groupType = memberType ?? member.DataType;
			_member = member;

			ConstructorInfo ci = _groupType.GetConstructor(Type.EmptyTypes);
			if (ci == null || !ci.IsPublic || _groupType.IsAbstract)
				throw new ArgumentException(string.Format(TokenizerMessages.CantUseTypeXAsTokenGroupBecauseItHasNoPublicParameterlessConstructor, memberType.FullName), "memberType");			
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

			bool ok = (bool)xmlTokenizer.InvokeMember("TryParse", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, null, arguments);

			value = arguments[2];

			return ok;
		}

		/// <summary>
		/// Gets the member.
		/// </summary>
		/// <value>The member.</value>
		protected internal TokenMember Member
		{
			get { return _member; }
		}
	}
}
