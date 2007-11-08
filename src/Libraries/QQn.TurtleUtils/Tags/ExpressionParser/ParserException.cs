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
		/// Initializes a new instance of the <see cref="ParserException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public ParserException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LexerException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="token">The token.</param>
		/// <param name="state">The state.</param>
		internal ParserException(string message, TagToken token, ParserState state)			
			: base(string.Format("{0} while parsing '{1}'", message, token))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ParserException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="state">The state.</param>
		internal ParserException(string message, ParserState state)
			: base(string.Format("{0} while parsing '{1}'", message, state.Expression))
		{
		}
	}

	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class PriorityException : ParserException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LexerException"/> class.
		/// </summary>
		public PriorityException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PriorityException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public PriorityException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LexerException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="token">The token.</param>
		/// <param name="state">The state.</param>
		internal PriorityException(string message, TagToken token, ParserState state)			
			: base(message, token, state)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ParserException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="state">The state.</param>
		internal PriorityException(string message, ParserState state)
			: base(message, state)
		{
		}
	}
}
