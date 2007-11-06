using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	enum TokenType
	{
		Zero=0,

		
		// Skip characters
		LastCharacter = Char.MaxValue,
		/// <summary>
		/// &lt;=
		/// </summary>
		IsLte,
		/// <summary>
		/// &gt;=
		/// </summary>
		IsGte,
		/// <summary>
		/// !=
		/// </summary>
		IsNot,
		/// <summary>
		/// ==
		/// </summary>
		IsEqual,

		/// <summary>
		/// and
		/// </summary>
		And,
		/// <summary>
		/// or
		/// </summary>
		Or,

		/// <summary>
		/// Exists (followed by '(')
		/// </summary>
		Function,

		/// <summary>
		/// true
		/// </summary>
		Literal,

		/// <summary>
		/// 'apple'
		/// </summary>
		String,

		/// <summary>
		/// 12.44
		/// </summary>
		Number,

		/// <summary>
		/// %(aaaaa)
		/// </summary>
		Tag,

		/// <summary>
		/// $(aaaaa)
		/// </summary>
		Property
	}

	class Token
	{
		readonly TokenType _type;
		int _offset;
		int _len;
		string _value;

		public Token(TokenType type, int position, int len, string value)
		{
			_type = type;
			_offset = position;
			_len = len;
			_value = value;
		}

		public Token(char token, int offset)
		{
			_type = (TokenType)token;
			_offset = offset;
		}

		public override string ToString()
		{
			return Value;
		}

		public string Value
		{
			get
			{
				return _value ?? ((_type <= TokenType.LastCharacter) ? ((char)_type).ToString() : _type.ToString());
			}
		}
	}
}
