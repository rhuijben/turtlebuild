using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	sealed class NumberExpression : TagExpression
	{
		ExValue _value;
		public NumberExpression(TagToken token)
			: base(token)
		{
			string tv = token.Value;
			if(tv.IndexOfAny(new char[] { '.', 'E', 'e' }) >= 0)
				_value = new ExValue(double.Parse(tv));
			else if(tv.StartsWith("0x", StringComparison.Ordinal))
				_value = new ExValue(int.Parse(tv, System.Globalization.NumberStyles.AllowHexSpecifier));
			else
				_value = new ExValue(int.Parse(tv, System.Globalization.NumberStyles.AllowLeadingSign));
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

		internal override ExValue Evaluate<TKey>(TagBatchInstance<TKey> instance)
		{
			return _value;
		}
	}
}
