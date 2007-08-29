using System;
using System.Collections.Generic;

namespace QQn.TurtleUtils
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class TokenAttributeBase : Attribute
	{
		internal const AttributeTargets TokenTargets = AttributeTargets.Field | AttributeTargets.Property;

		/// <summary>
		/// When overridden initializes a new instance of the <see cref="TokenAttributeBase"/> class.
		/// </summary>
		protected TokenAttributeBase()
		{
		}
	}
}
