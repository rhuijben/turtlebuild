using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleBuildUtils.AttributeParsers
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class AttributeParser<T> : AttributeParser
		where T : AttributeParser<T>, new()
	{
		static T _instance;

		/// <summary>
		/// Gets the default instance of this class.
		/// </summary>
		/// <value>The default.</value>
		protected internal static T Default
		{
			get { return _instance ?? (_instance = new T()); }
		}

		/// <summary>
		/// Gets a boolean indicating whether the parser can handle the specified fileextension
		/// </summary>
		/// <param name="extension">The extension.</param>
		/// <returns>true if the parser knows how to handle the extension</returns>
		public abstract bool HandlesExtension(string extension);
	}
}
