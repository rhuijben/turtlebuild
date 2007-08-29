using System;
using System.Collections.Generic;
using System.Reflection;

namespace QQn.TurtleUtils.Tokens.Definitions
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class TokenItemBase
	{
		readonly Type _valueType;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenItemBase"/> class.
		/// </summary>
		/// <param name="valueType">Type of the value.</param>
		protected TokenItemBase(Type valueType)
		{
			_valueType = valueType;
		}

		internal static T GetFirstAttribute<T>(ICustomAttributeProvider attributeProvider)
			where T : Attribute
		{
			if (attributeProvider == null)
				throw new ArgumentNullException("attributeProvider");

			foreach (T a in attributeProvider.GetCustomAttributes(typeof(T), true))
			{
				return a;
			}

			return null;
		}

		int _typeLevel;
		internal int TypeLevel
		{
			get
			{
				if (_typeLevel == 0)
				{
					_typeLevel = 1 + GetTypeLevel(ValueType);
				}

				return _typeLevel - 1;
			}
		}
		static int GetTypeLevel(Type type)
		{
			if (type == null)
				return 0;

			return 1 + GetTypeLevel(type.BaseType);
		}

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		/// <value>The type of the value.</value>
		public Type ValueType
		{
			get { return _valueType; }
		}
	}
}
