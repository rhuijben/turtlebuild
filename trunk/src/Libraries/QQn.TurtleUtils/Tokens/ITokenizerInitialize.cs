using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tokens
{
	/// <summary>
	/// 
	/// </summary>
	public class TokenizerEventArgs : EventArgs
	{
		object _context;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenizerEventArgs"/> class.
		/// </summary>
		public TokenizerEventArgs()
		{
		}

		/// <summary>
		/// Gets or sets the tokenizer Context
		/// </summary>
		/// <value>The context.</value>
		public object Context
		{
			get { return _context; }
			set { _context = value; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public interface ITokenizerInitialize
	{
		/// <summary>
		/// Called when initialization via the tokenizer starts
		/// </summary>
		/// <param name="e">The <see cref="QQn.TurtleUtils.Tokens.TokenizerEventArgs"/> instance containing the event data.</param>
		void BeginInitialize(TokenizerEventArgs e);

		/// <summary>
		/// Called when initialization via the tokenizer completed
		/// </summary>
		/// <param name="e">The <see cref="QQn.TurtleUtils.Tokens.TokenizerEventArgs"/> instance containing the event data.</param>
		void EndInitialize(TokenizerEventArgs e);
	}
}
