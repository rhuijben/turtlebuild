using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace QQn.TurtleUtils.Tokens.Definitions
{
	/// <summary>
	/// 
	/// </summary>
	public class TokenItem : TokenItemBase
	{
		readonly TokenMember _member;
		readonly string _name;
		readonly IList<string> _aliases;
		readonly Type _typeConverter;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenItem"/> class.
		/// </summary>
		/// <param name="member">The member.</param>
		/// <param name="attr">The attr.</param>
		/// <param name="valueType">Type of the value.</param>
		public TokenItem(TokenMember member, TokenAttribute attr, Type valueType)
			: base(valueType)
		{
			if (member == null)
				throw new ArgumentNullException("member");

			_name = attr.Name;
			_aliases = attr.Aliases;

			_member = member;
			_typeConverter = attr.TypeConverter ?? member.DefaultTypeConverter;

			if (valueType != null && !_member.DataType.IsAssignableFrom(valueType))
				throw new ArgumentException("valueType must be assignable to datatype of member", "valueType");
		}

		/// <summary>
		/// Gets the member.
		/// </summary>
		/// <value>The member.</value>
		public TokenMember Member
		{
			get { return _member; }
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// Token allows a direct value ('-v1' vs  '-v' | '-v 12' | '-v=12')
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value">The value.</param>
		/// <param name="state">The state.</param>
		/// <returns></returns>
		public virtual bool AllowDirectValue<T>(string value, TokenizerState<T> state)
			where T : class, new()
		{
			if (AllowPlusMinSuffix && ((value == "+") || (value == "-")))
				return true;
			else if (AcceptsValue && (state.TokenizerArgs.DirectValueSeparator != '\0') && value[0] == state.TokenizerArgs.DirectValueSeparator)
				return true;

			return false;
		}

		/// <summary>
		/// Token requires a value ('-source banana' | (if <see cref="AcceptsValue"/>) '-source=banana')
		/// </summary>
		/// <value><c>true</c> if [requires value]; otherwise, <c>false</c>.</value>
		public virtual bool RequiresValue
		{
			get { return (Member.DataType != typeof(Boolean)); }
		}

		/// <summary>
		/// Token allows a value ('-source=banana')
		/// </summary>
		/// <value><c>true</c> if [accepts value]; otherwise, <c>false</c>.</value>
		public bool AcceptsValue
		{
			get { return true; }
		}

		/// <summary>
		/// Token accepts a [+|-] suffix
		/// </summary>
		/// <value><c>true</c> if [allow plus min suffix]; otherwise, <c>false</c>.</value>
		public virtual bool AllowPlusMinSuffix
		{
			get { return (DataType == typeof(Boolean)); }
		}

		/// <summary>
		/// Gets the aliases.
		/// </summary>
		/// <value>The aliases.</value>
		public IList<string> Aliases
		{
			get { return _aliases ?? new string[0]; }
		}

		/// <summary>
		/// Gets the type of the data.
		/// </summary>
		/// <value>The type of the data.</value>
		public Type DataType
		{
			get { return ValueType ?? Member.DataType; }
		}

		TypeConverter _typeConverterInstance;
		/// <summary>
		/// Gets the type converter.
		/// </summary>
		/// <value>The type converter.</value>
		protected virtual TypeConverter TypeConverter
		{
			get 
			{
				if (_typeConverterInstance == null)
				{
					if (_typeConverter != null)
						_typeConverterInstance = (TypeConverter)Activator.CreateInstance(_typeConverter);
					else if (typeof(FileSystemInfo).IsAssignableFrom(DataType))
					{
						if (DataType.IsAssignableFrom(typeof(FileSystemInfo)))
							_typeConverterInstance = new FileSystemInfoTypeConverter();
						else if (DataType.IsAssignableFrom(typeof(FileInfo)))
							_typeConverterInstance = new FileInfoTypeConverter();
						else if (DataType.IsAssignableFrom(typeof(DirectoryInfo)))
							_typeConverterInstance = new DirectoryInfoTypeConverter();						
					}					
				}

				return _typeConverterInstance;
			}
		}

		/// <summary>
		/// Converts value to the <see cref="DataType"/> Type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value">The value.</param>
		/// <param name="state">The state.</param>
		/// <returns></returns>
		public virtual object ConvertValue<T>(string value, TokenizerState<T> state)
			where T : class, new()
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
			else if (v is ExpandableTokenCollection) // FileSystemInfo converted star object
				return v;

			throw new InvalidOperationException(string.Format("The typeconverter of type {0} (A {1} instance) can't convert a string into a {0}", DataType.FullName, tc.GetType().FullName, DataType.Name));
		}

		internal string GetStringValue<T>(object value, TokenizerState<T> state)
			where T : class, new()
		{
			string val = value as string;

			if (val != null)
				return val;

			TypeConverter tc = TypeConverter;

			if (tc == null)
				tc = TypeDescriptor.GetConverter(DataType);

			return tc.ConvertToString(state, state.CultureInfo, value);
		}

		/// <summary>
		/// Evaluates the specified value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value">The value.</param>
		/// <param name="state">The state.</param>
		public virtual void Evaluate<T>(string value, TokenizerState<T> state)
			where T : class, new()
		{
			Type dataType = DataType;

			if ((value == null) && dataType != typeof(Boolean))
				throw new ArgumentNullException("value", "Value must be set for non-booleans");
			else if (value == null)
				value = bool.TrueString;

			object RawValue = ConvertValue(value, state);

			
			ExpandableTokenCollection expandable = RawValue as ExpandableTokenCollection;

			if (expandable != null)
			{
				foreach(object o in expandable)
				{
					Member.SetValue(state, o);
				}
			}

			Member.SetValue(state, RawValue);
		}

		/// <summary>
		/// Evaluates the value as appended after the token
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value">The value.</param>
		/// <param name="state">The state.</param>
		public virtual void EvaluateDirect<T>(string value, TokenizerState<T> state)
			where T : class, new()
		{
			if (AllowPlusMinSuffix && ((value == "+") || (value == "-")))
			{
				switch (value)
				{
					case "+":
						Evaluate("true", state);
						break;
					case "-":
						Evaluate("false", state);
						break;
				}
			}
			else if (AcceptsValue && (state.TokenizerArgs.DirectValueSeparator != '\0') && value[0] == state.TokenizerArgs.DirectValueSeparator)
			{
				Evaluate(value.Substring(1), state);
			}
			else
				throw new NotImplementedException();
		}
	}
}
