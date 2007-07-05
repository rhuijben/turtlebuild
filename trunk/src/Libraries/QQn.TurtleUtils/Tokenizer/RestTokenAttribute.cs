using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tokenizer
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(TokenAttributeBase.TokenTargets, AllowMultiple=false)]
	public sealed class RestTokenAttribute : PositionTokenAttribute
	{
		/// <summary>
		/// 
		/// </summary>
		public RestTokenAttribute()
			: base(int.MaxValue)
		{
		}
	}
}
