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

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenAttribute"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="aliases">The aliases.</param>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenAttribute"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public TokenAttribute(string name)
			: this(name, (string[])null)
		{
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenAttribute"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="alias">The alias.</param>
		public TokenAttribute(string name, string alias)
			: this(name, new string[] { alias })
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenAttribute"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="alias1">The alias1.</param>
		/// <param name="alias2">The alias2.</param>
		public TokenAttribute(string name, string alias1, string alias2)
			: this(name, new string[] { alias1, alias2 })
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenAttribute"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="alias1">The alias1.</param>
		/// <param name="alias2">The alias2.</param>
		/// <param name="alias3">The alias3.</param>
		public TokenAttribute(string name, string alias1, string alias2, string alias3)
			: this(name, new string[] { alias1, alias2, alias3 })
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenAttribute"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="alias1">The alias1.</param>
		/// <param name="alias2">The alias2.</param>
		/// <param name="alias3">The alias3.</param>
		/// <param name="alias4">The alias4.</param>
		public TokenAttribute(string name, string alias1, string alias2, string alias3, string alias4)
			: this(name, new string[] { alias1, alias2, alias3, alias4 })
		{

		}

		/// <summary>
		/// 
		/// </summary>
		protected TokenAttribute()
		{
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
		/// Gets the aliases.
		/// </summary>
		/// <value>The aliases.</value>
		public IList<string> Aliases
		{
			get { return _aliases; }
		}

		/// <summary>
		/// Gets or sets the type converter.
		/// </summary>
		/// <value>The type converter.</value>
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

		/// <summary>
		/// Creates a <see cref="TokenItem"/> instance.
		/// </summary>
		/// <param name="tokenMember">The token member.</param>
		/// <returns></returns>
		public virtual TokenItem CreateToken(TokenMember tokenMember)
		{
			return new TokenItem(tokenMember, this);
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="TokenAttribute"/> is required.
		/// </summary>
		/// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
		public bool Required
		{
			get { return _required; }
			set { _required = value; }
		}
	}
}
