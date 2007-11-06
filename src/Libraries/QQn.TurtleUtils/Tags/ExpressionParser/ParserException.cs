using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class ParserException : ExpressionException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LexerException"/> class.
		/// </summary>
		public ParserException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LexerException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="expression">The expression.</param>
		/// <param name="position">The position.</param>
		internal ParserException(string message, TagToken token, ParserState state)			
			: base(string.Format("{0} while parsing '{1}'", token))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ParserException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="state">The state.</param>
		internal ParserException(string message, ParserState state)
			: base(string.Format("{0} while parsing '{1}'", state.Expression))
		{
		}
	}
}
