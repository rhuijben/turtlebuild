using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Scope = "type", Target = "QQn.TurtleUtils.Tokens.PositionTokenAttribute")]

namespace QQn.TurtleUtils.Tokens
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(BaseTokenAttribute.TokenTargets, AllowMultiple=true)]
	public class PositionTokenAttribute : TokenAttribute
	{
		readonly int _position;

		/// <summary>
		/// Initializes a new instance of the <see cref="PositionTokenAttribute"/> class.
		/// </summary>
		/// <param name="position">The position.</param>
		public PositionTokenAttribute(int position)
		{
			if (position < 0)
				throw new ArgumentOutOfRangeException("position", position, "Position must be at least 0");

			_position = position;
		}

		/// <summary>
		/// Gets the position.
		/// </summary>
		/// <value>The position.</value>
		public int Position
		{
			get { return _position; }
		} 
	}
}
