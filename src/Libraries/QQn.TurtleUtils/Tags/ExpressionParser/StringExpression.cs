using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	class StringExpression : TagExpression
	{
		readonly string _value;
		public StringExpression(TagToken token)
			: base(token)
		{
			_value = token.Value;
			_value = _value.Substring(1, _value.Length - 2);
		}

		public string Value
		{
			get { return _value; }
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return Token.ToString();
		}
	}
}
