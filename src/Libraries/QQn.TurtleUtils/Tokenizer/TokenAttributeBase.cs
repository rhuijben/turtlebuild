using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class TokenAttributeBase : Attribute
	{
		internal const AttributeTargets TokenTargets = AttributeTargets.Field | AttributeTargets.Property;

		/// <summary>
		/// 
		/// </summary>
		protected TokenAttributeBase()
		{
		}
	}
}
