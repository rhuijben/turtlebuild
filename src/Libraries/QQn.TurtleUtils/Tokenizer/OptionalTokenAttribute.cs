using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tokenizer
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(TokenAttributeBase.TokenTargets, AllowMultiple=false)]
	public sealed class OptionalTokenAttribute : TokenAttributeBase
	{
		bool _notOptional;

		/// <summary>
		/// 
		/// </summary>
		public OptionalTokenAttribute()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="optional"></param>
		public OptionalTokenAttribute(bool optional)
		{
			_notOptional = !optional;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool Optional
		{
			get { return !_notOptional; }
		}
	}
}
