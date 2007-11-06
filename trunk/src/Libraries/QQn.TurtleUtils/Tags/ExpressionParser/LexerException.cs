using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	[Serializable]
	class LexerException : Exception
	{
		public LexerException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LexerException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="expression">The expression.</param>
		/// <param name="position">The position.</param>
		public LexerException(string message, string expression, int position)			
			: base(string.Format("{0} while parsing '{1}' at position '{2}'", message, expression, position))
		{
		}

		internal LexerException(string message, ParserState state)
			: this(message, state.Expression, state.TokenStartPosition)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LexerException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public LexerException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
