using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;

namespace QQn.TurtleUtils.Tokenizer.Definitions
{
	public enum TokenMemberMode
	{
		Default,
		Array,
		List
	}
	public sealed class TokenMember : TokenItemBase
	{
		readonly MemberInfo _member;
		readonly string _name;
		readonly IList<TokenItem> _tokens;
		readonly bool _isProperty;

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

		public string Name
		{
			get { return _name; }
		}

		public IList<TokenItem> Tokens
		{
			get { return _tokens; }
		}

		Type _fieldType;
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
