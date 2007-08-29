using System;
using System.Collections.Generic;

namespace QQn.TurtleUtils.Tokens
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(BaseTokenAttribute.TokenTargets, AllowMultiple=false)]
	public sealed class TokenDescriptionAttribute : BaseTokenAttribute
	{
		readonly string _description;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenDescriptionAttribute"/> class.
		/// </summary>
		/// <param name="description">The description.</param>
		public TokenDescriptionAttribute(string description)
		{
			_description = description;
		}

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>The description.</value>
		public string Description
		{
			get { return _description ?? ""; }
		} 
	}
}
