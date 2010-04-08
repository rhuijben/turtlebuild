using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace QQn.TurtleUtils.Tokens.Definitions
{
	/// <summary>
	/// Specifies the way the tokenizer uses a member
	/// </summary>
	public enum TokenMemberMode
	{
		/// <summary>
		/// Sets value
		/// </summary>
		Default,
		/// <summary>
		/// Extends an array
		/// </summary>
		Array,
		/// <summary>
		/// Appends to an <see cref="System.Collections.IList"/>
		/// </summary>
		List,
		/// <summary>
		/// Appends to an <see cref="System.Collections.Generic.IList&lt;T&gt;"/>
		/// </summary>
		GenericList
	}

	/// <summary>
	/// 
	/// </summary>
	public sealed class TokenMember : TokenItemBase
	{
		readonly MemberInfo _member;
		readonly string _name;
		readonly IList<TokenItem> _tokens;
		readonly IList<TokenGroupItem> _groups;
		readonly bool _isProperty;
		readonly Type _defaultTypeConverter;

		static readonly IList<TokenItem> _EmptyTokensList = new TokenItem[0];
		static readonly IList<TokenGroupItem> _EmptyGroupList = new TokenGroupItem[0];

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenMember"/> class.
		/// </summary>
		/// <param name="member">The member.</param>
		public TokenMember(MemberInfo member)
			: base(null)
		{
			if (member == null)
				throw new ArgumentNullException("member");

			_member = member;
			_name = member.Name;
			_isProperty = member is PropertyInfo;

			TypeConverterAttribute tca = GetFirstAttribute<TypeConverterAttribute>(member);
			if(tca != null)
				_defaultTypeConverter = Type.GetType(tca.ConverterTypeName, false);

			List<TokenItem> tokens = null;
			foreach (TokenAttribute a in member.GetCustomAttributes(typeof(TokenAttribute), true))
			{
				if (a.Name == null)
					continue; // Position token
				if (tokens == null)
					tokens = new List<TokenItem>();

				tokens.Add(a.CreateToken(this));
			}

			List<TokenGroupItem> groups = null;
			foreach (TokenGroupAttribute a in member.GetCustomAttributes(typeof(TokenGroupAttribute), true))
			{
				if (groups == null)
					groups = new List<TokenGroupItem>();

				groups.Add(a.CreateGroup(this));
			}

			if (groups != null)
			{
				// Reverse sort the items on typelevel
				groups.Sort(delegate(TokenGroupItem x, TokenGroupItem y)
				{
					return x.TypeLevel - y.TypeLevel;
				});
			}

			if (tokens != null)
				_tokens = tokens.AsReadOnly();
			else
				_tokens = _EmptyTokensList;

			if (groups != null)
				_groups = groups.AsReadOnly();
			else
				_groups = _EmptyGroupList;
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
		/// Gets the tokens.
		/// </summary>
		/// <value>The tokens.</value>
		public IList<TokenItem> Tokens
		{
			get { return _tokens; }
		}

		/// <summary>
		/// Gets the tokens.
		/// </summary>
		/// <value>The tokens.</value>
		public IList<TokenGroupItem> Groups
		{
			get { return _groups; }
		}

		Type _fieldType;
		/// <summary>
		/// Gets the type of the field.
		/// </summary>
		/// <value>The type of the field.</value>
		public Type FieldType
		{
			get
			{
				if (_fieldType == null)
				{
					FieldInfo field = _member as FieldInfo;
					if (field != null)
						_fieldType = field.FieldType;
					else
					{
						PropertyInfo property = _member as PropertyInfo;
						_fieldType = property.PropertyType;
					}
				}
				return _fieldType;
			}
		}

		Type _dataType;
		Type _listType;
		TokenMemberMode _tokenMemberMode;
		/// <summary>
		/// Gets the type of the data.
		/// </summary>
		/// <value>The type of the data.</value>
		public Type DataType
		{
			get
			{
				if (_dataType == null)
				{
					Type fieldType = FieldType;
					_tokenMemberMode = TokenMemberMode.Default;

					if (typeof(Array).IsAssignableFrom(fieldType))
					{
						fieldType = fieldType.GetElementType();
						_tokenMemberMode = TokenMemberMode.Array;
					}
					else 
						foreach (Type iff in fieldType.GetInterfaces())
						{
							if (iff == typeof(IList))
							{
								MethodInfo m = fieldType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

								if (m != null)
								{
									ParameterInfo[] pi = m.GetParameters();

									if (pi.Length == 1)
									{
										fieldType = pi[0].ParameterType;
										_tokenMemberMode = TokenMemberMode.List;
										_listType = typeof(ICollection);
										break;
									}
								}
							}
							else if (iff.IsGenericType)
							{
								Type genericType = iff.GetGenericTypeDefinition();

								if (genericType == typeof(ICollection<>))
								{
									fieldType = iff.GetGenericArguments()[0];
									_tokenMemberMode = TokenMemberMode.GenericList;
									_listType = iff;
									break;
								}
							}
						}

					Type nullableBase = Nullable.GetUnderlyingType(fieldType);

					if (nullableBase != null)
					{
						// The .NET reflection api hides all nullable instances for the reflection code
						// (No need to wrap and unwrap boxed types)
						// See http://blogs.msdn.com/haibo_luo/archive/2005/08/23/455241.aspx for more information
						//
						// Before RTM there existed a method Nullable.Wrap<T>(T value) which we should have used all over the place;
						// now we only need to unwrap the type to allow using TypeConverters... on the real type
						//
						fieldType = nullableBase;						
					}

					_dataType = fieldType;
				}
				return _dataType;
			}
		}

		/// <summary>
		/// Gets the token member mode.
		/// </summary>
		/// <value>The token member mode.</value>
		public TokenMemberMode TokenMemberMode
		{
			get
			{
				if (DataType != null)
					return _tokenMemberMode;
				else
					return TokenMemberMode.Default;
			}
		}

		/// <summary>
		/// Sets the value of the token.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="state">The state.</param>
		/// <param name="value">The value.</param>
		public void SetValue<T>(TokenizerState<T> state, object value)
			where T : class, new()
		{
			if (value != null && !DataType.IsAssignableFrom(value.GetType()))
				throw new ArgumentException("Invalid value", "value");

			switch (TokenMemberMode)
			{
				case TokenMemberMode.Default:
					SetMemberValue(state, value);
					break;
				case TokenMemberMode.Array:
					{
						Array a = (Array)GetMemberValue(state);
						Array aNew;

						if (a != null)
						{
							aNew = Array.CreateInstance(FieldType.GetElementType(), a.Length + 1);
							if (a.Length >= 1)
								a.CopyTo(aNew, 0);
						}
						else
							aNew = Array.CreateInstance(FieldType.GetElementType(), 1);

						aNew.SetValue(value, aNew.Length - 1);

						SetMemberValue(state,aNew);
						break;
					}
				case TokenMemberMode.List:
				case TokenMemberMode.GenericList:
					{
						object l = GetMemberValue(state);

						if (l == null)
						{
							l = Activator.CreateInstance(FieldType);

							SetMemberValue(state, l);
						}

						if (TokenMemberMode == TokenMemberMode.List)
						{
							IList list = (IList)l;

							list.Add(value);
						}
						else
						{
							_listType.InvokeMember("Add", BindingFlags.InvokeMethod, null, l, new object[] { value }, CultureInfo.InvariantCulture);
						}
						break;
					}
				default:
					throw new InvalidOperationException();
			}
		}

		object GetMemberValue<T>(TokenizerState<T> state)
			where T : class, new()
		{
			return typeof(T).InvokeMember(Name, _isProperty ? BindingFlags.GetProperty : BindingFlags.GetField, null, state.Instance, null, CultureInfo.InvariantCulture);
		}

		void SetMemberValue<T>(TokenizerState<T> state, object value)
			where T : class, new()
		{
			typeof(T).InvokeMember(Name, _isProperty ? BindingFlags.SetProperty : BindingFlags.SetField, null, state.Instance, new object[] { value }, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Gets a list with the values of this field or <c>null</c> if the value has the value specified in the <see cref="DefaultValueAttribute"/> attribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="state">The state.</param>
		/// <returns></returns>
		public object[] GetValues<T>(TokenizerState<T> state)
			where T : class, new()
		{
			object result = GetMemberValue(state);

			switch (TokenMemberMode)
			{
				case TokenMemberMode.Default:
					DefaultValueAttribute dva = GetFirstAttribute<DefaultValueAttribute>(_member);

					if (dva != null && dva.Value != null && dva.Value.Equals(result))
						return null;

					return new object[] { result };
				case TokenMemberMode.Array:
				case TokenMemberMode.List:
				case TokenMemberMode.GenericList:
					
					List<object> items = new List<object>();
					if(result != null)
						foreach (object i in (IEnumerable)result)
						{
							items.Add(i);
						}
					return items.ToArray();
				default:
					throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Gets the default type converter.
		/// </summary>
		/// <value>The default type converter.</value>
		public Type DefaultTypeConverter
		{
			get { return _defaultTypeConverter; }
		}
	}
}
