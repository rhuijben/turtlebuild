using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokenizer.Definitions;

namespace QQn.TurtleUtils.Tokenizer
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(TokenAttributeBase.TokenTargets, AllowMultiple = true)]
	public class PlusMinTokenAttribute : TokenAttribute
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		public PlusMinTokenAttribute(string name)
			: base(name)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tokenMember"></param>
		/// <returns></returns>
		public override QQn.TurtleUtils.Tokenizer.Definitions.TokenItem CreateToken(QQn.TurtleUtils.Tokenizer.Definitions.TokenMember tokenMember)
		{
			return new PlusMinToken(tokenMember, this);
		}

		class PlusMinToken : TokenItem
		{
			public PlusMinToken(TokenMember member, TokenAttribute attr)
				: base(member, attr)
			{
			}
		}
	}
}
