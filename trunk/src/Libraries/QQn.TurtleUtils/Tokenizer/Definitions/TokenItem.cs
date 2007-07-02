using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace QQn.TurtleUtils.Tokenizer.Definitions
{
	public class TokenItem : TokenItemBase
	{
		readonly TokenMember _member;
		readonly string _name;
		readonly IList<string> _aliases;
		readonly Type _typeConverter;

		public TokenItem(TokenMember member, TokenAttribute attr)
		{
			if (member == null)
				throw new ArgumentNullException("member");

			_name = attr.Name;
			_aliases = attr.Aliases;

			_member = member;						
			_typeConverter = attr.TypeConverter;
		}

		public TokenMember Member
		{
			get { return _member; }
		}

		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// Token allows a direct value ('-v1' vs  '-v' | '-v 12' | '-v=12')
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool AllowDirectValue(string value)
		{
			return false;
		}

		/// <summary>
		/// Token requires a value ('-source banana' | (if <see cref="AcceptsValue"/>) '-source=banana')
		/// </summary>
		public virtual bool RequiresValue
		{
			get { return (Member.DataType != typeof(Boolean)); }
		}

		/// <summary>
		/// Token allows a value ('-source=banana')
		/// </summary>
		public bool AcceptsValue
		{
			get { return true; }
		}

		/// <summary>
		/// Token accepts a [+|-] suffix
		/// </summary>
		public virtual bool AllowPlusMinSuffix
		{
			get { return (DataType == typeof(Boolean)); }
		}

		public IList<string> Aliases
		{
			get { return _aliases ?? new string[0]; }
		}

		public Type DataType
		{
			get { return Member.DataType; }
		}

		TypeConverter _typeConverterInstance;
		protected virtual TypeConverter TypeConverter
		{
			get 
			{
				if (_typeConverterInstance != null)
					_typeConverterInstance = (TypeConverter)Activator.CreateInstance(_typeConverter);

				return _typeConverterInstance;
			}
		}

		/// <summary>
		/// Converts value to the <see cref="DataType"/> Type
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual object ConvertValue(string value)
		{
			if(typeof(string) == DataType)
				return value;
			else if (value == null)
				throw new ArgumentNullException("value");
			
			TypeConverter tc = TypeConverter;

			if(tc == null)
				tc = TypeDescriptor.GetConverter(DataType);

			object v = tc.ConvertFromString(value);
			
			if((v != null) && DataType.IsAssignableFrom(v.GetType()))
				return v;

			throw new InvalidOperationException(string.Format("The typeconverter of type {0} (A {1} instance) can't convert a string into a {0}", DataType.FullName, tc.GetType().FullName, DataType.Name));
		}

		public virtual void Evaluate<T>(string value, TokenizerState<T> state)
			where T : class, new()
		{
			Type dataType = DataType;

			if ((value == null) && dataType != typeof(Boolean))
				throw new ArgumentNullException("value", "Value must be set for non-booleans");
			else if (value == null)
				value = bool.TrueString;

			object RawValue = ConvertValue(value);

			Member.SetValue(state, RawValue);
		}

		internal void EvaluateDirect(string value)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}
	}
}
