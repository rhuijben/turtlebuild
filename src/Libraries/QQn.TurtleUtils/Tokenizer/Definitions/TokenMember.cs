using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;

namespace QQn.TurtleUtils.Tokenizer.Definitions
{
	/// <summary>
	/// 
	/// </summary>
	public enum TokenMemberMode
	{
		/// <summary>
		/// 
		/// </summary>
		Default,
		/// <summary>
		/// 
		/// </summary>
		Array,
		/// <summary>
		/// 
		/// </summary>
		List
	}

	/// <summary>
	/// 
	/// </summary>
	public sealed class TokenMember : TokenItemBase
	{
		readonly MemberInfo _member;
		readonly string _name;
		readonly IList<TokenItem> _tokens;
		readonly bool _isProperty;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenMember"/> class.
		/// </summary>
		/// <param name="member">The member.</param>
		public TokenMember(MemberInfo member)
		{
			if (member == null)
				throw new ArgumentNullException("member");

			_member = member;
			_name = member.Name;
			_isProperty = member is PropertyInfo;

			List<TokenItem> tokens = null;
			foreach (TokenAttribute a in member.GetCustomAttributes(typeof(TokenAttribute), true))
			{
				if (a.Name == null)
					continue; // Position token
				if(tokens == null)
					tokens = new List<TokenItem>();
				
				tokens.Add(a.CreateToken(this));
			}
			if (tokens != null)
				_tokens = tokens.AsReadOnly();
			else
				_tokens = new TokenItem[0];
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
					else if (typeof(IList).IsAssignableFrom(fieldType))
					{
						MethodInfo m = fieldType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

						if (m != null)
						{
							ParameterInfo[] pi = m.GetParameters();

							if (pi.Length == 1)
							{
								fieldType = pi[0].ParameterType;
								_tokenMemberMode = TokenMemberMode.List;
							}
						}
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
			if(value != null && !DataType.IsAssignableFrom(value.GetType()))
				throw new ArgumentException("Invalid value", "value");

			switch (TokenMemberMode)
			{
				case TokenMemberMode.Default:
					typeof(T).InvokeMember(Name, _isProperty ? BindingFlags.SetProperty : BindingFlags.SetField, null, state.Instance, new object[] { value });
					break;
				case TokenMemberMode.Array:
					{
						Array a = (Array)typeof(T).InvokeMember(Name, _isProperty ? BindingFlags.GetProperty : BindingFlags.GetField, null, state.Instance, null);
						Array aNew;

						if (a != null)
						{
							aNew = Array.CreateInstance(FieldType.GetElementType(), a.Length + 1);
							if (a.Length > 1)
								a.CopyTo(aNew, 0);
						}
						else
							aNew = Array.CreateInstance(FieldType.GetElementType(), 1);

						aNew.SetValue(value, aNew.Length - 1);

						typeof(T).InvokeMember(Name, _isProperty ? BindingFlags.SetProperty : BindingFlags.SetField, null, state.Instance, new object[] { aNew });
						break;
					}
				case TokenMemberMode.List:
					{
						IList l = (IList)typeof(T).InvokeMember(Name, _isProperty ? BindingFlags.GetProperty : BindingFlags.GetField, null, state.Instance, null);

						if (l == null)
						{
							l = (IList)Activator.CreateInstance(FieldType);

							typeof(T).InvokeMember(Name, _isProperty ? BindingFlags.SetProperty : BindingFlags.SetField, null, state.Instance, new object[] { l });
						}

						l.Add(value);
						break;
					}
				default:
					throw new InvalidOperationException();
			}			
		}
	}
}
