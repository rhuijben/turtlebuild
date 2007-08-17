using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens.Definitions;

namespace QQn.TurtleUtils.Tokens
{
	/// <summary>
	/// Tokenizer group
	/// </summary>
	[AttributeUsage(TokenAttributeBase.TokenTargets, AllowMultiple = true)]
	public class TokenGroupAttribute : TokenAttributeBase
	{
		readonly string _name;
		readonly Type _type;
		/// <summary>
		/// Tokens the group attribute.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public TokenGroupAttribute(string name)
		{
			if (string.IsNullOrEmpty("name"))
				throw new ArgumentNullException("name");

			_name = name;
		}

		/// <summary>
		/// Tokens the group attribute.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="groupType">Type of the group.</param>
		/// <returns></returns>
		public TokenGroupAttribute(string name, Type groupType)
		{
			if (string.IsNullOrEmpty("name"))
				throw new ArgumentNullException("name");

			_name = name;
			_type = groupType;
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// Gets the type of the group.
		/// </summary>
		/// <value>The type of the group.</value>
		public Type GroupType
		{
			get { return _type; }
		}

		/// <summary>
		/// Creates the group.
		/// </summary>
		/// <param name="tokenMember">The token member.</param>
		/// <returns></returns>
		protected internal virtual TokenGroupItem CreateGroup(TokenMember tokenMember)
		{
			return new TokenGroupItem(Name, tokenMember, GroupType);
		}
	}
}
