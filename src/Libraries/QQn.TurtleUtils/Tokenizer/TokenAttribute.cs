using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using QQn.TurtleUtils.Tokenizer.Definitions;

namespace QQn.TurtleUtils.Tokenizer
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(TokenAttributeBase.TokenTargets, AllowMultiple=true)]
	public class TokenAttribute : TokenAttributeBase
	{
		readonly string _name;
		IList<string> _aliases;
		Type _typeConverter;
		bool _required;
		
		[CLSCompliant(false)]
		public TokenAttribute(string name, params string[] aliases)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			_name = name;
			List<string> aliasList = new List<string>();
			aliasList.Add(name);

			if (aliases != null)
			{
				foreach (string alias in aliases)
				{
					if (!string.IsNullOrEmpty(alias) && !aliasList.Contains(alias))
						aliasList.Add(alias);
				}

				aliasList.Sort(StringComparer.InvariantCultureIgnoreCase);
			}

			_aliases = aliasList.AsReadOnly();
		}

		public TokenAttribute(string name)
			: this(name, (string[])null)
		{
			
		}

		public TokenAttribute(string name, string alias)
			: this(name, new string[] { alias })
		{

		}

		public TokenAttribute(string name, string alias1, string alias2)
			: this(name, new string[] { alias1, alias2 })
		{

		}

		public TokenAttribute(string name, string alias1, string alias2, string alias3)
			: this(name, new string[] { alias1, alias2, alias3 })
		{

		}

		public TokenAttribute(string name, string alias1, string alias2, string alias3, string alias4)
			: this(name, new string[] { alias1, alias2, alias3, alias4 })
		{

		}


		protected TokenAttribute()
		{
		}

		public string Name
		{
			get { return _name; }
		}

		public IList<string> Aliases
		{
			get { return _aliases; }
		}

		public Type TypeConverter
		{
			get { return _typeConverter; }
			set
			{
				if (value == null)
					_typeConverter = null;
				else if (typeof(TypeConverter).IsAssignableFrom(value))
					_typeConverter = value;
				else
					throw new ArgumentException("Typeconverter is no valid typeconverter");
			}
		}

		public virtual TokenItem CreateToken(TokenMember tokenMember)
		{
			return new TokenItem(tokenMember, this);
		}

		public bool Required
		{
			get { return _required; }
			set { _required = value; }
		}
	}
}
