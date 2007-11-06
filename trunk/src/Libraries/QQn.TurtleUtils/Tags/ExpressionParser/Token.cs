using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	/// <summary>
	/// 
	/// </summary>
	public enum TagTokenType
	{
		/// <summary>
		/// End of expression token
		/// </summary>
		AtEof=0,

		/// <summary>
		/// &lt;
		/// </summary>
		IsLt='<',
		/// <summary>
		/// &gt;
		/// </summary>
		IsGt = '>',

		/// <summary>
		/// ,
		/// </summary>
		Comma = ',',

		/// <summary>
		/// (
		/// </summary>
		ParenOpen = '(',
		/// <summary>
		/// )
		/// </summary>
		ParenClose = ')',

		/// <summary>
		/// !
		/// </summary>
		Not = '!',
		
		// Skip characters
		/// <summary>
		/// 
		/// </summary>
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
		Property,

		/// <summary>
		/// @(ProjectOutput)
		/// </summary>
		Item
	}

	/// <summary>
	/// 
	/// </summary>
	public class TagToken
	{
		readonly TagTokenType _type;
		int _offset;
		int _len;
		string _value;

		internal TagToken(TagTokenType type, int position, int len, string value)
		{
			_type = type;
			_offset = position;
			_len = len;
			_value = value;
		}

		internal TagToken(char token, int offset)
		{
			_type = (TagTokenType)token;
			_offset = offset;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return Value;
		}

		/// <summary>
		/// Gets the type of the token.
		/// </summary>
		/// <value>The type of the token.</value>
		public TagTokenType TokenType
		{
			get { return _type; }
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <value>The value.</value>
		public string Value
		{
			get
			{
				return _value ?? ((_type <= TagTokenType.LastCharacter) ? ((char)_type).ToString() : _type.ToString());
			}
		}
	}
}
