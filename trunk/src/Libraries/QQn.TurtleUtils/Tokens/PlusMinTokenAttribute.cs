using System;
using System.Collections.Generic;
using QQn.TurtleUtils.Tokens.Definitions;

namespace QQn.TurtleUtils.Tokens
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(TokenAttributeBase.TokenTargets, AllowMultiple = true)]
	public class PlusMinTokenAttribute : TokenAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PlusMinTokenAttribute"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public PlusMinTokenAttribute(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Creates a <see cref="TokenItem"/> instance.
		/// </summary>
		/// <param name="tokenMember">The token member.</param>
		/// <returns></returns>
		public override QQn.TurtleUtils.Tokens.Definitions.TokenItem CreateToken(QQn.TurtleUtils.Tokens.Definitions.TokenMember tokenMember)
		{
			return new PlusMinToken(tokenMember, this, ValueType);
		}

		class PlusMinToken : TokenItem
		{
			public PlusMinToken(TokenMember member, TokenAttribute attr, Type valueType)
				: base(member, attr, valueType)
			{
			}
		}
	}
}
