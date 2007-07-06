using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace QQn.TurtleUtils.Tokenizer.Definitions
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TokenizerState<T> : Hashtable
		where T : class, new()
	{
		readonly T _instance;
		readonly TokenizerDefinition _definition;
		readonly TokenizerArgs _args;		

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenizerState&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <param name="definition">The definition.</param>
		/// <param name="args">The args.</param>
		public TokenizerState(T instance, TokenizerDefinition definition, TokenizerArgs args)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
			else if (definition == null)
				throw new ArgumentNullException("definition");
			else if (args == null)
				throw new ArgumentNullException("args");

			_instance = instance;
			_definition = definition;
			_args = args;
		}

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public T Instance
		{
			get { return _instance; }
		}

		/// <summary>
		/// Gets the definition.
		/// </summary>
		/// <value>The definition.</value>
		public TokenizerDefinition Definition
		{
			get { return _definition; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is complete.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is complete; otherwise, <c>false</c>.
		/// </value>
		public bool IsComplete
		{
			get
			{
				// TODO: Verify required arguments

				return true;
			}
		}

		/// <summary>
		/// Gets the tokenizer args.
		/// </summary>
		/// <value>The tokenizer args.</value>
		public TokenizerArgs TokenizerArgs
		{
			get { return _args; }
		} 
	}
}
