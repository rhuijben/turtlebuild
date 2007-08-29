using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;


[module: SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Scope = "type", Target = "QQn.TurtleUtils.Tokens.ITokenizerExpandCollection")]

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
		void OnBeginInitialize(TokenizerEventArgs e);

		/// <summary>
		/// Called when initialization via the tokenizer completed
		/// </summary>
		/// <param name="e">The <see cref="QQn.TurtleUtils.Tokens.TokenizerEventArgs"/> instance containing the event data.</param>
		void OnEndInitialize(TokenizerEventArgs e);
	}

	/// <summary>
	/// Marker interface for tokenizer values which should be expanded while tokenizing
	/// </summary>
	public interface ITokenizerExpandCollection : System.Collections.IEnumerable
	{
	}
}
