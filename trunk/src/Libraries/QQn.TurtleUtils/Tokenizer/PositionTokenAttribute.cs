using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tokenizer
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(TokenAttributeBase.TokenTargets, AllowMultiple=true)]
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
