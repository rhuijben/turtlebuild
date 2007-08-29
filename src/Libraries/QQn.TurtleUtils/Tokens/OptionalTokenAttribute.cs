using System;
using System.Collections.Generic;

namespace QQn.TurtleUtils.Tokens
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(BaseTokenAttribute.TokenTargets, AllowMultiple=false)]
	public sealed class OptionalTokenAttribute : BaseTokenAttribute
	{
		bool _notOptional;

		/// <summary>
		/// Initializes a new instance of the <see cref="OptionalTokenAttribute"/> class.
		/// </summary>
		public OptionalTokenAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OptionalTokenAttribute"/> class.
		/// </summary>
		/// <param name="optional">if set to <c>true</c> [optional].</param>
		public OptionalTokenAttribute(bool optional)
		{
			_notOptional = !optional;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="OptionalTokenAttribute"/> is optional.
		/// </summary>
		/// <value><c>true</c> if optional; otherwise, <c>false</c>.</value>
		public bool Optional
		{
			get { return !_notOptional; }
		}
	}
}
