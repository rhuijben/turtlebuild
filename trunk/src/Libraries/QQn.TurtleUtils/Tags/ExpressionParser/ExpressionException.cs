using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	[Serializable]
	public class ExpressionException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionException"/> class.
		/// </summary>
		public ExpressionException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public ExpressionException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public ExpressionException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
