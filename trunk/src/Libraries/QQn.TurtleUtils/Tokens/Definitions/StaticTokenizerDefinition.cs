using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace QQn.TurtleUtils.Tokens.Definitions
{
	sealed class StaticTokenizerDefinition<T> : TokenizerDefinition
		where T : class, new()
	{
		public StaticTokenizerDefinition()
		{
			foreach (MemberInfo mi in typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance))
			{
				if ((mi.MemberType != MemberTypes.Field) && (mi.MemberType != MemberTypes.Property))
					continue;

				if(null == GetFirstAttribute<TokenAttributeBase>(mi))
					continue;

				TokenMember token = new TokenMember(mi);
				_tokens.Add(token.Name, token);

				foreach (PositionTokenAttribute pos in mi.GetCustomAttributes(typeof(PositionTokenAttribute), true))
				{
					if(pos is RestTokenAttribute)
						SetRest(pos.CreateToken(token));
					else
						AddPlaced(pos.Position, pos.CreateToken(token));
				}
			}

			Validate();
		}


		static StaticTokenizerDefinition<T> _definition;
		/// <summary>
		/// Gets the definition.
		/// </summary>
		/// <value>The definition.</value>
		public static StaticTokenizerDefinition<T> Definition
		{
			get
			{
				if (_definition == null)
					_definition = new StaticTokenizerDefinition<T>();

				return _definition;
			}
		}
	}
}
