using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class ParserArgs
	{ 
		bool _allowProperties;
		bool _allowTags;
		bool _allowItems;
		bool _applyAndOr;

		/// <summary>
		/// Gets or sets a value indicating whether to allow properties.
		/// </summary>
		/// <value><c>true</c> if [allow properties]; otherwise, <c>false</c>.</value>
		public bool AllowProperties
		{
			get { return _allowProperties; }
			set { _allowProperties = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to allow tags.
		/// </summary>
		/// <value><c>true</c> if [allow tags]; otherwise, <c>false</c>.</value>
		public bool AllowTags
		{
			get { return _allowTags; }
			set { _allowTags = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether [allow items].
		/// </summary>
		/// <value><c>true</c> if [allow items]; otherwise, <c>false</c>.</value>
		public bool AllowItems
		{
			get { return _allowItems; }
			set { _allowItems = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to apply and-or priority.
		/// </summary>
		/// <value><c>true</c> if and-or priority should be applied; otherwise, <c>false</c>.</value>
		/// <remarks>MSBuild does not support and-or priority resolving. This value should be set to false to stay compatible</remarks>
		public bool ApplyAndOrPriority
		{
			get { return _applyAndOr; }
			set { _applyAndOr = value; }
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns>A <see cref="ParserArgs"/> instance with all settings copied</returns>
		public ParserArgs Clone()
		{
			ParserArgs a = (ParserArgs)MemberwiseClone();
			return a;
		}
	}
}
