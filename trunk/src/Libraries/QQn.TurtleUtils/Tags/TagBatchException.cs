using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class TagBatchException : FormatException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TagBatchException"/> class.
		/// </summary>
		public TagBatchException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TagBatchException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public TagBatchException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TagBatchException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public TagBatchException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
