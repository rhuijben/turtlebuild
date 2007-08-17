using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tokens
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(TokenAttributeBase.TokenTargets, AllowMultiple=false)]
	public sealed class RestTokenAttribute : PositionTokenAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RestTokenAttribute"/> class.
		/// </summary>
		public RestTokenAttribute()
			: base(int.MaxValue)
		{
		}
	}
}
