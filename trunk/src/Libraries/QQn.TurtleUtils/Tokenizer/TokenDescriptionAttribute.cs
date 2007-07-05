using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tokenizer
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(TokenAttributeBase.TokenTargets, AllowMultiple=false)]
	public sealed class TokenDescriptionAttribute : TokenAttributeBase
	{
		readonly string _description;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="description"></param>
		public TokenDescriptionAttribute(string description)
		{
			_description = description;
		}

		/// <summary>
		/// 
		/// </summary>
		public string Description
		{
			get { return _description ?? ""; }
		} 
	}
}
